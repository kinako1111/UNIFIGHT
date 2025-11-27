using UnityEngine;

// 範囲内の敵を索敵し、敵の方を向いて自動で弾を発射する砲台のクラス
public class Turret : MonoBehaviour
{
	[Header("攻撃範囲"), SerializeField]
	private float m_attackRange = 10f;

	[Header("発射間隔（秒）"), SerializeField]
	private float m_fireInterval = 1.0f;

	[Header("タレットの消滅時間"), SerializeField]
	private float m_lifeTime = 10f;

	[Header("弾のPrefab"), SerializeField]
	private GameObject m_bulletPrefab;

	[Header("弾の生成地点（銃口）"), SerializeField]
	private Transform m_muzzlePoint;

	[Header("敵のレイヤー設定")]
	[SerializeField] private LayerMask m_enemyLayer;

	[Header("弾のエフェクト・SE設定")]
	[SerializeField] private GameObject m_bulletHitEffect;
	[SerializeField] private AudioClip m_bulletHitSe;

	[Header("モデルの向き補正（90, -90, 180などで調整）")]
	[SerializeField] private float m_angleCorrection = 0f;

	// ★追加：1秒間に何度回転するか（大きいほど速く振り向く）
	[Header("旋回速度（度/秒）")]
	[SerializeField] private float m_turnSpeed = 120f;

	private int m_turretAttackPower = 10;

	private float m_fireTimer;
	private GameObject m_currentTarget;

	private void Start()
	{
		Destroy(gameObject, m_lifeTime);
		m_fireTimer = m_fireInterval;
	}

	private void Update()
	{
		UpdateTarget();

		if (m_currentTarget != null)
		{
			RotateTowardsTarget();

			// 発射間隔を計測し、時間が来たら発射する
			m_fireTimer += Time.deltaTime;
			if (m_fireTimer >= m_fireInterval)
			{
				Fire();
				m_fireTimer = 0f;
			}
		}
	}

	// ★修正：ターゲットに向かって滑らかに回転する処理
	private void RotateTowardsTarget()
	{
		if (m_currentTarget == null) return;

		// ターゲットへの方向ベクトルを計算（高さYは無視して水平にする）
		Vector3 direction = m_currentTarget.transform.position - transform.position;
		direction.y = 0; // 水平方向のみ

		// 方向がゼロでない場合のみ回転させる
		if (direction != Vector3.zero)
		{
			// 1. 最終的に向きたい角度（ゴール）を計算
			//    敵の方向への回転 * 補正角度
			Quaternion lookRotation = Quaternion.LookRotation(direction);
			Quaternion targetRotation = lookRotation * Quaternion.Euler(0, m_angleCorrection, 0);

			// 2. 現在の角度からゴールの角度へ、指定した速度で徐々に回転させる
			transform.rotation = Quaternion.RotateTowards(
				transform.rotation,
				targetRotation,
				m_turnSpeed * Time.deltaTime
			);
		}
	}

	private void UpdateTarget()
	{
		// 今のターゲットが有効かチェック
		if (m_currentTarget != null)
		{
			float dist = Vector3.Distance(transform.position, m_currentTarget.transform.position);
			Status s = m_currentTarget.GetComponent<Status>();

			if (dist > m_attackRange || s == null || s.GetHp() <= 0)
			{
				m_currentTarget = null;
			}
		}

		// ターゲットがいなければ、一番近い敵を探す
		if (m_currentTarget == null)
		{
			Collider[] hits = Physics.OverlapSphere(transform.position, m_attackRange, m_enemyLayer);
			float closestDist = Mathf.Infinity;

			foreach (var hit in hits)
			{
				Status s = hit.GetComponent<Status>();
				if (s != null && s.GetHp() > 0)
				{
					float d = Vector3.Distance(transform.position, hit.transform.position);
					if (d < closestDist)
					{
						closestDist = d;
						m_currentTarget = hit.gameObject;
					}
				}
			}
		}
	}

	private void Fire()
	{
		if (m_bulletPrefab == null || m_muzzlePoint == null) return;

		// 弾を生成（回転は補正済みの本体ではなく、銃口の向きに合わせる）
		GameObject bulletObj = Instantiate(m_bulletPrefab, m_muzzlePoint.position, m_muzzlePoint.rotation);

		TurretBullet bulletScript = bulletObj.GetComponent<TurretBullet>();
		if (bulletScript != null)
		{
			bulletScript.SetStatus(m_currentTarget, m_turretAttackPower, m_bulletHitEffect, m_bulletHitSe);
		}
	}

	public void SetAttackPower(int power)
	{
		m_turretAttackPower = power;
	}
}