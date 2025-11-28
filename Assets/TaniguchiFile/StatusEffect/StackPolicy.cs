public enum DotStackPolicy
{
	KeepStrongest,   // 上限超過時は強いものを優先（弱いものは捨てる）
	ReplaceWeakest,  // 最弱DOTを新DOTで置換
	StackIntoWeakest // 最弱DOTにスタック（dmg/tick加算 等）
					 // 必要なら ExtendLongest / RefreshShortest 等を追加
}
