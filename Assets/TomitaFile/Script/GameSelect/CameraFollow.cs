using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 8, -6);

    public void SetTarget(Transform t)
    {
        target = t;

        // --- 修正ポイント ---
        if (target != null)
        {
            // 1. まず位置を合わせる
            transform.position = target.position + offset;

            // 2. 生成した瞬間に一度だけプレイヤーの方向を向く
            // これにより、カメラが「斜め下」などの適切な角度に自動で設定されます
            transform.LookAt(target);

            // 3. (オプション) もし「左右の向き(Y軸)」は固定したい場合は、
            // ここで特定の角度を上書きしても良いです。
            // transform.rotation = Quaternion.Euler(45, 0, 0); 
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 位置は常に追従する（向きはSetTargetで決まった角度を維持）
        transform.position = target.position + offset;
    }
}