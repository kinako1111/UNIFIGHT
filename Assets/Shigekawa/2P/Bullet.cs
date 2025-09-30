using UnityEngine;

public class Bullet : MonoBehaviour
{
	// [SerializeField] float bulletSpeed = 20f; // ← この行はPlayerController1で管理するため不要になりました
	[SerializeField] float lifeTime = 3f;

	private float _currentBulletSpeed; // PlayerController1から受け取った弾速を格納する変数

	// 弾の初期設定を行うメソッド
	public void Initialize(Vector3 direction, float speed)
	{
		transform.forward = direction;
		_currentBulletSpeed = speed; // 受け取った弾速を設定
	}

	void Update()
	{
		// 前方に移動 (_currentBulletSpeed を使用)
		transform.Translate(Vector3.forward * _currentBulletSpeed * Time.deltaTime);

		lifeTime -= Time.deltaTime;
		if (lifeTime <= 0)
		{
			Destroy(gameObject);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		// 敵に当たった場合の処理などをここに追加
		// 現状はColliderに触れたら消滅
		Destroy(gameObject);
	}
}