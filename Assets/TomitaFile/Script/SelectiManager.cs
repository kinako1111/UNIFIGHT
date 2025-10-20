using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectiManager : MonoBehaviour
{
    [SerializeField] GameSelectionData gameData;
    [SerializeField] GameObject[] characterPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        int characterId = gameData.selectedCharacterId;

        Instantiate(characterPrefabs[characterId], new Vector3(0, 1, -2), Quaternion.identity);
    }
}
