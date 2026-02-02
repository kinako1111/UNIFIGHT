using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class AutoAttack : MonoBehaviour
{
	[Header("攻撃方法(弾の生成有無)"), SerializeField]
	bool m_CreateBullet = false;

	[Header("攻撃範囲"), SerializeField]
	float m_autoAttackRange;

	[Header("攻撃範囲(見た目)"), SerializeField]
	GameObject m_rangeLooks;

	[Header("攻撃の発生回数  ※デフォは1"),SerializeField]
	int m_autoAttackCount = 1;

	[Header("一度に攻撃できる数"), SerializeField]
	int m_simultaneous = 1;

	[Header("攻撃一ヒット当たりのダメージ倍率　※％で"), SerializeField]
	int m_magnification;

	[Header("攻撃速度"), SerializeField]
	float m_autoAttackInterval = 0.75f;

	[Header("弾のPrefab"), SerializeField]
	List<GameObject> m_bulletPrefab = new();

	[Header("弾の生成地点	※近接キャラなら無視してOK"), SerializeField]
	Transform m_generateTransform;

	[Header("攻撃がヒットした敵に出すエフェクト"), SerializeField]
	GameObject m_effect;

	[Header("再生するAudioClip"), SerializeField]
	AudioClip m_se;

	[Header("障害物レイヤー"), SerializeField]
	LayerMask m_obstacleLayer;

	//範囲内のUnitのリスト
	List<GameObject> m_unitList = new();

	PlayerInput m_playerInput;
	PlayerController m_playerController;
	Animator m_animator;
	Status m_status;

	List<GameObject> m_target = new();
	bool m_isAttack;
	private bool m_isFirePressed = false;
	int m_currentBullet = 0;
	float m_nextAttackTime = 0f;	//攻撃間隔のクールダウンを測る変数


	public bool IsAttack => m_isAttack;

	private void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>();
		m_animator = GetComponent<Animator>();
		m_status = GetComponent<Status>();
		m_playerController = GetComponent<PlayerController>();
	}

	private void Start()
	{
		m_isAttack = false;
		m_rangeLooks.transform.localScale = new Vector3(
			m_autoAttackRange * 2,
			m_rangeLooks.transform.localScale.y,
			m_autoAttackRange * 2
			);
	}

	void Update()
	{
		if (m_status.GetDeath()) return;
		// Fireボタンが押されているかを判定
		m_isFirePressed = m_playerInput.actions["Fire"].ReadValue<float>() > 0f;

		//Fireが押されている間、攻撃範囲を表示
		m_rangeLooks.SetActive( m_isFirePressed );

		// 連打しても m_nextAttackTime に到達するまでは攻撃しない
		if (m_isFirePressed)
		{
			TryExecuteAutoAttack();
		}

		// 弾の切り替え
		if (m_CreateBullet)
		{
			if (m_playerInput.actions["NextBullet"].triggered)
			{
				m_currentBullet += 1;
				if(m_currentBullet >= m_bulletPrefab.Count)
				{
					m_currentBullet = 0;
				}
				Debug.Log(m_currentBullet);
			}
			else if (m_playerInput.actions["BackBullet"].triggered)
			{
				m_currentBullet -= 1;
				//切り替え後、弾の数を下回ったら最後尾のバレットへ
				if (m_currentBullet < 0) m_currentBullet = m_bulletPrefab.Count -1;
				Debug.Log(m_currentBullet);
			}
		}
	}


	public void TryExecuteAutoAttack()
	{
		if (m_isAttack) return;
		if (m_playerController.ActionApproval() == false) return;	
		if (Time.time < m_nextAttackTime) return;

		m_unitList.Clear();
		m_target.Clear();

		Collider[] colliders = Physics.OverlapSphere(transform.position, m_autoAttackRange);

		var candidates = colliders
			.Select(col => col.gameObject)
			.Where(obj => obj != this.gameObject && obj.CompareTag("Enemy"))
			.OrderBy(obj => (transform.position - obj.transform.position).sqrMagnitude)
			.ToList();

		if (candidates.Count == 0) return;

		foreach (var enemy in candidates)
		{
			if (m_target.Count >= m_simultaneous) break;

			// Raycastで障害物チェック
			Vector3 direction = (enemy.transform.position - transform.position).normalized;
			float distance = Vector3.Distance(transform.position, enemy.transform.position);

			if (!Physics.Raycast(transform.position, direction, distance, m_obstacleLayer))
			{
				// 障害物がない場合のみターゲットに追加
				m_target.Add(enemy);
			}
		}

		if (m_target.Count == 0) return;

		m_animator.SetTrigger("AutoAttack");
		m_isAttack = true;
		m_nextAttackTime = Time.time + m_autoAttackInterval;
	}

	public void OnAttackStart()
	{
		//アニメーションスタートのタイミングで呼び出す
		// ターゲット方向を向く
		Vector3 direction = (m_target.First().transform.position - transform.position).normalized;
		direction.y = 0; // 水平方向のみ
		if (direction != Vector3.zero)
		{
			Quaternion targetRotation = Quaternion.LookRotation(direction);
			targetRotation *= Quaternion.Euler(0, 90f, 0);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f);
		}
	}


	public void CloseAttack()
	{
		StartCoroutine(RepeatDamageCoroutine());
	}

	IEnumerator RepeatDamageCoroutine()
	{
		float rangeSqr = m_autoAttackRange * m_autoAttackRange;

		for (int i = 0; i < m_autoAttackCount; i++)
		{
			// 1. ターゲットが空でないか、最初の要素がDestroyされていないかチェック
			if (m_target.Count == 0 || m_target.First() == null)
			{
				yield break; // ターゲットが消えたらコルーチン終了
			}

			GameObject firstTarget = m_target.First();

			// 2. 距離チェック（ターゲットが生きていれば座標にアクセス可能）
			if ((transform.position - firstTarget.transform.position).sqrMagnitude <= rangeSqr)
			{
				if (firstTarget.TryGetComponent(out Status enemyStatus))
				{
					// ダメージ付与
					enemyStatus.Damage(m_status.GetAttackPower() * m_magnification / 100);

					// エフェクト生成（ターゲットの現在位置）
					if (m_effect != null)
					{
						Instantiate(m_effect, firstTarget.transform.position, transform.rotation);
					}

					// SE再生
					if (m_se != null)
					{
						SoundEffect.Play3D(m_se, firstTarget.transform.position);
					}
				}
			}

			// 攻撃間隔待機
			yield return new WaitForSeconds(m_autoAttackInterval);
		}
	}

	public void FarBulletAttack()
	{
		//遠距離
		foreach (GameObject target in m_target)
		{
			if (m_bulletPrefab[m_currentBullet] != null)
			{
				//弾を生成
				GameObject bullet = Instantiate(m_bulletPrefab[m_currentBullet], m_generateTransform.position, Quaternion.identity);

				//弾のターゲットをAAのターゲットに設定
				bullet.GetComponent<Homing>().SetStatus(target,m_status.GetAttackPower(),m_effect,m_se);
			}
		}
	}

	public void OnAttackEnd()
	{
		m_isAttack = false;
	}
}
