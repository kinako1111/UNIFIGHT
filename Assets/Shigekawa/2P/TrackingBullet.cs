using UnityEngine;

public class TrackingBullet : MonoBehaviour
{
	private GameObject m_target; // この弾が追跡する特定のターゲット
	private int m_damage; // ターゲットに与えるダメージ量

	[SerializeField] float m_speed = 10f; // 弾の移動速度
	[SerializeField] float m_trackingStrength = 5f; // ターゲットへの追尾の強さ（数値が大きいほど素早く向きを変える）
	public void SetStatus(GameObject target, int damage)
	{
		m_target = target; // Turretが選定したターゲットを設定
		m_damage = damage;
	}

	void Update()
	{
		// ターゲットが設定されていない、または無効なオブジェクトになっている場合は、この弾を破壊する
		if (m_target == null || !m_target.activeSelf)
		{
			Destroy(gameObject); // ターゲットを見失ったので弾は消滅
			return;
		}

		// 現在の弾の位置からターゲットの位置への方向ベクトルを計算
		Vector3 directionToTarget = (m_target.transform.position - transform.position).normalized;

		// ターゲットの方向に向くように、弾の回転を調整
		Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
		// 現在の回転から目標の回転へ、徐々に（TrackingStrengthの速さで）補間する
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_trackingStrength * Time.deltaTime);

		// 弾を自身の前方方向へ移動させる
		transform.Translate(Vector3.forward * m_speed * Time.deltaTime);
	}

	// 他のColliderと物理的に衝突したときに呼ばれる（この弾と相手のCollider両方にRigidbodyが必要）
	private void OnCollisionEnter(Collision collision)
	{
		// 衝突した相手が「Enemy」タグを持つオブジェクトであるかを確認
		if (collision.gameObject.CompareTag("Enemy"))
		{
			Status targetStatus;
			// 衝突したオブジェクトがStatusコンポーネントを持っているか試みる
			if (collision.gameObject.TryGetComponent(out targetStatus))
			{
				// ダメージを与える
				targetStatus.Damage(m_damage);
				Debug.Log(collision.gameObject.name + "に" + m_damage + "ダメージを与えた！");
			}

			// 弾は役目を終えたので、自身を破壊する
			Destroy(gameObject);
		}
		// もし敵ではない他のオブジェクト（例: 壁、地面など）に当たった場合も消したいなら、ここに処理を追加
		else
		{
			// ここでは敵以外に当たった場合も弾を破壊する例
			Destroy(gameObject);
		}
	}
}