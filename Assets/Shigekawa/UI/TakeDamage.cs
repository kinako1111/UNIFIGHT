using UnityEngine;
using System.Collections;

public class TakeDamage : MonoBehaviour
{
	//　DamageUIプレハブ
	[SerializeField]
	GameObject damageUI;

	public void Damage(Collider col)
	{
		//　DamageUIをインスタンス化。登場位置は接触したコライダの中心からカメラの方向に少し寄せた位置
		var obj = Instantiate<GameObject>(damageUI, col.bounds.center - Camera.main.transform.forward * 0.2f, Quaternion.identity);
	}
}
