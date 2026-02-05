using System.Collections.Generic;
using UnityEngine;

public enum DamageKinds
{
    Attack,   // 攻撃（赤）
    Heal,     // 回復（緑）
    Special   // その他（黄・青など）
}
public class TakeDamage : MonoBehaviour
{
    [SerializeField] GameObject damageUIPrefab;
    [SerializeField] GameObject healUIPrefab;
    [SerializeField] GameObject specialUIPrefab;
    List<GameObject> m_damageUI = new();

    // ★ 自分の担当カメラを保持する変数
    private Camera myPlayerCamera;

    // ★ GameSceneManager から呼ばれるアタッチ用メソッド
    public void SetPlayerCamera(Camera cam)
    {
        myPlayerCamera = cam;
        Debug.Log($"{gameObject.name} にカメラがアタッチされました: {cam.name}");
    }

    private void Start()
    {
        m_damageUI.Add(damageUIPrefab);
        m_damageUI.Add(healUIPrefab);
        m_damageUI.Add(specialUIPrefab);
    }

    public void ShowDamageUI(int damage, DamageKinds kinds = DamageKinds.Attack)
    {
        var position = transform.position + Vector3.up * 3f;
        var obj = Instantiate(m_damageUI[(int)kinds], position, Quaternion.identity);

        var damageUI = obj.GetComponent<DamageUI>();
        if (damageUI != null)
        {
            // ★ 生成した UI に、保持しているカメラを渡す
            damageUI.SetTargetCamera(myPlayerCamera);
            damageUI.SetDamage(damage);
        }

        // 念のため
        Destroy(obj, 3f);
    }
}