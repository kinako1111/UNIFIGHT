using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutoAttack : MonoBehaviour
{
	[Header("攻撃範囲"), SerializeField]
	float m_autoAttackRange;

	[Header("近接か遠距離か")]
	bool m_closeRange;

	[Header("一度に攻撃できる数"), SerializeField]
	int m_simultaneous;

	[Header("弾のプレファブ"),SerializeField]
	List<GameObject> m_bulletPrefab = new();

	[Header("Unitリストの取得用"),SerializeField]
	UnitManager m_unitManager;

	[Header("弾の生成地点"), SerializeField]
	Transform m_generateTransform;

	//範囲内のUnitのリスト
	List<GameObject> m_unitList = new();

	PlayerInput m_playerInput;
	Animator m_animator;


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
		//AAアニメーション再生
		m_animator.SetTrigger("AutoAttack");

		//ユニットリストを取得
		m_unitList.AddRange(m_unitManager.GetUnitList());

		//自分を除外
		m_unitList.Remove(this.gameObject);

		//攻撃範囲内にいるユニットがいない
		if (m_unitList.Find(target => (transform.position - target.transform.position).magnitude <= m_autoAttackRange) == null) return; 

		//AAのターゲットを設定
		GameObject target = m_unitList.Find(target => (transform.position - target.transform.position).magnitude <= m_autoAttackRange);

		//弾を生成
		GameObject bullet = Instantiate(m_bulletPrefab[0], m_generateTransform.position, Quaternion.identity);

		//弾のターゲットをAAのターゲットに設定
		bullet.GetComponent<Homing>().SetTarget(target);

		m_unitList.Clear();
	}

	private void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>();
		m_animator = GetComponent<Animator>();
	}

	private void Start()
	{
	}

	private void FixedUpdate()
	{
			
	}

	public void CloseAttack()
	{
		//近接なら範囲内の敵にダメージを与える
		
	}

	public void FarAttack()
	{
		//遠距離なら弾を生成
		Instantiate(m_bulletPrefab[0], m_generateTransform.position, Quaternion.identity);
	}
}
