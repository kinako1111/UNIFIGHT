using TMPro;
using UnityEngine;

public class DamageUI : MonoBehaviour
{
    private TextMeshProUGUI damageText;
    [SerializeField] private float fadeOutSpeed = 2f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float randomOffsetRange = 0.5f;

    private Transform _camTransform;

    void Awake()
    {
        damageText = GetComponentInChildren<TextMeshProUGUI>();
    }

    // カメラを外部からセットするメソッドを追加
    public void SetTargetCamera(Camera cam)
    {
        if (cam != null) _camTransform = cam.transform;
    }

    public void SetDamage(int damage)
    {
        if (damageText != null)
        {
            damageText.text = damage.ToString();
        }

        transform.position += new Vector3(
            Random.Range(-randomOffsetRange, randomOffsetRange),
            Random.Range(-randomOffsetRange, randomOffsetRange),
            0f
        );
    }

    void LateUpdate()
    {
        if (damageText == null) return;

        // 1. 移動はカメラがなくても実行
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. 回転はカメラがある時だけ実行
        if (_camTransform != null)
        {
            transform.rotation = _camTransform.rotation;
        }

        // 3. フェードアウトは常に実行
        Color textColor = damageText.color;
        textColor.a -= fadeOutSpeed * Time.deltaTime;
        damageText.color = textColor;

        // 4. 削除判定（これで確実に消えるようになります）
        if (textColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}