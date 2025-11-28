
/// <summary>
/// 継続ダメージの属性（UIや耐性、ログ分類のために用意）
/// </summary>
public enum DamageType
{
	Generic,  // 汎用
	Fire,     // 火傷
	Bleed,    // 出血
	Curse,    // 呪い
	Poison,   // 毒（※名前は使うがロジックは共通）
	Frost,    // 凍傷
	Acid      // 酸
}
