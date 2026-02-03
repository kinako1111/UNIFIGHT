using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
	[SerializeField] GameObject[] characterModels;
	[SerializeField] Transform visualRoot;

	public void ApplyCharacter(int characterId)
	{
		GameObject character =
		Instantiate(characterModels[characterId], visualRoot);

		// š‚±‚±‚ÅƒJƒƒ‰‚ÉTarget‚ğ“n‚·
		CameraFollow camFollow =
			GetComponentInChildren<CameraFollow>();

		camFollow.SetTarget(character.transform);

	}
}
