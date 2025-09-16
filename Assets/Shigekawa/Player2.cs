using UnityEngine;

public class Player2 : MonoBehaviour
{
	[SerializeField] float normalSpeed = 3f; // �ʏ�ړ����x
	private readonly float gravity = 5f; // �d�͉����x
	private readonly float groundPush = -1f;           // �n�ʂɉ����t����l
	private readonly float rotationSpeed = 10f;        // ��]��ԑ��x

	CharacterController characterController;
	Animator animator;

	Vector3 moveDirection = Vector3.zero;
	float verticalVelocity = 0f;

	void Start()
	{
		characterController = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();
	}

	void Update()
	{
		float speed = normalSpeed;

		// �J������̑O��E���E
		Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
		Vector3 cameraRight = Camera.main.transform.right;

		// ����
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		// �ړ�����
		Vector3 move = (cameraForward * v + cameraRight * h) * speed;

		// �ړ��A�j���[�V����
		float moveAmount = move.magnitude;
		// animator.SetFloat("MoveSpeed", moveAmount);

		// ���͂�����Ό�����ύX�i���炩��]�j
		if (moveAmount > 0.001f)
		{
			Vector3 lookPos = transform.position + move;
			Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
		}

		// �d�͏���
		if (characterController.isGrounded)
		{
			verticalVelocity = groundPush; // �n�ʂɉ����t����
		}
		else
		{
			verticalVelocity -= gravity * Time.deltaTime;
		}

		moveDirection = new Vector3(move.x, verticalVelocity, move.z);

		// ���ۂɈړ�
		characterController.Move(moveDirection * Time.deltaTime);
	}
}
