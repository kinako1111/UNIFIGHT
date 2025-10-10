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
	List<GameObject> m_target;
	Status m_status;

	bool m_isAttack;

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
	}

	private void OnEnable()
	{
		m_playerInput.actions["Fire"].performed += OnFire;
	}

	private void OnDisable()
	{
		m_playerInput.actions["Fire"].performed -= OnFire;
	}

	public void OnFire(InputAction.CallbackContext callback)
	{
		//ユニットリストを一度空にする
		m_unitList .Clear();

		Collider[] colliders = Physics.OverlapSphere(transform.position, m_autoAttackRange);

		// 自分との距離が近い順にソートして GameObject を取得
		m_unitList = colliders
			.Select(col => col.gameObject)
			.Where(obj => obj != this.gameObject && obj.CompareTag("Enemy")) // 自分以外 & ユニットのみ
			.OrderBy(obj => (transform.position - obj.transform.position).sqrMagnitude) // 距離の二乗でソート（高速）
			.ToList();

		//↓↓↓↓動かん
		//ユニットの数が０


		//攻撃範囲内にユニットがいるか  //magnitudeは重く平方根の計算を避けるためsqrMagnitudeを使う
		if (m_unitList.Count == 0) return;
	
		//AAアニメーション再生
		m_animator.SetTrigger("AutoAttack");

		//攻撃中の変数
		m_isAttack = true;

		//AAのターゲットを設定
		for(int i = 0; i < m_simultaneous; i++)
		{
			m_target.Add(m_unitList[i]);
			Debug.Log(m_unitList[i]);
		}
	}

	public void CloseAttack()
	{
		//近接
		//アニメーションの攻撃の当たる瞬間に呼び出す

		//対象キャラがまだ攻撃範囲内にいる
		float rangeSqr = m_autoAttackRange * m_autoAttackRange;
		if((transform.position - m_target.First().transform.position).sqrMagnitude <= rangeSqr)
		{
			Status status;
			if(m_target.First().TryGetComponent(out status))
			{
				for(int i = 0; i < m_autoAttackCount; i++)
				{
					//ダメージ付与
					status.Damage(m_status.GetAttackPower());

					//与えたダメージの表示


					//当たった位置でエフェクトの発生
					if (m_effect != null) return;
					Instantiate(m_effect,m_target.First().transform);

					//SE生成
					if (m_effect != null) return;
					SoundEffect.Play3D(m_se, m_target.First().transform.position);
				}
			}
		}
	}

	public void FarAttack()
	{
		//遠距離

		foreach(GameObject target in m_target)
		{
			//弾を生成
			GameObject bullet = Instantiate(m_bulletPrefab[0], m_generateTransform.position, Quaternion.identity);

			//SE生成
			SoundEffect.Play3D(m_se, transform.position);

			//弾のターゲットをAAのターゲットに設定
			bullet.GetComponent<Homing>().SetStatus(target,m_status.GetAttackPower(),m_effect,m_se);
		}
	}

	public void AttackEnd()
	{
		m_isAttack = false;
	}
}
