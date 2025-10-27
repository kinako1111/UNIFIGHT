using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [SerializeField] Status m_status;
    [SerializeField] Slider m_slider;
    [SerializeField] TextMeshProUGUI m_text;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

}
