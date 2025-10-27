using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLockRotation : MonoBehaviour
{

	public Transform target;
	[SerializeField] Vector3 offset = new Vector3(0, 10, -5);

	void LateUpdate()
	{
		if(target != null)
		{
			transform.position = target.position + offset;
			transform.LookAt(target);
		}

		
	}
}
