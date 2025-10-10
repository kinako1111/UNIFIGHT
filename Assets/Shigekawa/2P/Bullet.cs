using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
	[SerializeField] float lifeTime = 3f;

	private float _currentBulletSpeed;
	private Rigidbody _rb; // •Ï”–¼‚ğ_rb‚É•ÏX

	void Awake()
	{
		_rb = GetComponent<Rigidbody>(); // Rigidbody‚ğæ“¾
	}

	public void Initialize(Vector3 direction, float speed)
	{
		transform.forward = direction;
		_currentBulletSpeed = speed;

		_rb.velocity = direction * _currentBulletSpeed; // _rb‚ğg—p
	}

	void Update()
	{
		lifeTime -= Time.deltaTime;
		if (lifeTime <= 0)
		{
			Destroy(gameObject);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		Destroy(gameObject);
	}
}