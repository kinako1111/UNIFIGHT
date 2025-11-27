using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TurretBullet : MonoBehaviour
{
	[Header("弾速"), SerializeField]
	private float m_speed = 15f;

	[Header("消えるまでの時間"), SerializeField]
	private float m_deathTime = 3f;

	[Header("障害物レイヤー（Everything推奨）"), SerializeField]
	private LayerMask m_obstacleLayer = -1;

	// 生成時にセットされる変数
	private GameObject m_target;
	private int m_attackPower;
	private GameObject m_effect;
	private AudioClip m_se;

	private Rigidbody m_rb;

	// ★追加：自分で寿命をカウントするためのタイマー
	private float m_lifeTimer = 0f;

	private void Awake()
	{
		m_rb = GetComponent<Rigidbody>();
		m_rb.isKinematic = false;
	}

	private void Start()
	{
		// ★削除：これだとタイミングがズレることがあるので消します
		// Destroy(gameObject, m_deathTime);
	}

	private void FixedUpdate()
	{
		// ★追加：ここで時間を計り、寿命が来たら即座に処理を打ち切る
		m_lifeTimer += Time.fixedDeltaTime;
		if (m_lifeTimer >= m_deathTime)
		{
			Destroy(gameObject);
			return; // ★重要：ここで return することで、下の移動処理や判定をさせない
		}

		// ターゲットが消滅している、またはStatusで死亡判定なら弾を消す
		if (m_target == null)
		{
			Destroy(gameObject);
			return;
		}

		Status targetStatus = m_target.GetComponent<Status>();
		if (targetStatus != null && targetStatus.GetHp() <= 0)
		{
			Destroy(gameObject);
			return;
		}

		Vector3 direction = (m_target.transform.position - transform.position).normalized;
		m_rb.velocity = direction * m_speed;

		if (direction != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(direction);
		}

		// Raycast判定
		DetectObstacle(direction);
	}

	private void DetectObstacle(Vector3 direction)
	{
		float step = m_speed * Time.fixedDeltaTime;
		float rayDistance = step * 1.2f;

		if (Physics.Raycast(transform.position, direction, out RaycastHit hit, rayDistance, m_obstacleLayer, QueryTriggerInteraction.Collide))
		{
			if (hit.collider.gameObject == m_target) return;
			if (hit.collider.gameObject == gameObject) return;

			HitProcess();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == m_target)
		{
			Status status = m_target.GetComponent<Status>();
			if (status != null)
			{
				status.Damage(m_attackPower);
				HitEffect();
			}
		}
		else if (other.CompareTag("Wall"))
		{
			HitProcess();
		}
	}

	private void HitProcess()
	{
		Destroy(gameObject);
	}

	private void HitEffect()
	{
		if (m_effect != null)
		{
			Instantiate(m_effect, transform.position, Quaternion.identity);
		}

		if (m_se != null)
		{
			AudioSource.PlayClipAtPoint(m_se, transform.position);
		}

		Destroy(gameObject);
	}

	public void SetStatus(GameObject target, int attackPower, GameObject effect, AudioClip se)
	{
		m_target = target;
		m_attackPower = attackPower;
		m_effect = effect;
		m_se = se;
	}
}