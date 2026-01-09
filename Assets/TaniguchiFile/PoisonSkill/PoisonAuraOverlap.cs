
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class PoisonAuraOverlap : MonoBehaviour
//{
//	[Header("半径"), SerializeField] private float radius = 3f;
//	[Header("DPS（毎秒ダメージ）"), SerializeField] private int dps = 10;
//	[Header("付与間隔(秒)"), SerializeField] private float applyInterval = 0.5f;
//	[Header("寿命（秒）"), SerializeField] private float lifetime = 5f;
//	[Header("対象レイヤー"), SerializeField] private LayerMask targetLayers;
//	[Header("タグ（任意）"), SerializeField] private string targetTag = "Enemy";

//	// Auraは継続付与でリフレッシュさせるのが基本。maxStacks=1推奨。
//	[Header("スタックポリシー"), SerializeField]
//	private StatusEffectManager.StackPolicy stackPolicy = StatusEffectManager.StackPolicy.RefreshOnReapply;

//	[Header("最大スタック数（Layered時のみ）"), SerializeField]
//	private int maxStacks = 1; // Auraは通常1（層追加しない）

//	// 範囲内ターゲットごとの「次に付与できる時刻」を管理（重複付与防止）
//	private readonly Dictionary<GameObject, float> nextApplyTimes = new();

//	private float endTime;

//	// OverlapSphereNonAlloc のバッファ（GC負荷軽減）
//	private Collider[] hitsBuffer;
//	[SerializeField, Tooltip("OverlapSphereの最大ヒット数（GC対策）")]
//	private int maxHits = 32;

//	private void OnEnable()
//	{
//		endTime = Time.time + lifetime;
//		hitsBuffer = new Collider[maxHits];

//		// --- デバッグログ（必要なら外してください） ---
//		// Debug.Log($"[Aura] Enable radius={radius}, lifetime={lifetime}, interval={applyInterval}");
//	}

//	private void OnDisable()
//	{
//		nextApplyTimes.Clear();

//		// --- デバッグログ（必要なら外してください） ---
//		// Debug.Log("[Aura] Disable & clear dictionary");
//	}

//	private void Update()
//	{
//		// 寿命終了で自壊
//		if (Time.time >= endTime)
//		{
//			// --- デバッグログ（必要なら外してください） ---
//			// Debug.Log("[Aura] Lifetime ended → Destroy");

//			Destroy(gameObject);
//			return;
//		}

//		// 範囲内のコライダーを取得（NonAllocでGC削減）
//		int hitCount = Physics.OverlapSphereNonAlloc(transform.position, radius, hitsBuffer, targetLayers);

//		// --- デバッグログ（必要なら外してください） ---
//		// Debug.Log(hitCount);

//		if (hitCount == 0) return;

//		// 範囲外に出たターゲットの辞書エントリを掃除
//		CleanupMissing(hitsBuffer, hitCount);

//		// 範囲内の敵に一定間隔で毒を再付与（Refresh or Layered）
//		for (int i = 0; i < hitCount; i++)
//		{
//			var col = hitsBuffer[i];
//			if (col == null) continue;

//			var go = col.gameObject;

//			// タグフィルタ（空なら無視、指定があれば一致のみ許可）
//			if (!string.IsNullOrEmpty(targetTag) && !go.CompareTag(targetTag)) continue;

//			// 付与先の Status / Manager を取得
//			var status = go.GetComponent<Status>();
//			var manager = go.GetComponent<StatusEffectManager>();

//			// 付与先が不正（コンポーネント無し、死亡状態）ならスキップ
//			if (status == null || manager == null || status.GetDeath()) continue;

//			// 次回付与可能時刻の管理（エントリがなければ初期化）
//			if (!nextApplyTimes.TryGetValue(go, out var next))
//			{
//				next = 0f;
//				nextApplyTimes[go] = 0f;
//			}

//			// 付与可能タイミングを過ぎていれば付与
//			if (Time.time >= next)
//			{
//				float duration = applyInterval;

//				// 目標DPSを「攻撃力比の毎秒倍率」に変換。
//				// 例：AP=40, dps=10 → scale=10/40=0.25（APの25%/秒を与える）
//				float currentAP = Mathf.Max(1f, status.GetAttackPower());
//				float poisonScale = dps / currentAP;

//				// 毎秒ダメージは「getSourceValue() × scalePerSecond × StackCount」
//				// → getSourceValue に最新攻撃力を渡しているので、バフ/デバフに追随します。

//				var poison = new PoisonEffect(
//					scalePerSecond: poisonScale,
//					duration: applyInterval,          // 0.5秒でもOK
//					getSourceValue: () => status.GetAttackPower(),
//					onDamage: dmg => status.Damage(dmg),
//					icon: null,
//					policy: StatusEffectManager.StackPolicy.RefreshOnReapply,
//					maxStacks: 1,
//					continuousMode: true              // ★ 連続蓄積モード
//				);

//				// 付与（Manager は MonoBehaviour で Update 内に Tick がある前提）
//				manager.AddEffect(poison);

//				// 次回付与時刻を更新（applyInterval後に再付与）
//				nextApplyTimes[go] = Time.time + applyInterval;
//			}
//		}
//	}

//	/// <summary>
//	/// 現在範囲内にいないターゲットの辞書エントリを削除
//	/// </summary>
//	private void CleanupMissing(Collider[] currentHits, int count)
//	{
//		var present = new HashSet<GameObject>();
//		for (int i = 0; i < count; i++)
//		{
//			if (currentHits[i] != null)
//				present.Add(currentHits[i].gameObject);
//		}

//		var toRemove = new List<GameObject>();
//		foreach (var kvp in nextApplyTimes)
//		{
//			if (!present.Contains(kvp.Key))
//				toRemove.Add(kvp.Key);
//		}
//		foreach (var key in toRemove)
//			nextApplyTimes.Remove(key);
//	}

//	private void OnDrawGizmosSelected()
//	{
//		// エディタで半径を可視化
//		Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
//		Gizmos.DrawSphere(transform.position, radius);
//	}
//}
