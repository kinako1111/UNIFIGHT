using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputManagerEntry;

[CreateAssetMenu(menuName = "Game/StatusEffect Data", fileName = "NewStatusEffectData")]
public class StatusEffectData : ScriptableObject
{
	[Header("識別")]
	[SerializeField] private string key = "poison_numa";    //識別
	[SerializeField] private string displayName = "沼毒"; //表示名
	[SerializeField, TextArea(3,10)] private string skillExplanation = "毎秒攻撃力の10％のダメージを受ける";
	[SerializeField] private EffectKind kind = EffectKind.DamageOverTime;

	[Header("スタック運用")]
	[SerializeField] private StackMode stackMode = StackMode.None; 
	[SerializeField] private int maxStacks = 0;
	[SerializeField] private int stacksPerApply = 0;

	[Header("非スタック再付与時の方針")]
	[SerializeField] private NonStackReapplyPolicy nonStackPolicy = NonStackReapplyPolicy.RefreshDuration;

	[Header("時間・Tick")]
	[SerializeField] private float durationSeconds = 5f;
	[SerializeField] private float tickInterval = 1.0f;

	[Header("参照ステータス/係数")]
	[SerializeField] private ReferStat referStat = ReferStat.AttackPower;
	[SerializeField] private bool referAttacker = true;//trueなら攻撃者のステータス参照、falseなら対象者のステータスを参照
	[SerializeField] private float scaleRate = 25;   //与える効果の大きさ
	//バフの場合、使用者の攻撃力の200％バフ　-> referAttacker = true,scaleRate -> 2
	//持続ダメ、使用者の攻撃力20％毎秒ダメージ　-> referAttacker = true,scaleRate -> 0.2

	[Header("見た目")]
	[SerializeField] private Sprite icon; // UIアイコン
	[SerializeField] private GameObject vfxPrefab; // エフェクト
	[SerializeField] private AudioClip sfx; // サウンド
	[SerializeField] private Color uiColor = Color.green;

	[Header("DOT/HOTオプション")]
	[SerializeField] private bool applyImmediateFirstTick = false;

	// --- Public Getter ---
	public string Key => key;
	public string DisplayName => displayName;
	public EffectKind Kind => kind;
	public StackMode StackMode => stackMode;
	public int MaxStacks => Mathf.Max(1, (stackMode == StackMode.None) ? 1 :maxStacks);
	public int StacksPerApply => Mathf.Max(1, stacksPerApply);
	public NonStackReapplyPolicy NonStackPolicy => nonStackPolicy;
	public float DurationSeconds => Mathf.Max(0.01f, durationSeconds);
	public float TickInterval => Mathf.Max(0.01f, tickInterval);
	public ReferStat ReferStat => referStat;
	public bool ReferAttacker => referAttacker;
	public float ScaleRate => scaleRate;
	public Sprite Icon => icon;
	public GameObject VfxPrefab => vfxPrefab;
	public AudioClip Sfx => sfx;
	public Color UIColor => uiColor;
	public bool ApplyImmediateFirstTick => applyImmediateFirstTick;
}
