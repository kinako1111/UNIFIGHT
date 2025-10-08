using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutoAttack : MonoBehaviour
{
	[Header("UŒ‚”ÍˆÍ"), SerializeField]
	float m_autoAttackRange;

	[Header("‹ßÚ‚©‰“‹——£‚©")]
	bool m_clossRange;

	[SerializeField]
	List<GameObject> m_bulletPrefab = new();

	//”ÍˆÍ“à‚ÌUnit‚ÌƒŠƒXƒg
	List<GameObject> m_unitList = new();

	PlayerInput m_playerInput;


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

	}

	private void Awake()
	{
		
	}

	private void Start()
	{
		m_unitList.AddRange(GameObject.FindGameObjectsWithTag("Player"));
	}

	private void FixedUpdate()
	{
			
	}
}
