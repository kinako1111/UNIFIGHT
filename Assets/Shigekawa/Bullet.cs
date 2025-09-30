using UnityEngine;

public class Bullet : MonoBehaviour
{
	[SerializeField] float bulletSpeed = 20f; // ’e‘¬
	[SerializeField] float lifeTime = 3f; // ’e‚Ìõ–½

	public void SetDirection(Vector3 direction)
	{
		// ’e‚ª”­Ë‚³‚ê‚½•ûŒü‚Éi‚Ş‚æ‚¤‚Éİ’è
		transform.forward = direction;
	}

	void Update()
	{
		// ‘O•û‚ÉˆÚ“®
		transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);

		// õ–½‚ª—ˆ‚½‚çíœ
		lifeTime -= Time.deltaTime;
		if (lifeTime <= 0)
		{
			Destroy(gameObject);
		}
	}

	// Õ“Ë”»’èi—áF“G‚É“–‚½‚Á‚½‚ç’e‚ğÁ‚·‚È‚Çj
	void OnTriggerEnter(Collider other)
	{
		 if (other.CompareTag("Enemy"))
		{
			Destroy(other.gameObject); // “G‚ğ”j‰ó
			Destroy(gameObject); // ’e‚ğ”j‰ó
		}
		Destroy(gameObject); // ‰½‚©‚É“–‚½‚Á‚½‚ç’e‚ğ”j‰ó
	}
}