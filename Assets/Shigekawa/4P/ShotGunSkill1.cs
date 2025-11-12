using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShotGunSkill1 : MonoBehaviour
{
	// スキルのタイプはPointに固定 (スモークは設置型のため)
	private enum Type { Point }

	[Header("スキルの範囲を示すUI (GameObject)"), SerializeField]
	GameObject m_skillAimPoint; // 照準として使うGameObject

	[Header("スキルの照準感度"), SerializeField]
	float m_skillAimSensitivity =0.5f;

	[Header("スキルの照準最大範囲"), SerializeField]
	float m_skillAimRange = 5.0f;

	// 設置するスモークのPrefab
	[Header("設置するスモークのPrefab"), SerializeField]
	GameObject m_smokePrefab;

	// スキルクールダウン時間 (秒)
	[Header("スキルのクールダウン時間 (秒)"), SerializeField]
	float m_skillCooldownTime = 10.0f; // 10秒に設定

	Vector2 m_aimDirectionInput; // スティックの入力方向
	float m_aimStrength;         // スティックの傾き度合い

	bool m_isSkillPreparing; // スキル準備中かどうか (照準表示中)

	PlayerInput m_playerInput;
	Animator m_animator; // 必要であれば

	private bool m_isCoolingDown = false; // クールダウン中かどうかのフラグ
	private float m_nextSkillReadyTime = 0f; // 次のスキルが使用可能になる時間

	private void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>();
		m_animator = GetComponent<Animator>(); // Animatorが必要なければコメントアウト
	}

	private void Start()
	{
		if (m_skillAimPoint != null)
		{
			m_skillAimPoint.SetActive(false); // 初期状態では照準を非表示
		}
		else
		{
			Debug.LogWarning("m_skillAimPoint が設定されていません。Inspectorで設定してください。");
		}
	}

	private void OnEnable()
	{
		m_playerInput.actions["SkillButton"].performed += OnSkillPreparationStarted;
		m_playerInput.actions["SkillButton"].canceled += OnSkillFired;
		m_playerInput.actions["SkillCancel"].performed += OnSkillCancel;
	}

	private void OnDisable()
	{
		m_playerInput.actions["SkillButton"].performed -= OnSkillPreparationStarted;
		m_playerInput.actions["SkillButton"].canceled -= OnSkillFired;
		m_playerInput.actions["SkillCancel"].performed -= OnSkillCancel;
	}

	// スキル準備開始 (スキルボタン押下時)
	void OnSkillPreparationStarted(InputAction.CallbackContext context)
	{
		// クールダウン中はスキル準備不可
		if (m_isCoolingDown)
		{
			Debug.Log("スモークスキルはクールダウン中です。残り: " + (m_nextSkillReadyTime - Time.time).ToString("F1") + "秒");
			return;
		}

		if (m_skillAimPoint != null)
		{
			m_skillAimPoint.SetActive(true); // 照準を表示
			m_isSkillPreparing = true;      // 準備中フラグを立てる

			// 照準の位置をプレイヤーの足元に初期化
			m_skillAimPoint.transform.position = new Vector3(
				transform.position.x,
				m_skillAimPoint.transform.position.y, // Y座標は元のGameObjectのYを維持
				transform.position.z);
		}
	}

	// スキル発動 (スキルボタンリリース時)
	void OnSkillFired(InputAction.CallbackContext context)
	{
		// スキル準備中でない、またはクールダウン中の場合は発動しない
		if (!m_isSkillPreparing || m_isCoolingDown)
		{
			// Debug.Log("スキルが発動されませんでした。(準備中ではないか、クールダウン中)");
			if (m_skillAimPoint != null) m_skillAimPoint.SetActive(false);
			m_isSkillPreparing = false;
			return;
		}

		// 照準を非表示
		if (m_skillAimPoint != null)
		{
			m_skillAimPoint.SetActive(false);
		}
		m_isSkillPreparing = false; // 準備中フラグを下ろす

		// スモークのPrefabが設定されているかチェック
		if (m_smokePrefab != null)
		{
			// 照準の位置にスモークを生成
			Vector3 spawnPosition = m_skillAimPoint.transform.position;
			GameObject newSmoke = Instantiate(m_smokePrefab, spawnPosition, Quaternion.identity);
			Debug.Log("スモークを生成しました: " + newSmoke.name + " at " + spawnPosition);

			// スキル発動後、クールダウンを開始
			StartCooldown();
		}
		else
		{
			Debug.LogWarning("スモークのPrefabが設定されていません。Inspectorで設定してください。");
		}
	}

	// スキルキャンセル
	void OnSkillCancel(InputAction.CallbackContext context)
	{
		if (m_skillAimPoint != null)
		{
			m_skillAimPoint.SetActive(false); // 照準を非表示
		}
		m_isSkillPreparing = false; // 準備中フラグを下ろす
		Debug.Log("スモークスキルをキャンセルしました。");
	}

	private void FixedUpdate()
	{
		// クールダウン中かどうかの状態を更新
		if (m_isCoolingDown)
		{
			if (Time.time >= m_nextSkillReadyTime)
			{
				m_isCoolingDown = false;
				Debug.Log("スモークスキルが使用可能になりました！");
			}
		}

		// スキル準備中でない、クールダウン中の場合、または照準用GameObjectがなければ処理をスキップ
		if (!m_isSkillPreparing || m_isCoolingDown || m_skillAimPoint == null) return;

		// スティックからの入力を読み取る
		m_aimDirectionInput = m_playerInput.actions["SkillDirection"].ReadValue<Vector2>();
		m_aimStrength = m_aimDirectionInput.magnitude; // スティックの傾き度合い (0.0～1.0)

		// スティックが少しでも傾いていたら照準を動かす
		if (m_aimStrength > 0.1f)
		{
			float effectiveStrength = m_aimStrength * m_skillAimSensitivity;
			// 距離はスティックの傾きと最大範囲に応じて決定
			float finalOffsetDistance = Mathf.Min(effectiveStrength, 1.0f) * m_skillAimRange;

			// 入力方向ベクトルを生成 (Y軸は常に0として地面に沿って移動)
			Vector3 directionVector = (m_aimDirectionInput.magnitude > 0) ?
									  new Vector3(m_aimDirectionInput.x, 0, m_aimDirectionInput.y).normalized :
									  Vector3.forward; // 入力がない場合のデフォルト方向 (前方)

			// プレイヤーからの相対位置を計算
			Vector3 desiredOffset = directionVector * finalOffsetDistance;
			m_skillAimPoint.transform.position = transform.position + desiredOffset;

			// 照準のY座標は元のGameObjectのY座標を維持 (地面に固定するため)
			Vector3 currentPos = m_skillAimPoint.transform.position;
			m_skillAimPoint.transform.position = new Vector3(currentPos.x, m_skillAimPoint.transform.position.y, currentPos.z);
		}
		else
		{
			// スティックが傾いていない場合はプレイヤーの足元に位置を戻す
			m_skillAimPoint.transform.position = new Vector3(
				transform.position.x,
				m_skillAimPoint.transform.position.y,
				transform.position.z);
		}
	}

	private void StartCooldown()
	{
		m_isCoolingDown = true;
		m_nextSkillReadyTime = Time.time + m_skillCooldownTime;
		Debug.Log("スモークスキルのクールダウン開始: " + m_skillCooldownTime + "秒");
	}
}