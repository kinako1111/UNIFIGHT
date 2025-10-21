using Unity.Properties;
using UnityEngine;

public class Homing : MonoBehaviour
{	
	[Header("弾速"),SerializeField]
	float speed = 10f;

	[Header("山なり軌道の高さ"),SerializeField]
	float arcHeight = 0f;

	[Header("消えるまでの時間"), SerializeField]
	int m_deathTime = 3;

	//生成時に別オブジェクトからステータスを入れられる
	GameObject m_target;
	int m_attackPower;
	GameObject m_effect;
	AudioClip m_se;
	Rigidbody m_rb;

	private void Awake()
	{
		m_rb = GetComponent<Rigidbody>();
	}

	void Start()
	{
		//一定時間以内にヒットしないと破壊
		Destroy(gameObject, 3);
	}

	void Update()
	{
		//ターゲットがいなくなった場合いらないので破棄
		if (m_target == null) 
		{
			Debug.Log("ターゲットがいません");
			Destroy(gameObject);
		}

		//追跡処理
		// ターゲット方向のXZベクトルを計算
		Vector3 direction = m_target.transform.position - transform.position;
		direction.y = 0; // Y軸の動きを無視
		direction.Normalize();
		// Rigidbody に速度を設定
		m_rb.velocity = direction * speed;
	}

	private void OnTriggerEnter(Collider other)
	{
		//敵よりも先に自身の射出場所の当たり判定と衝突している
		Debug.Log("OnTriggerEnter 呼び出し");
		Debug.Log("衝突したオブジェクト: " + other);
		Debug.Log("ターゲット: " + m_target.name);

		//衝突先がターゲットの場合
		if (other.gameObject == m_target)
		{
			Debug.Log("衝突先がターゲットと同じ");
			Status status;
			if (m_target.TryGetComponent(out status))
			{
				Debug.Log("ステータススクリプト取得");
				//ダメージ付与
				status.Damage(m_attackPower);

				//当たった位置でエフェクトの発生
				if(m_effect != null)
				{
					Instantiate(m_effect, m_target.transform);
				}

				if(m_effect != null)
				{
					//SE生成
					SoundEffect.Play3D(m_se, m_target.transform.position);
				}

				//役目を終えたので破壊
				Destroy(gameObject);
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
