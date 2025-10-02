using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P : MonoBehaviour
{
    private CharacterController m_characterController;
    private Vector3 m_moveVelocity;

	[SerializeField] Status.Name m_status;

	[SerializeField]
	Unit m_unit;

	UnitData unitData;

    // Start is called before the first frame update
    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
		Debug.Log(unitData.Speed);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		//Wキーがおされたら
		if (Input.GetKey(KeyCode.W))
		{
			m_characterController.Move(this.gameObject.transform.forward * unitData.Speed * Time.deltaTime);
		}
		//Sキーがおされたら
		if (Input.GetKey(KeyCode.S))
		{
			m_characterController.Move(this.gameObject.transform.forward * -1f * unitData.Speed * Time.deltaTime);
		}
		//Aキーがおされたら
		if (Input.GetKey(KeyCode.A))
		{
			m_characterController.Move(this.gameObject.transform.right * -1 * unitData.Speed * Time.deltaTime);
		}
		//Dキーがおされたら
		if (Input.GetKey(KeyCode.D))
		{
			m_characterController.Move(this.gameObject.transform.right * unitData.Speed * Time.deltaTime);
		}


		// キャラクターを動かす
		m_characterController.Move(m_moveVelocity * Time.deltaTime);
	}
}
