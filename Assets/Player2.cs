using UnityEngine;

public class Player2 : MonoBehaviour
{
	[SerializeField] float normalSpeed = 3f; // 通常移動速度
	private readonly float gravity = 5f; // 重力加速度
	private readonly float groundPush = -1f;           // 地面に押し付ける値
	private readonly float rotationSpeed = 10f;        // 回転補間速度

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

		// カメラ基準の前後・左右
		Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
		Vector3 cameraRight = Camera.main.transform.right;

		// 入力
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		// 移動方向
		Vector3 move = (cameraForward * v + cameraRight * h) * speed;

		// 移動アニメーション
		float moveAmount = move.magnitude;
		// animator.SetFloat("MoveSpeed", moveAmount);

		// 入力があれば向きを変更（滑らか回転）
		if (moveAmount > 0.001f)
		{
			Vector3 lookPos = transform.position + move;
			Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
		}

		// 重力処理
		if (characterController.isGrounded)
		{
			verticalVelocity = groundPush; // 地面に押し付ける
		}
		else
		{
			verticalVelocity -= gravity * Time.deltaTime;
		}

		moveDirection = new Vector3(move.x, verticalVelocity, move.z);

		// 実際に移動
		characterController.Move(moveDirection * Time.deltaTime);
	}
}
