
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class DamageSlowField : MonoBehaviour
{
	public float radius = 5f;
	public int baseDamagePerTick = 50;
	public float tickInterval = 1f;
	public float duration = 5f;
	public float perStackSlow = 0.3f;
	public DamageType damageType = DamageType.Poison;
	public LayerMask targetLayers;  

	private SphereCollider triggerCollider;

	private void Awake()
	{
		triggerCollider = GetComponent<SphereCollider>();
		triggerCollider.isTrigger = true;
		triggerCollider.radius = radius;

		if (gameObject.transform.position.y <= 0)
		{
			gameObject.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
		}

		Destroy(gameObject, duration);
	}

	private void OnTriggerEnter(Collider other)
	{
		// ★ レイヤーマスク判定（対象外なら return）
		if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
			return;

		var manager = other.GetComponent<StatusEffectManagerModel>();
		var status = other.GetComponent<Status>();

		if (manager != null && status != null)
		{
			Debug.Log($"[Field] {other.name} に状態異常を付与");

			var dot = new DamageOverTimeStackEffect(
				status,
				key: "Poison",
				displayName: "毒フィールド",
				type: damageType,
				initialStacks: 1,
				maxStacks: 1,
				baseDamagePerTick: baseDamagePerTick,
				tickInterval: tickInterval,
				perStackDecay: false,
				durationSeconds: duration,
				applyImmediateFirstTick: true
			);
			manager.AddOrStack(dot, 0);

			var slow = new SlowStackEffect(
				status,
				initialStacks: 1,
				maxStacks: 1,
				perStackSlow: perStackSlow,
				perStackDecay: false,
				durationSeconds: duration
			);
			manager.AddOrStack(slow, 0);
		}
		else
		{
			Debug.LogWarning($"[Field] {other.name} に StatusEffectManagerModel または Status がありません");
		}
	}

	private void OnTriggerExit(Collider other)
	{
		// ★ レイヤーマスク判定
		if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
			return;

		var manager = other.GetComponent<StatusEffectManagerModel>();
		if (manager != null)
		{
			Debug.Log($"[Field] {other.name} から状態異常を解除");
			manager.RemoveEffect("Poison");
			manager.RemoveEffect("Slow");
		}
	}
}
