
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterType
{
	AssaultRifle,
	ShotGun,
	Poison,
	Sniper,
}

[System.Serializable]
public class Character
{
	public CharacterType type;
	public GameObject characterObject;
}

public class CharacterSelector : MonoBehaviour
{
	[SerializeField] List<Character> characters;

	// 各ボタンをインスペクターで設定
	[SerializeField] Button assaultButton;
	[SerializeField] Button shotgunButton;
	[SerializeField] Button poisonButton;
	[SerializeField] Button sniperButton;

	void Start()
	{
		// ボタンにイベントを登録
		assaultButton.onClick.AddListener(() => SelectCharacter(CharacterType.AssaultRifle));
		shotgunButton.onClick.AddListener(() => SelectCharacter(CharacterType.ShotGun));
		poisonButton.onClick.AddListener(() => SelectCharacter(CharacterType.Poison));
		sniperButton.onClick.AddListener(() => SelectCharacter(CharacterType.Sniper));

		// 初期表示（例：アサルト）
		SelectCharacter(CharacterType.AssaultRifle);
	}

	public void SelectCharacter(CharacterType selectedType)
	{
		Debug.Log("押された");

		foreach (var character in characters)
		{
			character.characterObject.SetActive(character.type == selectedType);
		}

	}
}
