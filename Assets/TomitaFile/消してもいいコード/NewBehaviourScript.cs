using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour
{
	public float m_sceneTime;
	bool m_isscene;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
	

    // Update is called once per frame
    void Update()
    {
		if (m_isscene) return;

		m_sceneTime -= Time.deltaTime;

        if(m_sceneTime <= 0)
        {
            SceneManager.LoadScene("CharactorSelectionScene");
			m_isscene = true;
		}

	}
}
