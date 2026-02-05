using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNumber : MonoBehaviour
{


	// Update is called once per frame
	void Update()
    {
        if(GameObject.FindGameObjectsWithTag("Player").Length <= 0)
        {
			SceneChanger changer = GameObject.FindWithTag("SceneManager").GetComponent<SceneChanger>();
			changer.ChangeScene("LoseScene");
		}
    }
}
