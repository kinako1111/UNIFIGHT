using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
	[Header("スモークの効果時間 (秒)"), SerializeField]
	float m_duration = 5.0f; // スモークが持続する時間

	[Header("敵の移動速度減少率 (%)"), SerializeField]
	[Range(0f, 100f)]
	float m_slowPercentage = 50f; // 50%減速 (例)

	[Header("効果範囲の半径"), SerializeField]
	float m_effectRadius = 3.0f; // スモークの効果が及ぶ範囲

	[Header("影響を与えるレイヤー"), SerializeField]
	LayerMask m_enemyLayer; // 敵のレイヤーを設定

	private List<Status> m_affectedEnemies = new List<Status>(); // 影響を受けている敵のリスト

	void Start()
	{
		// スモークの持続時間後に自身を破棄
		Destroy(gameObject, m_duration);
	}

	void FixedUpdate()
	{
		// 定期的に範囲内の敵をチェックし、減速効果を適用
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_effectRadius, m_enemyLayer);

		List<Status> currentEnemiesInRange = new List<Status>();

		foreach (var hitCollider in hitColliders) 
		{
			Status enemyStatus = hitCollider.GetComponent<Status>();
			if (enemyStatus != null && !enemyStatus.GetDeath()) // 死亡していない敵のみ
			{
				currentEnemiesInRange.Add(enemyStatus);

				// まだ影響を受けていない敵であれば効果を適用
				if (!m_affectedEnemies.Contains(enemyStatus))
				{
					ApplySlowEffect(enemyStatus);
					m_affectedEnemies.Add(enemyStatus);
				}
			}
		}

		// 範囲外に出た敵から効果を解除
		for (int i = m_affectedEnemies.Count - 1; i >= 0; i--)
		{
			if (m_affectedEnemies[i] == null || !currentEnemiesInRange.Contains(m_affectedEnemies[i]) || m_affectedEnemies[i].GetDeath())
			{
				RemoveSlowEffect(m_affectedEnemies[i]);
				m_affectedEnemies.RemoveAt(i);
			}
		}
	}

	void ApplySlowEffect(Status enemyStatus)
	{
		float speedMultiplier = 1f - (m_slowPercentage / 100f);
		enemyStatus.ApplySpeedModifier(speedMultiplier, this); // this を引数に渡すことで、どのオブジェクトが効果を与えているか識別できる
	}

	void RemoveSlowEffect(Status enemyStatus)
	{
		// 敵がnullでないか、まだ生きているかチェック
		if (enemyStatus != null && !enemyStatus.GetDeath())
		{
			enemyStatus.RemoveSpeedModifier(this);
		}
	}

	// スモークが破棄される際に、まだ影響を受けている敵から効果を解除
	void OnDestroy()
	{
		for (int i = m_affectedEnemies.Count - 1; i >= 0; i--)
		{
			if (m_affectedEnemies[i] != null && !m_affectedEnemies[i].GetDeath()) // 敵が既に破壊されていないか、死亡していないかチェック
			{
				RemoveSlowEffect(m_affectedEnemies[i]);
			}
		}
		m_affectedEnemies.Clear();
	}
}