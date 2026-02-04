using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BlockCursor : MonoBehaviour
{
	// 対象のAction
	[SerializeField]  InputActionProperty _buttonAction;

	private void OnEnable()
	{
		// Input ActionのperformedコールバックにOnButtonActionを登録
		_buttonAction.action.performed += OnButtonAction;
		_buttonAction.action.Enable();
	}

	private void OnDisable()
	{
		// Input ActionのperformedコールバックからOnButtonActionを削除
		_buttonAction.action.performed -= OnButtonAction;
		_buttonAction.action.Disable();
	}

	// Input Actionのperformedコールバック
	public void OnButtonAction(InputAction.CallbackContext context)
	{
		// UIの上にカーソルがあったら、入力を受け付けない
		if (EventSystem.current.IsPointerOverGameObject()) return;

		print("ボタンが押された！");
	}
}
