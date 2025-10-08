using UnityEngine;

public class Homing : MonoBehaviour
{
	GameObject target;
	public float speed = 10f;

	public float arcHeight = 0f;

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
		Vector3 directionXZ = (target.transform.position - transform.position);
		directionXZ.y = 0;
		directionXZ.Normalize();

		Vector3 moveXZ = directionXZ * speed * Time.deltaTime;

		Vector3 nextPosition = transform.position + moveXZ;

		transform.position = nextPosition;
	}

	public void SetTarget(GameObject newTarget)
	{
		target = newTarget;
		Debug.Log(newTarget.gameObject);
	}
}
