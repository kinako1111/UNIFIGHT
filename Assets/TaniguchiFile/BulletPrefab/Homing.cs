
using UnityEngine;

/// <summary>
/// 追尾弾：衝突時に「攻撃者の攻撃力ぶんの即時ダメージ」→「攻撃者攻撃力の割合/秒のDOT（毒属性）」を付与。
/// スタック対応：Key="Poison" 同名扱いで層数を増加。上限=poisonMaxStacks。
/// </summary>
public class Homing : MonoBehaviour
{
	private enum Type
	{
		Poison,
		Heal,
		SpeedBuff,
	}

	[Header("弾の種類"), SerializeField]
	private Type m_bulletType = Type.Poison;

	[Header("弾速"), SerializeField]
	private float speed = 10f;

	[Header("山なり軌道の高さ（未使用なら0）"), SerializeField]
	private float arcHeight = 0f;

	[Header("消えるまでの時間(秒)"), SerializeField]
	private int m_deathTime = 3;

	[Header("毒の継続時間(秒)"), SerializeField]
	private float poisonDuration = 5f;

	[Header("毒の係数（攻撃力の割合/秒）"), SerializeField, Tooltip("例: 0.30 → 攻撃力の30%/秒")]
	private float poisonScalePerSecond = 0.30f;

	[Header("毒の最大スタック数"), SerializeField]
	private int poisonMaxStacks = 3;

	[Header("毒のTick間隔(秒)"), SerializeField]
	private float poisonTickInterval = 1.0f;

	[Header("弾毎の効果音(無ければ無視)"), SerializeField]
	private AudioClip m_bulletSe; 

	[Header("弾毎のエフェクト(無ければ無視)"), SerializeField]
	private GameObject m_bulletEffect;

	// --- 実行時パラメータ ---
	private GameObject m_target;        // 追尾対象（敵）

	private int m_attackPower;          // フォールバック用（owner不在時）
	private Status m_ownerStatus;       // 任意：攻撃者（プレイヤー等）のStatus（最新ATKを参照）

	private GameObject m_effect;        // ヒット時エフェクト
	private AudioClip m_se;             // ヒット時SE
	private Rigidbody m_rb;

	private void Awake()
	{
		m_rb = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		// 寿命で自壊
		Destroy(gameObject, m_deathTime);
	}

	private void Update()
	{
		// ターゲットが無効なら破棄
		if (m_target == null)
		{
			Destroy(gameObject);
			return;
		}

		// ターゲットの生存確認
		Status targetStatus;
		if (!m_target.TryGetComponent(out targetStatus) || targetStatus.GetHp() <= 0 || targetStatus.GetDeath())
		{
			Destroy(gameObject);
			return;
		}

		// 追尾（簡易直線。arcHeightを使った山なりは必要なら拡張）
		Vector3 toTarget = (m_target.transform.position - transform.position);
		Vector3 direction = toTarget.normalized;

		// （オプション）山なり成分
		float yBoost = 0f;
		if (arcHeight > 0f)
		{
			float dist = toTarget.magnitude;
			float t = dist / Mathf.Max(0.01f, speed);          // 到達予測秒
			float u = Mathf.Clamp01(Time.deltaTime / Mathf.Max(t, 0.0001f));
			yBoost = 4f * arcHeight * (u - u * u);
		}

		m_rb.velocity = direction * speed + Vector3.up * yBoost;
		transform.forward = direction; // 見た目調整（任意）
	}

	private void OnTriggerEnter(Collider other)
	{
		// 衝突先がターゲットのみ処理
		if (other.gameObject != m_target) return;

		// ターゲットの Status / Manager
		if (!m_target.TryGetComponent(out Status status))
		{
			Destroy(gameObject);
			return;
		}

		if (status == null || status.GetDeath())
		{
			Debug.LogWarning("[Homing] 付与先が不正（Status無し or 死亡）");
			Destroy(gameObject);
			return;
		}

		// 攻撃者の現在攻撃力（ownerがあればバフ/デバフ反映、無ければフォールバック値）
		int attackerAtk = (m_ownerStatus != null) ? m_ownerStatus.GetAttackPower() : m_attackPower;
		attackerAtk = Mathf.Max(0, attackerAtk);

		switch (m_bulletType)
		{
			case Type.Poison:
				{
					// --- ① 即時ダメージ：攻撃力100% ---
					status.Damage(attackerAtk);
					break;
				}

			case Type.Heal:
				{
					// 例：即時回復 + HOT（同名Key="HOT"、層で回復量/tick加算）
					// int healInstant = attackerAtk;
					// status.Heal(healInstant);
					// var manager = status.GetComponent<StatusEffectManager>() ?? status.gameObject.AddComponent<StatusEffectManager>();
					// var hot = new HealOverTimeStackEffect(...); // ※未実装なら後で提供します
					// manager.AddOrStack(hot, stacksToAdd: 1, perStackDuration: hotDuration);
					break;
				}

			case Type.SpeedBuff:
				{
					// 例：速度バフ（Key="Slow" の逆。Key="SpeedBuff" で +x%/層）
					// var manager = status.GetComponent<StatusEffectManager>() ?? status.gameObject.AddComponent<StatusEffectManager>();
					// var spd = new SpeedBuffStackEffect(...);
					// manager.AddOrStack(spd, stacksToAdd: 1, perStackDuration: duration);
					break;
				}
		}

		// ヒット演出
		if (m_effect != null)
		{
			Instantiate(m_effect, m_target.transform.position, Quaternion.identity);
		}

		if (m_se != null)
		{
			SoundEffect.Play3D(m_se, m_target.transform.position);
		}

		//その弾専用のヒット演出
		if (m_bulletEffect != null)
		{
			Instantiate(m_bulletEffect, m_target.transform.position, Quaternion.identity);
		}

		if (m_bulletSe != null)
		{
			SoundEffect.Play3D(m_bulletSe, m_target.transform.position);
		}

		// 弾は役目を終えたので破壊
		Destroy(gameObject);
	}

	private void OnCollisionEnter(Collision collision)
	{
		// 壁に当たったら貫通しない
		if (collision.gameObject.CompareTag("Wall"))
		{
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// 追尾弾のパラメータ設定（フォールバック版）。
	/// owner 未指定時は m_attackPower を使用。
	/// </summary>
	public void SetStatus(GameObject newTarget, int attackPower, GameObject effect, AudioClip se)
	{
		m_target = newTarget;
		m_attackPower = attackPower;
		m_effect = effect;
		m_se = se;
		m_ownerStatus = null;
	}

	/// <summary>
	/// 追尾弾のパラメータ設定（攻撃者Statusを渡す版：バフ/デバフに追随）。
	/// </summary>
	public void SetStatus(GameObject newTarget, Status ownerStatus, GameObject effect, AudioClip se)
	{
		m_target = newTarget;
		m_ownerStatus = ownerStatus; // 攻撃者の現在ATKを参照
		m_effect = effect;
		m_se = se;

		// フォールバック値も用意（初期化時点の攻撃力）
		m_attackPower = (ownerStatus != null) ? ownerStatus.GetAttackPower() : 0;
	}
}
