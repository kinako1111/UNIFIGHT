using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLockRotation : MonoBehaviour
{

	[SerializeField] Transform target;
	[SerializeField] Vector3 offset = new Vector3(0, 10, -5);

	void LateUpdate()
	{
		transform.position = target.position + offset;
	}
}
