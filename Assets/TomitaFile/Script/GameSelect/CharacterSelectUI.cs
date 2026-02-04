using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterSelectUI : MonoBehaviour
{
	SelectRecord m_record;

	Dictionary<PlayerInput,int>temSelect = new Dictionary<PlayerInput,int>();

	public void DecisionCharacter()
	{
		
	}

	private void Start()
	{
		m_record = GameObject.FindGameObjectWithTag("GameController").GetComponent<SelectRecord>();
	}

	private void Update()
	{
		// ‘Sˆõ‘I‘ðŠ®—¹‚µ‚½‚©
		if (m_record.GetDictionary().Count == m_record.GetMaxPlayerCount())
		{
			Debug.Log("[CharacterSelect] All Player READY");

			SceneChanger changer = GameObject.FindWithTag("SceneManager").GetComponent<SceneChanger>();
			changer.ChangeScene(m_record.GetMapID());
		}
	}
}
