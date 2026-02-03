using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using JetBrains.Annotations;
using UnityEngine.InputSystem;

public class SelectiManager : MonoBehaviour
{
	[SerializeField] GameSelectionData gameData;
	[SerializeField] GameObject playerPrefab;
	[SerializeField] Camera cameraPrefab;
	[SerializeField] Transform[] spawnPoints;

	List<Camera> cameras = new List<Camera>();

	private void Start()
	{
		int playerCount = gameData.selectedCharacterIds.Count;

		for(int i = 0; i < playerCount; i++)
		{
			int characterId = gameData.selectedCharacterIds[i];

			// プレイヤーを生成
			GameObject player = Instantiate(
				playerPrefab,
				spawnPoints[i].position,
				Quaternion.identity
				);

			// キャラ反映
			player.GetComponent<PlayerSetup>().ApplyCharacter(characterId);

			//カメラ生成
			Camera cam = Instantiate(cameraPrefab);
			cam.GetComponent<CameraFollow>().target = player.transform;

			// PlayerInputにカメラ割り当て
			player.GetComponent<PlayerInput>().camera = cam;

			// AudioListenterは1Pのみ
			cam.GetComponent<AudioListener>().enabled = (i == 0);

			cameras.Add(cam);
		}

		// 分割設定
		FindObjectOfType<CameraSplitManager>().Setup(cameras);
	}
}
