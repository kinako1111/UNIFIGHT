
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// プレイヤーにアタッチして使用する「スキル発動管理」
/// 入力 → 準備 → 発動/キャンセル、クールタイム、UI連動を一括管理します。
/// </summary>
public class SkillActivation : MonoBehaviour
{
	[Header("スキル一覧 ※プレイヤーにアタッチする等の実体化必須"), SerializeField]
	List<MonoBehaviour> skillComponents;
	// ↑ Inspector で ISkill を実装したコンポーネント（MonoBehaviour）を並べる。
	//   ※ interface は Unity の標準シリアライズ対象外なので MonoBehaviour として受け取り、
	//      Awake で ISkill にキャストして使用する。

	[Header("スキル一覧"), SerializeField]
	List<ISkill> skills = new List<ISkill>();
	// ↑ 実際に使う ISkill のリスト。skillComponents から抽出して格納。

	// ─────────────────────────────────────────────────
	// スキルとSkillPrintの対応付け（UIバインディング）
	// どの ISkill に、どのクールタイムUI(SkillPrint)を当てるかを Inspector で設定できるようにする。
	// 順番は skillComponents と一致していなくてもOK。辞書で解決する。
	// ─────────────────────────────────────────────────
	[System.Serializable]
	public class SkillUIBinding
	{
		[SerializeField] private MonoBehaviour skillComponent; // ISkill 実装を持つコンポーネント
		[SerializeField] private SkillPrint skillPrint;     // クールタイムUI（Image.fillAmount を更新）

		// 外部からは読み取り専用でアクセスする
		public ISkill Skill => skillComponent as ISkill;
		public SkillPrint Print => skillPrint;

		/// <summary>
		/// Inspector 設定の妥当性チェック（エラーメッセージ付き）
		/// </summary>
		public bool IsValid(out string error)
		{
			if (skillComponent == null)
			{
				error = "[SkillUIBinding] skillComponent が未設定です。";
				return false;
			}
			if (Skill == null)
			{
				error = $"[SkillUIBinding] {skillComponent.name} は ISkill を実装していません。";
				return false;
			}
			if (skillPrint == null)
			{
				error = $"[SkillUIBinding] {skillComponent.name} に対応する SkillPrint が未設定です。";
				return false;
			}
			error = null;
			return true;
		}
	}

	[Header("スキルとUIの紐付け（同じ順番でなくてもOK）"), SerializeField]
	List<SkillUIBinding> uiBindings = new List<SkillUIBinding>();
	// ↑ ISkill (MonoBehaviour) と SkillPrint のペアを複数登録する。

	// ─────────────────────────────────────────────────
	// 状態管理用のフィールド
	// ─────────────────────────────────────────────────
	int currentSkillIndex = 0; // 現在「準備中」のスキルインデックス
	bool m_approvalSkill;       // 準備状態フラグ（ボタン押下中など）

	PlayerInput m_playerInput;  // 新Input System のエントリポイント
	Animator m_animator;     // 発動時のアニメ再生などに使用

	// 個別クールタイム管理：各 ISkill ごとの残り時間（秒）
	Dictionary<ISkill, float> cooldownTimers = new Dictionary<ISkill, float>();

	// UI 反映用：ISkill → SkillPrint の対応辞書
	Dictionary<ISkill, SkillPrint> skillToPrint = new Dictionary<ISkill, SkillPrint>();

	// 入力イベントを解除するためのデリゲート参照を保持（ラムダを毎回生成しない）
	System.Action<InputAction.CallbackContext> onSkill1Performed;
	System.Action<InputAction.CallbackContext> onSkill2Performed;

	// ─────────────────────────────────────────────────
	// 初期化：コンポーネント取得、ISkill 抽出、UIバインディング作成、入力デリゲート用意
	// ─────────────────────────────────────────────────
	private void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>();
		m_animator = GetComponent<Animator>();

		// 1) skillComponents から ISkill を抽出して skills へ格納、クールタイム初期化
		foreach (MonoBehaviour comp in skillComponents)
		{
			if (comp is ISkill skill)
			{
				skills.Add(skill);
				cooldownTimers[skill] = 0f; // 0f = 即使用可
			}
			else
			{
				// MonoBehaviour だが ISkill 未実装の場合は警告
				Debug.LogWarning($"{comp.name} は ISkill を実装していません。");
			}
		}

		// 2) UI バインディング：uiBindings から ISkill → SkillPrint の辞書を構築
		skillToPrint.Clear();
		foreach (var binding in uiBindings)
		{
			if (binding == null) continue;

			// Inspector 設定の妥当性をチェック（未設定や未実装を早期検出）
			if (!binding.IsValid(out var err))
			{
				Debug.LogWarning(err);
				continue;
			}

			var s = binding.Skill; // ISkill 参照（読み取り専用）
			var sp = binding.Print; // SkillPrint 参照（読み取り専用）

			if (!skillToPrint.ContainsKey(s))
			{
				skillToPrint.Add(s, sp);
				// 起動時は「使用可 = 0」に見せる
				sp.UpdateClock(0f);
			}
		}

		// 3) 入力イベント用デリゲート（解除で同参照を使えるよう、フィールドに保持）
		onSkill1Performed = ctx => OnPreparation(0); // Skill1 入力でスキル0を準備
		onSkill2Performed = ctx => OnPreparation(1); // Skill2 入力でスキル1を準備
	}

	// ─────────────────────────────────────────────────
	// 入力イベント購読（OnEnable）／解除（OnDisable）
	// ─────────────────────────────────────────────────
	private void OnEnable()
	{
		// performed: ボタン押下で「準備開始」
		m_playerInput.actions["Skill1"].performed += onSkill1Performed;
		m_playerInput.actions["Skill2"].performed += onSkill2Performed;

		// canceled: ボタン解放で「発動試行」（準備中かつCT0であれば発動）
		m_playerInput.actions["Skill1"].canceled += OnReleasedSkill;
		m_playerInput.actions["Skill2"].canceled += OnReleasedSkill;

		// キャンセル専用ボタン：準備解除（UIを隠す）
		m_playerInput.actions["SkillCancel"].performed += OnSkillCancel;
	}

	private void OnDisable()
	{
		// OnEnable と同じ参照を使って必ず解除すること（匿名ラムダを都度生成しない）
		m_playerInput.actions["Skill1"].performed -= onSkill1Performed;
		m_playerInput.actions["Skill2"].performed -= onSkill2Performed;

		m_playerInput.actions["Skill1"].canceled -= OnReleasedSkill;
		m_playerInput.actions["Skill2"].canceled -= OnReleasedSkill;
		m_playerInput.actions["SkillCancel"].performed -= OnSkillCancel;
	}

	// ─────────────────────────────────────────────────
	// 準備開始：指定インデックスのスキルを「準備状態」にする
	// ・クールタイム中なら無視
	// ・Self タイプ以外はプレビューUIを表示・スケール調整
	// ─────────────────────────────────────────────────
	void OnPreparation(int skillIndex)
	{
		// 不正インデックス防止
		if (skillIndex >= skills.Count) return;

		ISkill skill = skills[skillIndex];

		// クールタイム中は準備不可
		if (cooldownTimers.TryGetValue(skill, out float cd) && cd > 0f) return;

		// 準備状態に移行
		currentSkillIndex = skillIndex;
		m_approvalSkill = true;

		// Self タイプはUIプレビュー不要のことが多いので終了
		if (skill.SkillType == SkillType.Self) return;

		// プレビューUIを表示（存在する場合）
		if (skill.SkillUI != null)
		{
			// 射程などに応じて X/Z のスケールを調整（Y は薄板として 0.01 固定）
			skill.SkillUI.transform.localScale = new Vector3(skill.SkillRangeX, 0.01f, skill.SkillRangeZ);
			skill.SkillUI.SetActive(true);
		}
	}

	// ─────────────────────────────────────────────────
	// 発動試行：スキルボタンを離した（canceled）タイミング
	// ・準備中 && クール0 なら発動
	// ・発動成功時：クール開始、アニメ再生、UIを非表示にする
	// ─────────────────────────────────────────────────
	void OnReleasedSkill(InputAction.CallbackContext context)
	{
		// 不正インデックス防止
		if (currentSkillIndex < 0 || currentSkillIndex >= skills.Count) return;

		ISkill skill = skills[currentSkillIndex];

		bool fired = false;
		// 準備中で、かつクールタイムが 0 以下なら発動
		if (m_approvalSkill && cooldownTimers[skill] <= 0f)
		{
			ReleasedSkill();                          // 実際の発動処理（SkillTypeごとの引数切替）
			cooldownTimers[skill] = skill.CoolDownTime; // 個別クールタイム開始
			fired = true;

			// 発動直後、UIゲージを「満タン(1.0)寄り」にしてクール開始を視覚化
			if (skillToPrint.TryGetValue(skill, out var sp))
			{
				float denom = Mathf.Max(0.0001f, skill.CoolDownTime); // 0除算回避
				sp.UpdateClock(cooldownTimers[skill] / denom);
			}
		}

		// 発動できた場合のみアニメを再生（任意仕様）
		if (fired && m_animator != null)
		{
			m_animator.SetTrigger("Use");
		}

		// 準備解除
		m_approvalSkill = false;

		// プレビューUIを隠し、位置をプレイヤー足元へリセット
		if (skill.SkillUI != null)
		{
			skill.SkillUI.SetActive(false);
			skill.SkillUI.transform.position = new Vector3(
				transform.position.x,
				skill.SkillUI.transform.position.y,
				transform.position.z);
		}
	}

	// ─────────────────────────────────────────────────
	// 準備キャンセル（SkillCancel 入力）
	// ・準備状態を解除し、プレビューUIを隠す＆位置リセット
	// ─────────────────────────────────────────────────
	void OnSkillCancel(InputAction.CallbackContext context)
	{
		if (currentSkillIndex < 0 || currentSkillIndex >= skills.Count) return;

		ISkill skill = skills[currentSkillIndex];
		m_approvalSkill = false;

		if (skill.SkillUI != null)
		{
			skill.SkillUI.SetActive(false);
			skill.SkillUI.transform.position = new Vector3(
				transform.position.x,
				skill.SkillUI.transform.position.y,
				transform.position.z);
		}
	}

	// ─────────────────────────────────────────────────
	// スキル発動本体：SkillType に応じて Execute の引数を切り替える
	// Point      : UI の位置（ターゲット地点）
	// Direction  : 自身の位置 ＋ UI の回転（向き）
	// Target     : TODO（ターゲット選択システムと連携）
	// Self       : 自身を対象に発動（位置＋GameObject 参照）
	// ─────────────────────────────────────────────────
	void ReleasedSkill()
	{
		ISkill skill = skills[currentSkillIndex];
		switch (skill.SkillType)
		{
			case SkillType.Point:
				skill.Execute(skill.SkillUI.transform.position);
				break;

			case SkillType.Direction:
				skill.Execute(transform.position, skill.SkillUI.transform.rotation);
				break;

			case SkillType.Target:
				// TODO: ターゲット決定ロジックをここで呼ぶ（例：ロックオン対象、レイキャスト結果など）
				break;

			case SkillType.Self:
				skill.Execute(transform.position, default, gameObject);
				Debug.Log("セルフスキル");
				break;
		}
	}

	// ─────────────────────────────────────────────────
	// 毎Fixedフレーム処理：
	// ・各スキルのクールタイムを減算＆0でクランプ
	// ・SkillPrint（UI）へ比率（残りCT/総CT）を渡して表示更新
	// ・準備中であれば、入力方向に応じてプレビューUIの位置/向きを更新
	// ─────────────────────────────────────────────────
	private void FixedUpdate()
	{
		// 1) クールタイム更新＆UI反映
		var keys = new List<ISkill>(cooldownTimers.Keys);
		foreach (var skill in keys)
		{
			// 残りCTを減らす（0以下にはしない）
			if (cooldownTimers[skill] > 0f)
			{
				cooldownTimers[skill] -= Time.fixedDeltaTime;
				if (cooldownTimers[skill] < 0f) cooldownTimers[skill] = 0f;
			}

			// 対応する UI がある場合に、0〜1 の比率で更新
			if (skillToPrint.TryGetValue(skill, out var sp) && sp != null)
			{
				// ゼロ秒CTなら常に 0（=使用可）にする方が直感的
				float ct = skill.CoolDownTime;
				float ratio = (ct <= 0f) ? 0f : cooldownTimers[skill] / ct;
				sp.UpdateClock(ratio);
			}
		}

		// 2) 準備中でなければ、以降のプレビュー更新は不要
		if (!m_approvalSkill) return;

		// 3) 入力（スキル方向スティックやマウス等からの2Dベクトル）を取得
		Vector2 dir = m_playerInput.actions["SkillDirection"].ReadValue<Vector2>();
		float strength = dir.magnitude;

		// 現在準備中のスキルに応じて、プレビューUIの見せ方を変える
		ISkill currentSkill = skills[currentSkillIndex];
		switch (currentSkill.SkillType)
		{
			case SkillType.Direction:
				// 一定以上入力が入っているとき、向きを更新（Y=0 平面上の向き）
				if (strength > 0.2f && currentSkill.SkillUI != null)
				{
					Vector3 look = new Vector3(dir.x, 0f, dir.y);
					currentSkill.SkillUI.transform.rotation = Quaternion.LookRotation(look);
				}
				break;

			case SkillType.Point:
				// 一定以上入力が入っているとき、プレイヤー位置 + 入力方向 * 距離 でUI位置を更新
				if (strength > 0.1f && currentSkill.SkillUI != null)
				{
					currentSkill.SkillUI.transform.position = new Vector3(
						dir.x * currentSkill.SkillDistance + transform.position.x,
						currentSkill.SkillUI.transform.position.y, // 高さは既存のYを維持
						dir.y * currentSkill.SkillDistance + transform.position.z
					);
				}
				break;
		}
	}
}
