using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SmokeSlow : MonoBehaviour
{
	public float radius = 10f;
	public float duration = 10f;
	public float perStackSlow = 0.3f;
	public LayerMask targetLayers;

	private SphereCollider triggerCollider;
	// ★ 現在範囲内にいる管理コンポーネントを保持するリスト
	private List<StatusEffectManagerModel> affectedManagers = new List<StatusEffectManagerModel>();

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
		if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
			return;

		StatusEffectManagerModel manager = other.GetComponent<StatusEffectManagerModel>();
		Status status = other.GetComponent<Status>();

		if (manager != null && status != null)
		{
			// リストに追加
			if (!affectedManagers.Contains(manager))
			{
				affectedManagers.Add(manager);
			}

			SlowStackEffect slow = new SlowStackEffect(
				status,
				initialStacks: 1,
				maxStacks: 1,
				perStackSlow: perStackSlow,
				perStackDecay: false,
				durationSeconds: duration
			);
			manager.AddOrStack(slow, 0);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
			return;

		StatusEffectManagerModel manager = other.GetComponent<StatusEffectManagerModel>();
		if (manager != null)
		{
			RemoveSlowEffect(manager);
			// リストから除外
			affectedManagers.Remove(manager);
		}
	}

	// ★ 破棄されるときに実行
	private void OnDestroy()
	{
		foreach (StatusEffectManagerModel manager in affectedManagers)
		{
			if (manager != null) // 対象が先に消滅している可能性を考慮
			{
				RemoveSlowEffect(manager);
			}
		}
		affectedManagers.Clear();
	}

	// 解除処理の共通化
	private void RemoveSlowEffect(StatusEffectManagerModel manager)
	{
		Debug.Log($"[Field] {manager.name} の状態異常を解除");
		manager.RemoveEffect("Poison");
		manager.RemoveEffect("Slow");
	}
}