using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class GameSceneManager : MonoBehaviour
{
	SelectRecord m_selectRecord;

	[SerializeField] GameObject[] m_playerPrefab;
	[SerializeField] Transform[] m_genelatePos;
	[SerializeField] Camera cameraPrefab;

	[SerializeField] InputActionAsset m_actionAsset; // ← これを追加
	[SerializeField] string m_actionMapName = "PlayerInput";

	void Start()
	{
		m_selectRecord = GameObject.FindWithTag("GameController").GetComponent<SelectRecord>();
		var selectionDict = m_selectRecord.GetDictionary();

		int index = 0;
		foreach (KeyValuePair<InputDevice,int> entry in m_selectRecord.GetDictionary())
		{
			InputDevice device = entry.Key; // 今度は InputDevice が取得できる
			int charId = entry.Value;

			GameObject playerObj = Instantiate(m_playerPrefab[charId], m_genelatePos[index].position, Quaternion.identity);

			// PlayerInputの設定
			PlayerInput newPlayerInput = playerObj.GetComponent<PlayerInput>();
			if (newPlayerInput != null)
			{
				newPlayerInput.actions = m_actionAsset;

				// 保存しておいたデバイスをペアリングする
				InputUser.PerformPairingWithDevice(device, newPlayerInput.user);

				newPlayerInput.enabled = true;
			}

			// SkillActivation を書き換えた場合の Setup 呼び出し
			var skill = playerObj.GetComponent<SkillActivation>();
			if (skill != null)
			{
				skill.Setup(newPlayerInput, Instantiate(cameraPrefab));
			}

			index++;
		}
	}
}
