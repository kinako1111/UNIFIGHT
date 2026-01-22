using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoy : MonoBehaviour
{
	// 範囲確認用（必要に応じてGizmos表示などに使う）
	[Header("注目範囲"), SerializeField]
	Collider m_attentionCollider;

	[Header("消えるまでの時間"), SerializeField]
	float m_deathTimer = 10f;

	[Header("破壊時のエフェクト"), SerializeField]
	GameObject m_effect;

	[Header("破壊時の効果音"), SerializeField]
	AudioClip m_se;

	[Header("爆発のダメージ(固定ダメージ)"),SerializeField]
	int m_exprosionDamage = 0;

	// このデコイをターゲットしている敵のリスト
	List<EnemyAction> m_enemyList = new();

	private void Start()
	{
		// 時間経過で自壊
		Destroy(gameObject, m_deathTimer);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other == null) return;

		if (other.CompareTag("Enemy"))
		{
			if (other.TryGetComponent(out EnemyAction enemyAction))
			{
				// まだリストになければ追加
				if (!m_enemyList.Contains(enemyAction))
				{
					m_enemyList.Add(enemyAction);

					// 敵に「このデコイをターゲット候補に入れろ」と命令
					// ※第1引数の '0' は、敵側で自動ソートするなら無視してOK
					enemyAction.ChangeHate(0, gameObject);

					Debug.Log("デコイ発見: " + other.name);
				}
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other == null) return;

		if (other.CompareTag("Enemy"))
		{
			if (other.TryGetComponent(out EnemyAction enemyAction))
			{
				RemoveEnemyFromList(enemyAction);
			}
		}
	}

	private void OnDestroy()
	{
		// デコイが壊れた時、登録されている全敵のリストから自分を削除する
		foreach (EnemyAction enemy in m_enemyList)
		{
			// 【重要】敵が既に死んでいる可能性があるためnullチェック
			if (enemy != null)
			{
				enemy.RemoveHate(gameObject);

				//ステータス持ちにはダメージ
				Status status;
				if(enemy.gameObject.TryGetComponent(out status))
				{
					status.Damage(m_exprosionDamage);
				}
			}
		}

		// リストをクリア
		m_enemyList.Clear();

		// エフェクト生成（アプリ終了時などはエラーになることがあるのでチェック）
		if (gameObject.scene.isLoaded)
		{
			if (m_effect != null)
			{
				Instantiate(m_effect, transform.position, Quaternion.identity);
			}

			if (m_se != null)
			{
				// SoundEffectクラスの実装依存ですが、恐らくこれでOK
				SoundEffect.Play3D(m_se, transform.position);
			}
		}
	}

	// リストから安全に削除する処理
	void RemoveEnemyFromList(EnemyAction enemy)
	{
		if (m_enemyList.Contains(enemy))
		{
			// 敵側のリストからも自分（デコイ）を削除
			if (enemy != null)
			{
				enemy.RemoveHate(gameObject);
			}
			else
			{
				Debug.LogWarning("EnemyListにEnemyActionが存在しないObjectが紛れ込んでいます");
			}
			m_enemyList.Remove(enemy);
		}
	}
}