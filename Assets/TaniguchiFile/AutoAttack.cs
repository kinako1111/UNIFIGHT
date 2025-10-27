using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class AutoAttack : MonoBehaviour
{
	[Header("攻撃範囲"), SerializeField]
	float m_autoAttackRange;

	[Header("攻撃範囲(見た目)"), SerializeField]
	GameObject m_rangeLooks;

	[Header("攻撃の発生回数  ※デフォは1"),SerializeField]
	int m_autoAttackCount = 1;

	[Header("一度に攻撃できる数"), SerializeField]
	int m_simultaneous = 1;

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
	private Coroutine m_attackCoroutine;


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

		// 押されたら攻撃開始
		if (m_isFirePressed && m_attackCoroutine == null)
		{
			m_attackCoroutine = StartCoroutine(RepeatAttack());
		}

		// 離されたら攻撃停止
		if (!m_isFirePressed && m_attackCoroutine != null)
		{
			StopCoroutine(m_attackCoroutine);
			m_attackCoroutine = null;
		}
	}

	IEnumerator RepeatAttack()
	{
		while (true)
		{
			// 攻撃処理（OnFire の中身を関数化して呼び出す）
			ExecuteAutoAttack();
			yield return new WaitForSeconds(m_autoAttackInterval); // 攻撃間隔（調整可能）
		}
	}

	private void ExecuteAutoAttack()
	{
		if (m_isAttack) return;

		m_unitList.Clear();
		m_target.Clear();

		Collider[] colliders = Physics.OverlapSphere(transform.position, m_autoAttackRange);

		m_unitList = colliders
			.Select(col => col.gameObject)
			.Where(obj => obj != this.gameObject && obj.CompareTag("Enemy"))
			.OrderBy(obj => (transform.position - obj.transform.position).sqrMagnitude)
			.ToList();

		if (m_unitList.Count == 0) return;

		m_animator.SetTrigger("AutoAttack");
		m_isAttack = true;

		for (int i = 0; i < m_simultaneous && i < m_unitList.Count; i++)
		{
			m_target.Add(m_unitList[i]);
		}
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
					// ダメージ付与
					status.Damage(m_status.GetAttackPower());

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


	public void FarAttack()
	{
		//遠距離
		foreach (GameObject target in m_target)
		{
			if (m_bulletPrefab[0] != null)
			{
				//弾を生成
				GameObject bullet = Instantiate(m_bulletPrefab[0], m_generateTransform.position, Quaternion.identity);

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
