
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class DamageSlowField : MonoBehaviour
{
	public float radius = 5f;
	public int baseDamagePerTick = 50;
	public float tickInterval = 1f;
	public float duration = 10f;
	public float perStackSlow = 0.3f; // 30%減速
	public DamageType damageType = DamageType.Poison;

	private SphereCollider triggerCollider;

	private void Awake()
	{
		triggerCollider = GetComponent<SphereCollider>();
		triggerCollider.isTrigger = true;
		triggerCollider.radius = radius;

		// フィールド寿命
		Destroy(gameObject, duration);
	}

	private void OnTriggerEnter(Collider other)
	{
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
			manager.AddOrStack(dot, stacksToAdd: 0);

			var slow = new SlowStackEffect(
				status,
				initialStacks: 1,
				maxStacks: 1,
				perStackSlow: perStackSlow,
				perStackDecay: false,
				durationSeconds: duration
			);
			manager.AddOrStack(slow, stacksToAdd: 0);
		}
		else
		{
			Debug.LogWarning($"[Field] {other.name} に StatusEffectManagerModel または Status がありません");
		}
	}

	private void OnTriggerExit(Collider other)
	{
		var manager = other.GetComponent<StatusEffectManagerModel>();
		if (manager != null)
		{
			Debug.Log($"[Field] {other.name} から状態異常を解除");
			manager.RemoveEffect("Poison");
			manager.RemoveEffect("Slow");
		}
	}
}
