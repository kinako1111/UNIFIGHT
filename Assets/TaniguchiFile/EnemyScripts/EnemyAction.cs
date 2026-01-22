using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro; // 必要に応じて
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAction : MonoBehaviour
{
	// 攻撃の種類（現状は物理攻撃のみ）
	enum AttackList
	{
		PhysicalAttack,
	}

	[Header("ステータス")]
	[SerializeField] Status status;

	[Header("NavMesh")]
	[SerializeField] NavMeshAgent m_navmeshAgent;

	[Header("エフェクトリスト")]
	[SerializeField] List<GameObject> m_effectList = new();

	[Header("攻撃判定発生位置")]
	[SerializeField] List<Transform> m_attackPos = new();

	[Header("サウンドリスト")]
	[SerializeField] List<AudioClip> m_soundList = new();

	[Header("攻撃範囲")]
	[SerializeField] float AttackRange = 2.0f;

	[Header("自身のコライダー")]
	[SerializeField] Collider m_collider;

	[Header("攻撃用コライダー")]
	[SerializeField] List<Collider> m_colliderList = new();

	[Header("攻撃クールタイム")]
	[SerializeField] float m_attackCoolTime = 2.0f;

	[Header("ターゲットリスト（デコイ・防衛対象・プレイヤー）")]
	[SerializeField] List<GameObject> m_targetList = new();

	// UI関連
	[SerializeField] Slider m_slider;

	// 内部変数
	Animator m_animator;
	Status m_status; // 自身のStatusコンポーネントキャッシュ
	float m_coolTime;
	bool isMove;
	bool isAttack = false;
	bool m_damageAction = false;

	// 回転速度（スムーズな方向転換用）
	float rotationSpeed = 5.0f;

	private void Awake()
	{
		m_animator = GetComponent<Animator>();
		m_status = GetComponent<Status>();

		//攻撃のクールタイム
		m_coolTime = m_attackCoolTime;

		// 初期状態でシーンにいるプレイヤーや防衛対象をリストに入れる
		// （動的に増減する場合はChangeHate/RemoveHateで管理）
		AddTargetsByTag("Player");
		AddTargetsByTag("Target");
	}

	// 指定タグのオブジェクトをまとめてリストに追加するヘルパー
	private void AddTargetsByTag(string tagName)
	{
		GameObject[] objects = GameObject.FindGameObjectsWithTag(tagName);
		foreach (var obj in objects)
		{
			if (!m_targetList.Contains(obj))
			{
				m_targetList.Add(obj);
			}
		}
	}

	private void FixedUpdate()
	{
		// 1. 死亡判定
		if (status.GetDeath())
		{
			HandleDeath();
			return;
		}

		// 2. UI更新
		UpdateUI();

		// 3. ターゲット選定ロジック（ここがAIの頭脳）
		// リストから消滅したオブジェクト(null)を除去
		m_targetList.RemoveAll(t => t == null);

		// 優先順位（デコイ＞ターゲット＞プレイヤー）に従って現在の標的を決める
		GameObject currentTarget = GetPriorityTarget();

		// ターゲットがいない場合は待機
		if (currentTarget == null)
		{
			isMove = false;
			m_navmeshAgent.isStopped = true;
			m_animator.SetBool("Walk", false);
			return;
		}

		// 4. 行動分岐（攻撃か移動か）
		float dist = Vector3.Distance(currentTarget.transform.position, transform.position);

		if (dist <= AttackRange)
		{
			PerformAttackState(currentTarget);
		}
		else
		{
			PerformMoveState(currentTarget);
		}

		// 移動アニメーション反映
		m_animator.SetBool("Walk", isMove);
	}

	// --- コアロジック: 優先順位に基づいたターゲット取得 ---
	GameObject GetPriorityTarget()
	{
		// 1. デコイを探す (どこにいても最優先)
		GameObject decoy = GetClosestObjectWithTag("Decoy");
		if (decoy != null) return decoy;

		// 2. プレイヤーが「攻撃範囲内」にいるかチェック
		// 範囲内にプレイヤーがいれば、拠点を無視してプレイヤーを狙う
		GameObject nearbyPlayer = GetClosestObjectWithTag("Player");
		if (nearbyPlayer != null)
		{
			float distToPlayer = Vector3.Distance(transform.position, nearbyPlayer.transform.position);
			if (distToPlayer <= AttackRange)
			{
				return nearbyPlayer;
			}
		}

		// 3. 範囲内にプレイヤーがいない、またはデコイがないなら「防衛対象」を狙う
		GameObject baseTarget = GetClosestObjectWithTag("Target");
		if (baseTarget != null) return baseTarget;

		// 4. 防衛対象すらなくなったら、遠くにいるプレイヤーでも追いかける
		//　使わないと思うけど一応
		if (nearbyPlayer != null) return nearbyPlayer;

		return null;
	}

	// 指定タグの中で一番近いオブジェクトを探す
	GameObject GetClosestObjectWithTag(string tagName)
	{
		GameObject closest = null;
		float minDist = float.MaxValue;

		foreach (var t in m_targetList)
		{
			if (t != null && t.CompareTag(tagName))
			{
				float d = Vector3.Distance(transform.position, t.transform.position);
				if (d < minDist)
				{
					minDist = d;
					closest = t;
				}
			}
		}
		return closest;
	}

	// --- 行動ステート ---

	void PerformAttackState(GameObject target)
	{
		isMove = false;
		m_navmeshAgent.isStopped = true;

		// 攻撃中でなければターゲットの方を向く
		if (!isAttack)
		{
			Vector3 dir = (target.transform.position - transform.position).normalized;
			dir.y = 0; // 上下方向の回転は防ぐ
			if (dir != Vector3.zero)
			{
				Quaternion targetRot = Quaternion.LookRotation(dir);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
			}
		}

		// クールタイム消化
		m_coolTime -= Time.deltaTime;
		if (m_coolTime < 0)
		{
			OnAttack();
		}
	}

	void PerformMoveState(GameObject target)
	{
		// ダメージモーション中などでなければ移動
		if (!isAttack && !m_damageAction)
		{
			isMove = true;
			m_navmeshAgent.isStopped = false;
			m_navmeshAgent.speed = m_status.GetSpeed();

			// 負荷軽減のため、目的地が大きく変わった時だけセットする
			if (Vector3.Distance(m_navmeshAgent.destination, target.transform.position) > 0.5f)
			{
				m_navmeshAgent.SetDestination(target.transform.position);
			}
		}
		else
		{
			isMove = false;
			m_navmeshAgent.isStopped = true;
		}
	}

	void HandleDeath()
	{
		m_navmeshAgent.isStopped = true;
		if (m_collider != null) m_collider.enabled = false;
		// 死亡アニメーションなどが再生されている前提
	}

	void UpdateUI()
	{
		if (m_slider != null)
		{
			// MaxValueはStart時などに1回設定するのが望ましいが、変化する可能性を考慮してここでも可
			m_slider.maxValue = status.GetMaxHp();
			m_slider.value = status.GetHp();
		}
	}

	// --- 攻撃アニメーションイベント・処理 ---

	void OnAttack()
	{
		if (isAttack) return;
		isAttack = true;
		m_animator.SetTrigger("Attack");
		// 次の攻撃までの時間をセット
		m_coolTime = m_attackCoolTime;
	}

	// Animation Eventから呼ばれる: 攻撃判定発生
	public void AttackHit()
	{
		int index = (int)AttackList.PhysicalAttack;
		if (index >= m_colliderList.Count) return;

		// コライダー有効化
		if (m_colliderList[index] != null)
		{
			m_colliderList[index].enabled = true;
		}

		// エフェクト生成
		if (index < m_effectList.Count && m_effectList[index] != null)
		{
			Instantiate(m_effectList[index], m_attackPos[index].transform);
		}

		// SE再生
		if (index < m_soundList.Count && m_soundList[index] != null)
		{
			SoundEffect.Play3D(m_soundList[index], m_attackPos[index].transform.position);
		}
	}

	// Animation Eventから呼ばれる: 攻撃判定終了
	public void AttackHitEnd()
	{
		int index = (int)AttackList.PhysicalAttack;
		if (index < m_colliderList.Count && m_colliderList[index] != null)
		{
			m_colliderList[index].enabled = false;
		}
	}

	// Animation Eventから呼ばれる: 攻撃モーション終了
	public void AttackEnd()
	{
		isAttack = false;
	}

	// --- 外部からの制御・イベント ---

	public void Damagefirst()
	{
		m_damageAction = true;
		m_navmeshAgent.isStopped = true;
	}

	public void DamageEnd()
	{
		m_damageAction = false;
	}

	public void SetSlider(Slider slider)
	{
		m_slider = slider;
	}

	// Decoyや外部スクリプトから呼ばれるヘイト管理
	// order引数は互換性のために残すが、優先順位はタグで自動判定するため無視してAddする
	public void ChangeHate(int order, GameObject target)
	{
		if (target != null && !m_targetList.Contains(target))
		{
			m_targetList.Add(target);
			// リストに追加さえすれば、FixedUpdateのGetPriorityTargetが
			// 次のフレームで自動的に優先度(Decoy > Target > Player)を判断します
		}
	}

	// ターゲットが破壊されたり、範囲外に出た時に呼ばれる
	public void RemoveHate(GameObject target)
	{
		if (m_targetList.Contains(target))
		{
			m_targetList.Remove(target);
		}
	}
}