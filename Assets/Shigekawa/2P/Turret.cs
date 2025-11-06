using System.Collections;
using System.Collections.Generic;
using System.Linq; // LINQを使用するため

using UnityEngine;

public class Turret : MonoBehaviour
{
	// 攻撃範囲の半径
	[Header("攻撃範囲"), SerializeField]
	float m_autoAttackRange = 5f;

	// 一度の攻撃でダメージ判定を発生させる回数 (アニメーターがないため、この値は直接的な影響は少ないが、将来的な拡張のために残す)
	[Header("攻撃の発生回数  ※デフォは1"), SerializeField]
	int m_autoAttackCount = 1; // 現状、TrackingBulletが1発で処理するため、この値は直接的な影響なし

	// 一度に攻撃できる敵の数
	[Header("一度に攻撃できる数"), SerializeField]
	int m_simultaneous = 1;

	// 攻撃と攻撃の間の待ち時間（秒）
	[Header("攻撃速度"), SerializeField]
	float m_autoAttackInterval = 0.75f;

	// 遠距離攻撃で使用する弾のプレハブ
	[Header("弾のPrefab"), SerializeField]
	GameObject m_bulletPrefab;

	// 弾を生成する位置のTransform
	[Header("弾の生成地点"), SerializeField]
	Transform m_generateTransform;

	[Header("回転速度"), SerializeField]
	float m_rotationSpeed = 10f;

	[Header("タレット消滅時間"), SerializeField]
	float m_turretLifeTime = 10f;

	// TurretBulletが持つプロパティの一部をTurretから設定できるようにするためのフィールド
	[Header("弾のプロパティ"), SerializeField]
	float m_bulletSpeed = 10f;
	[SerializeField]
	float m_bulletTrackingStrength = 5f;
	[SerializeField]
	float m_bulletLifeTime = 3f; // TurretBulletのライフタイムをここでも設定できるようにする

	// 攻撃範囲内にいる全てのユニットのリスト
	List<GameObject> m_unitsInRange = new();

	// 実際に攻撃対象となる敵のリスト
	List<GameObject> m_currentAttackTargets = new();

	Status m_status;

	bool m_isAttacking; // 現在攻撃中かどうかを示すフラグ
	private Coroutine m_attackCoroutine; // 攻撃ループを管理するコルーチン


	// 現在攻撃中であるか外部から参照するためのプロパティ
	public bool IsAttacking => m_isAttacking;

	private void Awake()
	{
		m_status = GetComponent<Status>();
	}

	private void Start()
	{
		m_isAttacking = false;
		// タレット本体を、指定した時間後に自動的に破棄する
		Destroy(gameObject, m_turretLifeTime);
	}

	void Update()
	{
		DetectAndSelectTargets();

		if (m_currentAttackTargets.Count > 0 && m_attackCoroutine == null)
		{
			m_attackCoroutine = StartCoroutine(AttackLoopCoroutine());
			m_isAttacking = true;
		}
		else if (m_currentAttackTargets.Count == 0 && m_attackCoroutine != null)
		{
			StopCoroutine(m_attackCoroutine);
			m_attackCoroutine = null;
			m_isAttacking = false;
		}
	}

	private void DetectAndSelectTargets()
	{
		m_unitsInRange.Clear();
		m_currentAttackTargets.Clear();

		Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_autoAttackRange);

		List<GameObject> potentialTargets = hitColliders
			.Select(collider => collider.gameObject)
			.Where(gameObject => gameObject != this.gameObject && gameObject.CompareTag("Enemy"))
			.ToList();

		m_unitsInRange = potentialTargets
			.OrderBy(gameObject => (transform.position - gameObject.transform.position).sqrMagnitude)
			.ToList();

		for (int i = 0; i < m_simultaneous && i < m_unitsInRange.Count; i++)
		{
			m_currentAttackTargets.Add(m_unitsInRange[i]);
		}
	}

	IEnumerator AttackLoopCoroutine()
	{
		while (m_currentAttackTargets.Count > 0)
		{
			PerformAttackLogic();
			yield return new WaitForSeconds(m_autoAttackInterval);
		}
		m_attackCoroutine = null;
		m_isAttacking = false;
	}

	private void PerformAttackLogic()
	{
		if (m_currentAttackTargets.Count == 0) return;

		RotateTowardsTarget();
		ExecuteFarAttack();
	}

	private void RotateTowardsTarget()
	{
		if (m_currentAttackTargets.Count == 0 || m_currentAttackTargets.First() == null) return;

		Vector3 targetDirection = (m_currentAttackTargets.First().transform.position - transform.position).normalized;
		targetDirection.y = 0;

		if (targetDirection != Vector3.zero)
		{
			Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
			targetRotation *= Quaternion.Euler(0, -90f, 0);

			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, m_rotationSpeed * Time.deltaTime);
		}
	}


	private void ExecuteFarAttack()
	{
		foreach (GameObject target in m_currentAttackTargets.ToList())
		{
			if (target == null || !target.activeSelf)
			{
				continue;
			}

			Vector3 directionToTarget = (target.transform.position - m_generateTransform.position).normalized;
			Quaternion initialBulletRotation = Quaternion.LookRotation(directionToTarget);

			GameObject bullet = Instantiate(m_bulletPrefab, m_generateTransform.position, initialBulletRotation);

			TurretBullet turretBulletComponent = bullet.GetComponent<TurretBullet>();
			if (turretBulletComponent != null)
			{
				if (m_status != null)
				{
					// TurretBulletのInitializeメソッドにm_bulletLifeTimeも渡す
					turretBulletComponent.Initialize(target, m_bulletSpeed, m_bulletTrackingStrength, m_status.GetAttackPower(), m_bulletLifeTime);
				}
			}
		}
	}
}