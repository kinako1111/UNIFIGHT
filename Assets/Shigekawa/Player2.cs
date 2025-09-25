using UnityEngine;

public class Player2 : MonoBehaviour
{
	[SerializeField] float normalSpeed = 3f;     // 通常移動速度
	private readonly float gravity = 5f;         // 重力加速度
	private readonly float groundPush = -1f;     // 地面に押し付ける値
	private readonly float rotationSpeed = 10f;  // 回転補間速度

	CharacterController characterController;
	Animator animator;

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

		// 入力（X=左右, Z=前後）
		float inputX = Input.GetAxis("Horizontal");
		float inputZ = Input.GetAxis("Vertical");

		// 水平移動
		Vector3 horizontalMove = Vector3.zero;
		if (inputX != 0f || inputZ != 0f)
		{
			horizontalMove = (cameraForward * inputZ + cameraRight * inputX).normalized * speed;

			// 向きを変更（滑らか回転）
			Vector3 lookPos = transform.position + horizontalMove;
			Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
		}

		// 移動アニメーション（DampTimeを0にして即反映）
		float moveAmount = horizontalMove.magnitude;
		if (animator != null)
		{
			animator.SetFloat("MoveSpeed", moveAmount, 0f, Time.deltaTime);
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

		Vector3 verticalMove = new Vector3(0, verticalVelocity, 0);

		// 実際に移動
		characterController.Move((horizontalMove + verticalMove) * Time.deltaTime);
	}
}
