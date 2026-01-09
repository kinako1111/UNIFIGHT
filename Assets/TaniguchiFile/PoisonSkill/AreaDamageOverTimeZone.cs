
using System.Collections.Generic;
using UnityEngine;

// ゾーン内の敵にDOTを付与し続ける。出たら解除する。
[RequireComponent(typeof(Collider))]
public class AreaDamageOverTimeZone : MonoBehaviour
{
	[Header("DOT設定")]
	[SerializeField] private string effectKey = "AreaPoison";      // ゾーン識別用のKey（敵側で同名はスタック合算）
	[SerializeField] private string displayName = "毒の霧";          // UI表示名
	[SerializeField] private DamageType damageType = DamageType.Poison;

	[Tooltip("入った瞬間に付与する初期スタック数")]
	[SerializeField] private int initialStacks = 1;

	[SerializeField] private int maxStacks = 10;

	[Tooltip("1スタックあたりのダメージ量（Tick毎）")]
	[SerializeField] private int baseDamagePerTick = 2;

	[Tooltip("DOTのTick間隔（秒）。例：0.5なら毎秒2回ダメージ")]
	[SerializeField] private float tickInterval = 0.5f;

	[Header("持続時間と更新方針")]
	[Tooltip("true: スタック毎に個別タイマー / false: 共有タイマー（一般的なゾーンは共有推奨）")]
	[SerializeField] private bool perStackDecay = false;

	[Tooltip("ゾーン内にいる間は、一定間隔で持続時間をリフレッシュします")]
	[SerializeField] private float refreshDurationSeconds = 2f;

	[Tooltip("入った瞬間に初回ダメージを与える")]
	[SerializeField] private bool applyImmediateFirstTick = true;

	[Header("フィルタリング")]
	[Tooltip("対象レイヤーのみDOT適用")]
	[SerializeField] private LayerMask targetLayerMask = ~0;

	[Tooltip("対象タグ。空ならタグ不問")]
	[SerializeField] private string requiredTag = "";

	// ゾーンに滞在中の対象と、その対象に付与したEffectの参照
	private readonly Dictionary<Status, IStatusEffectModel> activeTargets = new();

	[SerializeField] private float refreshInterval = 0.5f; // 0.5秒ごとに延長
	private float refreshAccum = 0f;

	private Collider zoneCollider;

	private void Awake()
	{
		zoneCollider = GetComponent<Collider>();
		zoneCollider.isTrigger = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!IsValidTarget(other, out Status status, out StatusEffectManagerModel manager))
			return;

		// すでに登録済みならスタック追加だけ
		if (activeTargets.TryGetValue(status, out var existing))
		{
			// ゾーン再侵入/複数ゾーン重複などへの安全策：一定のdurationでリフレッシュ
			existing.AddStacks(initialStacks, refreshDurationSeconds);
			return;
		}

		// 新規にDOTインスタンスを作成して付与
		var dot = new DamageOverTimeStackEffect(
			owner: status,
			key: effectKey,
			displayName: displayName,
			type: damageType,
			initialStacks: initialStacks,
			maxStacks: maxStacks,
			baseDamagePerTick: baseDamagePerTick,
			tickInterval: tickInterval,
			perStackDecay: perStackDecay,
			durationSeconds: refreshDurationSeconds,
			applyImmediateFirstTick: applyImmediateFirstTick
		);

		manager.AddOrStack(dot, initialStacks, perStackDuration: refreshDurationSeconds);
		activeTargets[status] = dot;
	}

	private void Update()
	{
		refreshAccum += Time.deltaTime;
		if (refreshAccum >= refreshInterval)
		{
			refreshAccum -= refreshInterval;
			// ゾーン内の全対象の持続時間だけを延長
			foreach (var kv in activeTargets)
			{
				kv.Value.AddStacks(0, refreshDurationSeconds);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!IsValidTarget(other, out Status status, out StatusEffectManagerModel manager))
			return;

		// ゾーンから出たら該当Keyの効果を解除（安全に存在チェック）
		if (activeTargets.Remove(status))
		{
			manager.RemoveEffect(effectKey);
		}
	}

	private bool IsValidTarget(Collider other, out Status status, out StatusEffectManagerModel manager)
	{
		status = null; manager = null;

		// レイヤーマスクとタグでフィルタリング
		if ((targetLayerMask.value & (1 << other.gameObject.layer)) == 0) return false;
		if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return false;

		status = other.GetComponent<Status>();
		manager = other.GetComponent<StatusEffectManagerModel>();
		if (status == null || manager == null) return false;

		return true;
	}

#if UNITY_EDITOR
	// 視認性向上用のギズモ
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.25f);
		var col = GetComponent<SphereCollider>();
		if (col != null)
		{
			Gizmos.DrawSphere(transform.position + col.center, col.radius * Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z)));
		}
		else
		{
			// BoxCollider等でも枠表示
			Gizmos.DrawWireCube(transform.position, transform.localScale);
		}
	}
#endif
}
