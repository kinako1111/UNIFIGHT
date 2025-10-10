using UnityEngine;

public class Homing : MonoBehaviour
{
	//生成時に別オブジェクトからステータスを入れられる
	GameObject m_target;
	int m_attackPower;
	GameObject m_effect;
	AudioClip m_se;
	

	[Header("弾速"),SerializeField]
	float speed = 10f;

	[Header("山なり軌道の高さ"),SerializeField]
	float arcHeight = 0f;

	void Start()
	{

	}

	void Update()
	{
		//ターゲットがいなくなった場合
		if (m_target == null) 
		{
			Destroy(gameObject);
		}

		// XZ方向のターゲット追尾
		Vector3 directionXZ = (m_target.transform.position - transform.position);
		directionXZ.y = 0;
		directionXZ.Normalize();
		Vector3 moveXZ = directionXZ * speed * Time.deltaTime;
		Vector3 nextPosition = transform.position + moveXZ;
		transform.position = nextPosition;
	}

	private void OnTriggerEnter(Collider other)
	{
		//衝突先がターゲットの場合
		if(other == m_target)
		{
			Status status;
			if (m_target.TryGetComponent(out status))
			{
				//ダメージ付与
				status.Damage(m_attackPower);

				//与えたダメージの表示


				//当たった位置でエフェクトの発生
				if (m_effect != null) return;
				Instantiate(m_effect, m_target.transform);

				//SE生成
				if (m_effect != null) return;
				SoundEffect.Play3D(m_se, m_target.transform.position);
			}
		}
	}

	public void SetStatus(GameObject newTarget,int attackPower,GameObject effect,AudioClip se)
	{
		m_target = newTarget;
		m_attackPower = attackPower;
		m_effect = effect;
		m_se = se;
	}
}
