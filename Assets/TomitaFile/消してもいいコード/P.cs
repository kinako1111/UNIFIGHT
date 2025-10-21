using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class P : MonoBehaviour
{
    private CharacterController m_characterController;
    private Vector3 m_moveVelocity;

	//[SerializeField] Status m_status;
	private float speed = 4f;

	[SerializeField] Slider m_playerSlider;

	[SerializeField] TextMeshProUGUI m_hpText;

    // Start is called before the first frame update
    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		//m_playerSlider.maxValue = m_status.maxHp;
		//m_playerSlider.value = m_status.GetHp();
		//m_hpText.text = m_status.GetHp().ToString() + " / " + m_status.maxHp.ToString();

		//Wキーがおされたら
		if (Input.GetKey(KeyCode.W))
		{
			m_characterController.Move(this.gameObject.transform.forward * speed * Time.deltaTime);
		}
		//Sキーがおされたら
		if (Input.GetKey(KeyCode.S))
		{
			m_characterController.Move(this.gameObject.transform.forward * -1f * speed * Time.deltaTime);
		}
		//Aキーがおされたら
		if (Input.GetKey(KeyCode.A))
		{
			m_characterController.Move(this.gameObject.transform.right * -1 * speed * Time.deltaTime);
		}
		//Dキーがおされたら
		if (Input.GetKey(KeyCode.D))
		{
			m_characterController.Move(this.gameObject.transform.right * speed * Time.deltaTime);
		}


		// キャラクターを動かす
		m_characterController.Move(m_moveVelocity * Time.deltaTime);
	}

	//private void OnControllerColliderHit(ControllerColliderHit hit)
	//{
	//	int hitDamage = 10;
	//	int heal = 1;

	//	if(hit.gameObject.CompareTag("Enemy"))
	//	{
	//		if (m_status.GetHp() <= 0) return;
	//		m_status.Damage(hitDamage);
	//	}

	//	if(hit.gameObject.CompareTag("Heal"))
	//	{
	//		if(m_status.GetMaxHp() <= m_status.GetHp()) return;
	//		m_status.Heal(heal);
	//	}
	//}
}
