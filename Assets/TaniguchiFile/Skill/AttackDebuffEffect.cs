
/// <summary>
/// UŒ‚—Í‚ğŒ¸­‚³‚¹‚éó‘ÔˆÙí
/// </summary>
public class AttackDebuffEffect : StatusEffectBase
{
	private float debuffRate; // UŒ‚—Í”{—¦i—áF0.5f‚Å”¼Œ¸j

	public AttackDebuffEffect(float duration, float debuffRate) : base("AttackDebuff", duration)
	{
		this.debuffRate = debuffRate;
	}

	public override void Tick(Status target, float deltaTime)
	{
		elapsedTime += deltaTime;
	}

	public override float ModifyAttackPower(float currentPower)
	{
		return currentPower * debuffRate; // UŒ‚—Í‚ğŒ¸­
	}
}
