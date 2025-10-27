using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SelectiManager : MonoBehaviour
{
    [SerializeField] GameSelectionData gameData;
    [SerializeField] GameObject[] characterPrefabs;

    private GameObject playerInstance;
    // Start is called before the first frame update
    void Start()
    {
        int characterId = gameData.selectedCharacterId;

        // ÉvÉåÉCÉÑÅ[Çê∂ê¨
        playerInstance = Instantiate(characterPrefabs[characterId], new Vector3(0, 1, -2), Quaternion.identity);

		CameraLockRotation cameraLock = Camera.main.GetComponent<CameraLockRotation>();
        cameraLock.target = playerInstance.transform;

	}
}
