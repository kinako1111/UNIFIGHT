
/// <summary>
/// UŒ‚—Í‚ğ‘‰Á‚³‚¹‚éó‘ÔˆÙí
/// </summary>
public class AttackBuffEffect : StatusEffectBase
{
	private float buffRate; // UŒ‚—Í”{—¦i—áF1.5f‚Å50%ƒAƒbƒvj

	public AttackBuffEffect(float duration, float buffRate) : base("AttackBuff", duration)
	{
		this.buffRate = buffRate;
	}

	public override void Tick(Status target, float deltaTime)
	{
		elapsedTime += deltaTime; // ŠÔŒo‰ß
	}

	public override float ModifyAttackPower(float currentPower)
	{
		return currentPower * buffRate; // UŒ‚—Í‚ğ”{—¦‚ÅC³
	}
}
