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

	//範囲内のUnitのリスト
	List<GameObject> m_unitList = new();

	PlayerInput m_playerInput;
	Animator m_animator;
	List<GameObject> m_target = new();
	Status m_status;
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
		// Fireボタンが押されているかを判定
		m_isFirePressed = m_playerInput.actions["Fire"].ReadValue<float>() > 0f;

		//Fireが押されている間、攻撃範囲を表示
		m_rangeLooks.SetActive( m_isFirePressed );


		// ★ 連打しても m_nextAttackTime に到達するまでは攻撃しない
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

	IEnumerator RepeatAttack()
	{
		while (true)
		{
			// 攻撃処理（OnFire の中身を関数化して呼び出す）
			TryExecuteAutoAttack();
			yield return new WaitForSeconds(m_autoAttackInterval); // 攻撃間隔（調整可能）
		}
	}


	// ★ ExecuteAutoAttack をクールダウン判定付きの Try に変更
	private void TryExecuteAutoAttack()
	{
		// 攻撃中は発動しない（アニメ中の多重発動防止）
		if (m_isAttack) return;

		// クールダウン：次に攻撃できる時刻になるまで発動しない
		if (Time.time < m_nextAttackTime) return;

		m_unitList.Clear();
		m_target.Clear();

		Collider[] colliders = Physics.OverlapSphere(transform.position, m_autoAttackRange);

		m_unitList = colliders
			.Select(col => col.gameObject)
			.Where(obj => obj != this.gameObject && obj.CompareTag("Enemy"))
			.OrderBy(obj => (transform.position - obj.transform.position).sqrMagnitude)
			.ToList();

		if (m_unitList.Count == 0) return;

		for (int i = 0; i < m_simultaneous && i < m_unitList.Count; i++)
		{
			m_target.Add(m_unitList[i]);
		}

		// 実際に攻撃を発動
		m_animator.SetTrigger("AutoAttack");
		m_isAttack = true;

		// ★ 次の攻撃可能時刻をセット（ここが重要）
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
			if ((transform.position - m_target.First().transform.position).sqrMagnitude <= rangeSqr)
			{
				Status status;
				if (m_target.First().TryGetComponent(out status))
				{
					// ダメージ付与			※ここの値は１００分率
					status.Damage(m_status.GetAttackPower() *m_magnification /100);

					// ダメージ表示
					Debug.Log(m_status.GetAttackPower() + "ダメージを与えた！");

					// エフェクト
					if (m_effect != null)
					{
						Instantiate(m_effect, m_target.First().transform);
					}

					// SE
					if (m_se != null)
					{
						SoundEffect.Play3D(m_se, m_target.First().transform.position);
					}
				}
			}

			// 攻撃間隔待機
			yield return new WaitForSeconds(m_autoAttackInterval);
		}

		// 攻撃終了はアニメーションイベントで呼ばれるので、ここでは呼ばない
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
