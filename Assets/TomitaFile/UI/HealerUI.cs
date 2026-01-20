using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealerUI : MonoBehaviour
{
	private TextMeshProUGUI damageText;
	private float fadeOutSpeed = 1f;
	[SerializeField] private float moveSpeed = 0.4f;
	[SerializeField] private float randomOffsetRange = 0.5f; // X・Y方向のランダム範囲

	void Awake()
	{
		// TextMeshProUGUI を取得
		damageText = GetComponentInChildren<TextMeshProUGUI>();
	}

	public void SetDamage(int damage)
	{
		if (damageText != null)
		{
			damageText.text = damage.ToString();
		}

		// ランダムなオフセット（X:左右, Y:上下）※Z方向は0
		float offsetX = Random.Range(-randomOffsetRange, randomOffsetRange);
		float offsetY = Random.Range(-randomOffsetRange, randomOffsetRange);

		transform.position += new Vector3(offsetX, offsetY, 0f);
	}

	void LateUpdate()
	{
		if (damageText == null) return;

		// カメラの方向を向く（ビルボード）
		transform.rotation = Camera.main.transform.rotation;

		// 上に移動
		transform.position += Vector3.up * moveSpeed * Time.deltaTime;

		// 徐々に透明にする
		damageText.color = Color.Lerp(damageText.color, new Color(1f, 0f, 0f, 0f), fadeOutSpeed * Time.deltaTime);

		// 透明になったら削除
		if (damageText.color.a <= 0.1f)
		{
			Destroy(gameObject);
		}
	}
}
