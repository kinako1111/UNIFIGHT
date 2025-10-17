using TMPro;
using UnityEngine;

public class DamageUI : MonoBehaviour
{
	private TextMeshProUGUI damageText;
	private float fadeOutSpeed = 1f;
	[SerializeField] private float moveSpeed = 0.4f;

	public void SetDamage(int damage)
	{
		damageText = GetComponentInChildren<TextMeshProUGUI>();
		damageText.text = damage.ToString();
	}

	void LateUpdate()
	{
		transform.rotation = Camera.main.transform.rotation;
		transform.position += Vector3.up * moveSpeed * Time.deltaTime;

		damageText.color = Color.Lerp(damageText.color, new Color(1f, 0f, 0f, 0f), fadeOutSpeed * Time.deltaTime);

		if (damageText.color.a <= 0.1f)
		{
			Destroy(gameObject);
		}
	}
}