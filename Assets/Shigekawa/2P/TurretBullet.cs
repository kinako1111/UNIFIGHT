using UnityEngine;

public class TurretBullet : MonoBehaviour
{
	[Header("弾が自動で消滅するまでの時間 (秒)"), SerializeField]
	private float lifeTime = 3f; 
	[Header("ターゲットに与えるダメージ量"), SerializeField]
	int attackPower = 10;

	[Header("追尾弾が移動する速度"), SerializeField]
	float trackingSpeed = 10f;
	[Header("ターゲットに向かってどれくらいの速さで向きを変えるか"), SerializeField]
	float trackingStrength = 5f;

	private GameObject m_target; 
	private Rigidbody rb;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = gameObject.AddComponent<Rigidbody>();
		}
		rb.isKinematic = true;

		Collider col = GetComponent<Collider>();
		if (col == null)
		{
			col = gameObject.AddComponent<SphereCollider>();
		}
		col.isTrigger = true;
	}

	public void Initialize(GameObject target, float speed, float strength, int power, float bulletLifeTime)
	{
		m_target = target;
		trackingSpeed = speed;
		trackingStrength = strength;
		attackPower = power;
		this.lifeTime = bulletLifeTime; 
		rb.velocity = Vector3.zero;
	}

	void Update()
	{
		lifeTime -= Time.deltaTime;
		if (lifeTime <= 0)
		{
			Destroy(gameObject);
			return;
		}

		if (m_target == null || !m_target.activeSelf)
		{
			Destroy(gameObject);
			return;
		}

		Vector3 directionToTarget = (m_target.transform.position - transform.position).normalized;
		Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, trackingStrength * Time.deltaTime);

		transform.Translate(Vector3.forward * trackingSpeed * Time.deltaTime, Space.Self);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (m_target != null && other.gameObject == m_target)
		{
			Status targetStatus;
			if (other.gameObject.TryGetComponent(out targetStatus))
			{
				targetStatus.Damage(attackPower);
			}
			Destroy(gameObject);
		}
		else if (other.CompareTag("Enemy"))
		{
			Status enemyStatus;
			if (other.gameObject.TryGetComponent(out enemyStatus))
			{
				enemyStatus.Damage(attackPower);
			}
			Destroy(gameObject);
		}
		else if (!other.isTrigger)
		{
			Destroy(gameObject);
		}
	}
}