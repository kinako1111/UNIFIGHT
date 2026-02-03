using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoChange : MonoBehaviour
{
	[SerializeField] private GameObject[] m_characterSelect;
	private int index = 0;
	void Start()
	{
		m_characterSelect[0].SetActive(true);
	}
	public void Right()
	{
		m_characterSelect[index].SetActive(false);
		index++;
		if (index >= m_characterSelect.Length)
			index = 0;
		m_characterSelect[index].SetActive(true);
	}
	public void Left()
	{
		m_characterSelect[index].SetActive(false);
		index--;
		if (index < 0)
			index = m_characterSelect.Length - 1;
		m_characterSelect[index].SetActive(true);
	}
}
