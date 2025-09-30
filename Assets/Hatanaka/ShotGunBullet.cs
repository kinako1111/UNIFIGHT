using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGunBullet : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] float bulletSpeed;
    [SerializeField] float deleteTime;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
	public void SetDirection(Vector3 direction)
	{
		// ’e‚ª”­Ë‚³‚ê‚½•ûŒü‚Éi‚Ş‚æ‚¤‚Éİ’è
		transform.forward = direction;
	}
	// Update is called once per frame
	void Update()
    {
		transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
		deleteTime -= Time.deltaTime;
        damage -= Time.deltaTime;
        if(deleteTime<0)
        {
            Destroy(this.gameObject);
        }
    }

	private void OnTriggerEnter(Collider other)
	{
		if(CompareTag("Enemy"))
        {
			Destroy(other.gameObject); // “G‚ğ”j‰ó
			Destroy(gameObject); // ’e‚ğ”j‰ó
		}
	}
}
