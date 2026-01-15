using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPrint : MonoBehaviour
{
    Image ChargeImage;
    // Start is called before the first frame update
    void Start()
    {
		ChargeImage = GetComponent<Image>();
	}
	 
	// Update is called once per frame
	public void UpdateClock(float charge)
	{
		//Žó‚¯Žæ‚Á‚½floatŒ^‚Ì’l‚ð‘ã“ü‚·‚é
		ChargeImage.fillAmount = charge;
	}
}
