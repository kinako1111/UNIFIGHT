using TMPro;
using UnityEngine;

public class DamageUI : MonoBehaviour
{
	private TextMeshProUGUI damageText;
	[SerializeField] private float fadeOutSpeed = 2f; // 少し早めが気持ちいいです
	[SerializeField] private float moveSpeed = 1.5f;
	[SerializeField] private float randomOffsetRange = 0.5f;

	private Transform _camTransform;

	void Awake()
	{
		damageText = GetComponentInChildren<TextMeshProUGUI>();
		// カメラのTransformをキャッシュ
		if (Camera.main != null) _camTransform = Camera.main.transform;
	}

	public void SetDamage(int damage)
	{
		if (damageText != null)
		{
			damageText.text = damage.ToString();
		}

		// 初期位置にランダム性を持たせる
		transform.position += new Vector3(
			Random.Range(-randomOffsetRange, randomOffsetRange),
			Random.Range(-randomOffsetRange, randomOffsetRange),
			0f
		);
	}

	void LateUpdate()
	{
		if (damageText == null || _camTransform == null) return;

		// 1. カメラの方向を向く（ビルボード）
		transform.rotation = _camTransform.rotation;

		// 2. 上に移動
		transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

		// 3. アルファ値だけを下げる（元の色を維持）
		Color textColor = damageText.color;
		textColor.a -= fadeOutSpeed * Time.deltaTime;
		damageText.color = textColor;

		// 4. 完全に透明になったら削除
		if (textColor.a <= 0)
		{
			Destroy(gameObject);
		}
	}
}