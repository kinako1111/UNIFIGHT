using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 目的地へ飛ぶ弾。到達 or 衝突で「一度だけ」範囲爆発（固定ダメージ）して消滅。
/// ・山なり(任意)の直進
/// ・爆発は Layer/Tag で対象をフィルタ
/// ・寿命切れ時の挙動は、爆発する/しないを選べる
/// ・Status.Damage(int) を呼び出すだけ（状態異常なし）
/// ・向き制御：オブジェクト内の2点（A→B）を向き軸として、着弾予定地点 or 進行方向へ揃える
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BulletHoming : MonoBehaviour
{
    // =======================
    // ① 基本の移動パラメータ
    // =======================
    [Header("移動設定"), SerializeField]
    private float speed = 10f;            // 直進速度

    [SerializeField, Tooltip("未使用なら 0：完全直進。>0で簡易山なり加算")]
    private float arcHeight = 0f;         // 疑似的な放物線（見た目用）

    [SerializeField, Tooltip("寿命(秒)。0以下なら無限寿命")]
    private float deathTime = 3f;         // 自動消滅までの時間

    [SerializeField, Tooltip("この距離以内なら到達とみなす")]
    private float arriveThreshold = 0.15f;// 目的地到達の判定距離

    // =======================
    // ② 爆発/トリガー条件
    // =======================
    [Header("爆発設定"), SerializeField]
    private bool explodeOnArrive = true;  // 到達で爆発するか

    [SerializeField]
    private bool explodeOnCollision = true; // 衝突で爆発するか（壁/敵/その他）

    [SerializeField, Tooltip("寿命切れ(時間切れ)でも爆発させるか")]
    private bool onExpireExplode = false;

    [SerializeField, Tooltip("爆発半径")]
    private float explosionRadius = 3.0f;

    [SerializeField, Tooltip("爆発ダメージ（固定値）")]
    private int explosionDamage = 10;

    // =======================
    // ③ 爆発対象のフィルタ
    // =======================
    [SerializeField, Tooltip("爆発のダメージ対象となるレイヤー（例：Enemy）")]
    private LayerMask targetLayers;

    [SerializeField, Tooltip("タグでも絞りたい場合に指定（空文字なら無視）")]
    private string requiredTag = "Enemy";

    // =======================
    // ④ 演出（任意）
    // =======================
    [Header("演出"), SerializeField]
    private GameObject hitEffect;   // 爆発時に出したいエフェクト（任意）

    [SerializeField]
    private AudioClip hitSe;        // 同SE（任意）

    [SerializeField, Tooltip("弾専用のヒット演出（任意）")]
    private GameObject bulletEffect;

    [SerializeField]
    private AudioClip bulletSe;

    // =======================
    // ⑤ 実行時の状態
    // =======================
    private Rigidbody m_rb;
    private Vector3 m_destination;          // 設定された目的地（ワールド座標固定）
    private bool m_hasDestination = false;  // 目的地がセット済か
    private bool m_explodedOrDestroyed = false; // 既に爆発/破壊したか（重複防止）

    // OverlapSphereNonAlloc 用の可変バッファ（GC削減に有効）
    [SerializeField, Tooltip("0 なら通常の OverlapSphere。>0 なら NonAlloc を使用")]
    private int nonAllocMaxHits = 0;
    private Collider[] _nonAllocHits;       // NonAlloc の結果格納

    // =======================
    // ⑥ 向き制御（A→B を向き軸として、着弾予定地点 or 進行方向に揃える）
    // =======================
    [Header("向き制御：オブジェクト内の2点（A→B）")]
    [SerializeField, Tooltip("オブジェクト内の基準点（回転の起点）。例：根本/重心など")]
    private Transform orientPointA;

    [SerializeField, Tooltip("オブジェクト内の先端点。A→Bの方向が“弾の向き軸”になる")]
    private Transform orientPointB;

    public enum AimMode
    {
        Destination,  // A から着弾予定地点へ向ける
        Velocity,     // A から現在の速度ベクトル（進行方向）へ向ける
    }

    [SerializeField, Tooltip("A→B を合わせる“目標方向”の基準")]
    private AimMode aimMode = AimMode.Destination;

    [SerializeField, Tooltip("回転の追随速度。0で瞬時、>0でスムーズ（slerp）")]
    private float aimSlerpSpeed = 20f;

    [SerializeField, Tooltip("回転適用の座標系。通常は『弾（このTransform）を回す』")]
    private Transform rotateRoot; // nullなら this.transform

    [SerializeField, Tooltip("ロール安定のための補助Up（ワールド）。未指定なら世界Up(Y+)")]
    private Vector3 worldUpHint = Vector3.up;

    [SerializeField, Tooltip("Upを強く固定したい場合は true（FromToRotation後にLookRotationでUpを与える）")]
    private bool stabilizeRollWithUp = false;

    // =======================
    // Unity ライフサイクル
    // =======================
    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_rb.useGravity = false;    // 弾が重力落下しない想定（必要なら true に）
        m_rb.isKinematic = false;   // 物理で動く（速度を直接与える）

        if (nonAllocMaxHits > 0)
        {
            _nonAllocHits = new Collider[nonAllocMaxHits];
        }
    }

    private void Start()
    {
        // 寿命が設定されていれば、自動で終了処理（爆発する/しないは onExpireExplode 次第）
        if (deathTime > 0f)
        {
            Invoke(nameof(HandleExpire), deathTime);
        }
    }

    private void Update()
    {
        if (m_explodedOrDestroyed) return;

        // 目的地が未設定なら停止（発射側の SetDestination 呼び忘れ防止）
        if (!m_hasDestination)
        {
            m_rb.velocity = Vector3.zero;
            return;
        }

        // 現在位置→目的地のベクトルと距離
        Vector3 toDest = m_destination - transform.position;
        float dist = toDest.magnitude;

        // 到達判定
        if (dist <= arriveThreshold)
        {
            if (explodeOnArrive) DoExplosionAndDestroy(); // 爆発して終了
            else PlayEffectsAndDestroy();  // 演出のみで終了
            return;
        }

        // 進行方向
        Vector3 dir = toDest.normalized;

        // 疑似山なり（見た目向けの簡易式）：不要なら arcHeight=0 に
        float yBoost = 0f;
        if (arcHeight > 0f)
        {
            // 「到達までの推定時間」を元に、1フレームだけ山を作る簡略式
            float tEstimated = dist / Mathf.Max(0.01f, speed);
            float u = Mathf.Clamp01(Time.deltaTime / Mathf.Max(tEstimated, 0.0001f));
            yBoost = 4f * arcHeight * (u - u * u); // 0→1→0 の山
        }

        // 速度を与える：直進＋任意の上向き成分
        m_rb.velocity = dir * speed + Vector3.up * yBoost;

        // ★見た目回転は「A→B軸で向きを合わせる」専用メソッドに任せるため、
        // ここでは transform.forward を直接いじらない（FixedUpdateで回す）。
    }

    private void FixedUpdate()
    {
        if (m_explodedOrDestroyed) return;
        UpdateOrientation(); // 物理系は FixedUpdate で回転を適用すると滑らか
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (m_explodedOrDestroyed) return;

        // 物理衝突（Trigger じゃない Collider 同士の接触）
        if (explodeOnCollision)
        {
            // 接触点付近で爆発（壁の手前で爆発して見た目が自然）
            DoExplosionAndDestroy(collision.GetContact(0).point);
        }
        else
        {
            PlayEffectsAndDestroy();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_explodedOrDestroyed) return;

        // Trigger コライダーに入った場合（到達点が Trigger、敵が Trigger 等）
        if (explodeOnCollision)
        {
            // 弾と相手の最近接点を爆心地に
            DoExplosionAndDestroy(other.ClosestPoint(transform.position));
        }
        else
        {
            PlayEffectsAndDestroy();
        }
    }

    // 寿命切れ
    private void HandleExpire()
    {
        if (m_explodedOrDestroyed) return;

        if (onExpireExplode) DoExplosionAndDestroy();
        else PlayEffectsAndDestroy();
    }

    // =======================
    // API：発射側から呼ぶ
    // =======================
    /// <summary>目的地（ワールド座標）を設定：呼び出し時点の固定座標</summary>
    public void SetDestination(Vector3 destination)
    {
        m_destination = destination;
        m_hasDestination = true;
    }

    /// <summary>目的地（Transform）を設定：呼び出し時点の position を採用（追尾はしない）</summary>
    public void SetDestination(Transform destination)
    {
        SetDestination(destination.position);
    }

    // =======================
    // 爆発・演出・破壊
    // =======================
    /// <summary>
    /// 範囲爆発→対象収集→ダメージ→演出→自身破壊（1回だけ）
    /// </summary>
    private void DoExplosionAndDestroy(Vector3? overridePos = null)
    {
        if (m_explodedOrDestroyed) return;
        m_explodedOrDestroyed = true;

        // 爆心地（指定があれば接触点、なければ弾の現在位置）
        Vector3 center = overridePos ?? transform.position;

        // 1) 爆風に触れるコライダーを取得
        //    - targetLayers でレイヤー制限
        //    - QueryTriggerInteraction.Collide で Trigger も含める（敵が Trigger でもOK）
        List<Collider> hits = new();
        if (_nonAllocHits != null && _nonAllocHits.Length > 0)
        {
            // GC を出さない版（最大数は nonAllocMaxHits 依存）
            int count = Physics.OverlapSphereNonAlloc(
                center, explosionRadius, _nonAllocHits, targetLayers, QueryTriggerInteraction.Collide
            );
            for (int i = 0; i < count; i++)
            {
                if (_nonAllocHits[i] != null) hits.Add(_nonAllocHits[i]);
            }
        }
        else
        {
            // シンプル版（配列を生成するので小規模向け）
            var cols = Physics.OverlapSphere(center, explosionRadius, targetLayers, QueryTriggerInteraction.Collide);
            if (cols != null && cols.Length > 0) hits.AddRange(cols);
        }

        // 2) 1つのキャラが複数コライダーを持っていても二重ヒットさせないためのセット
        HashSet<GameObject> processed = new();

        foreach (var col in hits)
        {
            if (col == null) continue;

            // Rigidbody を持つならルートは Rigidbody の GameObject とみなす
            var root = col.attachedRigidbody != null ? col.attachedRigidbody.gameObject : col.gameObject;
            if (processed.Contains(root)) continue;
            processed.Add(root);

            // タグでさらに絞りたい場合（空文字ならスキップ）
            if (!string.IsNullOrEmpty(requiredTag) && !root.CompareTag(requiredTag))
                continue;

            // Status コンポーネントがある相手だけ固定ダメージ
            if (root.TryGetComponent(out Status status))
            {
                status.Damage(explosionDamage);
            }

            // （拡張ポイント）
            // - Knockback: if (root.TryGetComponent(out Rigidbody rb)) { rb.AddExplosionForce(...); }
            // - 距離減衰や遮蔽チェックを入れる場合はここで
        }

        // 3) 爆発演出（パーティクル/SE）
        PlayEffects(center);

        // 4) 自身破壊
        Destroy(gameObject);
    }

    /// <summary>演出だけ再生して破壊（爆発ダメージは無し）</summary>
    private void PlayEffectsAndDestroy()
    {
        if (m_explodedOrDestroyed) return;
        m_explodedOrDestroyed = true;

        PlayEffects(transform.position);
        Destroy(gameObject);
    }

    private void PlayEffects(Vector3 pos)
    {
        if (hitEffect != null) Instantiate(hitEffect, pos, Quaternion.identity);
        if (hitSe != null) SoundEffect.Play3D(hitSe, pos); // または AudioSource.PlayClipAtPoint(hitSe, pos);
        if (bulletEffect != null) Instantiate(bulletEffect, pos, Quaternion.identity);
        if (bulletSe != null) SoundEffect.Play3D(bulletSe, pos);
    }

    // =======================
    // 向き合わせ（A→B軸を、着弾予定地点 or 進行方向に揃える）
    // =======================
    private void UpdateOrientation()
    {
        if (orientPointA == null || orientPointB == null) return;

        var root = rotateRoot != null ? rotateRoot : transform;

        // 現在の「向き軸」= A→B（ワールド）
        Vector3 a = orientPointA.position;
        Vector3 b = orientPointB.position;
        Vector3 currentDir = (b - a);
        if (currentDir.sqrMagnitude < 1e-6f) return; // AとBが同一点はNG

        // 目標方向
        Vector3 targetDir;
        switch (aimMode)
        {
            case AimMode.Velocity:
                // 速度ベクトル（Rigidbody推奨）
                Vector3 v = (m_rb != null) ? m_rb.velocity : root.forward; // 速度が取れない場合は前方でフォールバック
                if (v.sqrMagnitude < 1e-6f) return;
                targetDir = v;
                break;

            case AimMode.Destination:
            default:
                if (!m_hasDestination) return; // 目的地未設定なら回さない
                targetDir = (m_destination - a);
                if (targetDir.sqrMagnitude < 1e-6f) return;
                break;
        }

        // 軸合わせ用の回転差分（currentDir を targetDir へ）
        Quaternion delta = Quaternion.FromToRotation(currentDir, targetDir);

        // 目標回転 = 「今の回転」に「差分回転」を掛ける
        Quaternion targetRot = delta * root.rotation;

        // ロール安定を強めたい場合、Upベクトルを与えて LookRotation で補正
        if (stabilizeRollWithUp)
        {
            Vector3 newForward = targetDir.normalized;
            if (newForward.sqrMagnitude > 1e-6f)
            {
                Quaternion upFixed = Quaternion.LookRotation(newForward, worldUpHint.normalized);
                // 直接置き換えると急に回るので、適度にブレンド
                targetRot = Quaternion.Slerp(targetRot, upFixed, 0.5f);
            }
        }

        // スムーズ or 瞬時
        if (aimSlerpSpeed > 0f)
        {
            float t = 1f - Mathf.Exp(-aimSlerpSpeed * Time.fixedDeltaTime); // 物理フレームに合わせる
            targetRot = Quaternion.Slerp(root.rotation, targetRot, t);
        }

        // Rigidbodyがあるなら MoveRotation（物理的に滑らか）
        if (m_rb != null && !m_rb.isKinematic)
        {
            m_rb.MoveRotation(targetRot);
        }
        else
        {
            root.rotation = targetRot;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // シーンビューで爆発半径を可視化（選択時のみ）
        Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.35f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
        Gizmos.color = new Color(1f, 0.3f, 0.0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        // 向き軸（A→B）と目的地の可視化
        if (orientPointA != null && orientPointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(orientPointA.position, 0.03f);
            Gizmos.DrawSphere(orientPointB.position, 0.03f);
            Gizmos.DrawLine(orientPointA.position, orientPointB.position);
        }
        if (m_hasDestination && orientPointA != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_destination, 0.06f);
            Gizmos.DrawLine(orientPointA.position, m_destination);
        }
    }
#endif
}