
using UnityEngine;

/// <summary>
/// バフ系スキルの実装例
/// ISkillインターフェースを実装し、スキル発動時に対象へ状態異常を付与する
/// </summary>
public class Buff : MonoBehaviour, ISkill
{
	[SerializeField] GameObject m_prefab = null;       // スキルのエフェクトプレハブ（必要なら使用）
	[SerializeField] float m_cooldownTime = 5f;        // クールダウン時間
	[SerializeField] string m_skillName = "□□□";      // スキル名
	[SerializeField] SkillType m_skillType;            // スキルタイプ（Point, Direction, Selfなど）
	[SerializeField] GameObject m_skillUI;             // スキルUI（範囲表示など）
	[SerializeField] float m_skillRangeX;              // スキル範囲X
	[SerializeField] float m_skillRangeZ;              // スキル範囲Z
	[SerializeField] float m_skillDistance;            // スキル距離

	// ISkillインターフェースのプロパティ実装
	public string SkillName => m_skillName;
	public float CoolDownTime => m_cooldownTime;
	public SkillType SkillType => m_skillType;
	public GameObject SkillUI => m_skillUI;
	public float SkillRangeX => m_skillRangeX;
	public float SkillRangeZ => m_skillRangeZ;
	public float SkillDistance => m_skillDistance;

	/// <summary>
	/// スキル発動時の処理
	/// position: 発動位置
	/// rotation: 発動方向
	/// target: 対象オブジェクト（プレイヤーや敵など）
	/// </summary>
	public void Execute(Vector3 position, Quaternion rotation, GameObject target)
	{
		// 対象のStatusEffectManagerを取得
		var manager = target.GetComponent<StatusEffectManager>();
		if (manager == null)
		{
			Debug.LogWarning("対象にStatusEffectManagerがありません");
			return;
		}

		// 攻撃力バフを付与（5秒間、攻撃力1.5倍）
		manager.AddEffect(new AttackBuffEffect(5f, 1.5f));

		// デバッグログ
		Debug.Log($"{target.name} に攻撃力バフを付与しました（5秒間、1.5倍）");

		// 必要ならエフェクトを生成
		if (m_prefab != null)
		{
			Instantiate(m_prefab, position, rotation);
		}
	}
}
