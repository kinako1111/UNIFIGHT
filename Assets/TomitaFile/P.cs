using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P : MonoBehaviour
{
    private CharacterController m_characterController;
    private Vector3 m_moveVelocity;

    [SerializeField] float m_moveSpeed;


    // Start is called before the first frame update
    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		//W�L�[�������ꂽ��
		if (Input.GetKey(KeyCode.W))
		{
			m_characterController.Move(this.gameObject.transform.forward * m_moveSpeed * Time.deltaTime);
		}
		//S�L�[�������ꂽ��
		if (Input.GetKey(KeyCode.S))
		{
			m_characterController.Move(this.gameObject.transform.forward * -1f * m_moveSpeed * Time.deltaTime);
		}
		//A�L�[�������ꂽ��
		if (Input.GetKey(KeyCode.A))
		{
			m_characterController.Move(this.gameObject.transform.right * -1 * m_moveSpeed * Time.deltaTime);
		}
		//D�L�[�������ꂽ��
		if (Input.GetKey(KeyCode.D))
		{
			m_characterController.Move(this.gameObject.transform.right * m_moveSpeed * Time.deltaTime);
		}


		// �L�����N�^�[�𓮂���
		m_characterController.Move(m_moveVelocity * Time.deltaTime);
	}
}
