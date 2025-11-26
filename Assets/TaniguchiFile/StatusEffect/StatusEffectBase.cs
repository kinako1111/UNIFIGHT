
using UnityEngine;

/// <summary>
/// 状態異常の共通処理を持つ基底クラス。
/// - 時間管理（duration / elapsed）
/// - Name / Icon の保持
/// - デフォルトの攻撃力修正は「何もしない」
/// 継承して、必要メソッドだけをオーバーライドします。
/// </summary>
public abstract class StatusEffectBase : IStatusEffect
{
	protected readonly string name;
	protected readonly Sprite icon;

	protected float duration;     // 総効果時間（秒）
	protected float elapsed;      // 経過時間（秒）

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="name">表示・識別用名称</param>
	/// <param name="duration">効果時間（秒）</param>
	/// <param name="icon">UI表示用アイコン（任意）</param>
	protected StatusEffectBase(string name, float duration, Sprite icon = null)
	{
		this.name = name;
		this.duration = Mathf.Max(0f, duration);
		this.icon = icon;
		this.elapsed = 0f;
	}

	/// <summary>
	/// デフォルトは攻撃力に影響なし。
	/// バフ/デバフは派生クラスでオーバーライドしてください。
	/// </summary>
	public virtual float ModifyAttackPower(float currentPower)
	{
		return currentPower;
	}

	/// <summary>
	/// 毎フレームの時間進行。
	/// DoTなどの逐次効果は派生クラスで処理します。
	/// </summary>
	public virtual void Tick(float deltaTime)
	{
		elapsed += deltaTime;
	}

	/// <summary>
	/// 効果時間切れ判定。
	/// duration==0 でも「即時適用型」として1フレームで期限切れになります。
	/// </summary>
	public bool IsExpired => elapsed >= duration;

	public string Name => name;

	public Sprite Icon => icon;

	public  float Duration => duration;

	// --- 補助: 残り時間（UI表示等用） ---
	public float RemainingTime => Mathf.Max(0f, duration - elapsed);
}
