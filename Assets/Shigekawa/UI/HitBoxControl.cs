using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxControl : MonoBehaviour
{
	[SerializeField] GameObject _targets;
	[SerializeField] GameObject _canvas;
	[SerializeField] GameObject _damageUI;

	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.CompareTag("Bullet"))
		{
			PopDamageUI();
		}
	}

	void PopDamageUI()
	{
		var obj = new GameObject("Target");
		var ui = Instantiate(_damageUI);

		obj.transform.SetParent(_targets.transform);		// ターゲット＆テキストUIの親をそれぞれ変更
		ui.transform.SetParent(_canvas.transform);

		ui.SetActive(true);

		var circlePos = Random.insideUnitCircle * 1.2f;
		obj.transform.position = transform.position + Vector3.up * Random.Range(3.0f, 4.0f) + new Vector3(circlePos.x, 0, circlePos.y); // ターゲットの位置をランダムで指定する
		ui.GetComponent<RectTransform>().position = RectTransformUtility.WorldToScreenPoint(Camera.main,obj.transform.position);		// + UIの位置をターゲットの画面上の位置に指定する

		Destroy(obj, 5.0f);
		Destroy(ui, 5.0f);
	}

}
