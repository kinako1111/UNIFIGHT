using UnityEngine;

public class HomingP : MonoBehaviour
{
	Transform target;
	public float speed = 10f;
	public float arcHeight = 5f;

	private Vector3 startPosition;
	private float elapsedTime = 0f;

	void Start()
	{
		startPosition = transform.position;
	}

	void Update()
	{
		if (target == null) return;

		elapsedTime += Time.deltaTime;

		// XZ方向のターゲット追尾
		Vector3 directionXZ = (target.position - transform.position);
		directionXZ.y = 0;
		directionXZ.Normalize();

		Vector3 moveXZ = directionXZ * speed * Time.deltaTime;

		// Y方向の放物線（時間ベース）
		float heightOffset = arcHeight * Mathf.Sin(elapsedTime * Mathf.PI / 2f); // 0→最大→0の放物線を描く

		Vector3 nextPosition = transform.position + moveXZ;
		nextPosition.y = startPosition.y + heightOffset;

		transform.position = nextPosition;
	}
}
