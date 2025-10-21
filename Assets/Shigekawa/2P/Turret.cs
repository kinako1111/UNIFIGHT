using System.Collections;
using System.Collections.Generic;
using System.Linq; // LINQを使用するため

using UnityEngine;

public class Turret : MonoBehaviour
{
	// 攻撃範囲の半径
	[Header("攻撃範囲"), SerializeField]
	float m_autoAttackRange = 5f;

	// 攻撃範囲を視覚的に示すためのオブジェクト
	[Header("攻撃範囲(見た目)"), SerializeField]
	GameObject m_rangeLooks;

	// 一度の攻撃でダメージ判定を発生させる回数 (アニメーターがないため、この値は直接的な影響は少ないが、将来的な拡張のために残す)
	[Header("攻撃の発生回数  ※デフォは1"), SerializeField]
	int m_autoAttackCount = 1;

	// 一度に攻撃できる敵の数
	[Header("一度に攻撃できる数"), SerializeField]
	int m_simultaneous = 1;

	// 攻撃と攻撃の間の待ち時間（秒）
	[Header("攻撃速度"), SerializeField]
	float m_autoAttackInterval = 0.75f;

	// 遠距離攻撃で使用する弾のプレハブ
	[Header("弾のPrefab"), SerializeField]
	GameObject m_bulletPrefab;

	// 弾を生成する位置のTransform
	[Header("弾の生成地点"), SerializeField]
	Transform m_generateTransform;

	[Header("プレイヤーのTransform"), SerializeField] // ヘッダーを追加し、インスペクターで設定しやすいように
	Transform m_player;

	[Header("回転速度"), SerializeField] // Slerpの補間値を設定できるように追加。大きめの値が推奨 (例: 5f-15f)
	float m_rotationSpeed = 10f; // 初期値を10fに変更

	// ----- 内部で使用する変数群 -----
	// 攻撃範囲内にいる全てのユニットのリスト
	List<GameObject> m_unitsInRange = new();

	// 実際に攻撃対象となる敵のリスト
	List<GameObject> m_currentAttackTargets = new();

	Status m_status; // タレットのステータスコンポーネント

	bool m_isAttacking; // 現在攻撃中かどうかを示すフラグ
	private Coroutine m_attackCoroutine; // 攻撃ループを管理するコルーチン


	// 現在攻撃中であるか外部から参照するためのプロパティ
	public bool IsAttacking => m_isAttacking;

	private void Awake()
	{
		m_status = GetComponent<Status>();
	}

	private void Start()
	{
		m_isAttacking = false;
		// 攻撃範囲の見た目のサイズを、設定された攻撃範囲に合わせて調整
		if (m_rangeLooks != null)
		{
			m_rangeLooks.transform.localScale = new Vector3(
				m_autoAttackRange * 2, // 直径は半径の2倍
				m_rangeLooks.transform.localScale.y,
				m_autoAttackRange * 2
			);
			m_rangeLooks.SetActive(false); // 初期状態では攻撃範囲の見た目を非表示にする
		}
	}

	void Update()
	{
		// 毎フレーム、攻撃範囲内の敵を最新の状態に更新する
		DetectAndSelectTargets();

		// 攻撃対象がいない場合、または攻撃中でない場合はプレイヤーを向く（スムーズに）
		// 攻撃対象がいる場合は、PerformAttackLogic内でターゲットを向く処理が呼ばれる
		if (m_currentAttackTargets.Count == 0 && m_player != null)
		{
			Vector3 directionToPlayer = (m_player.position - transform.position).normalized;
			directionToPlayer.y = 0; // Y軸方向の回転は無視し、水平方向のみを考慮する

			if (directionToPlayer != Vector3.zero)
			{
				Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_rotationSpeed * Time.deltaTime); // Time.deltaTimeを追加
			}
		}


		// 攻撃対象の敵がいて、まだ攻撃コルーチンが開始されていない場合
		if (m_currentAttackTargets.Count > 0 && m_attackCoroutine == null)
		{
			// 攻撃ループを開始する
			m_attackCoroutine = StartCoroutine(AttackLoopCoroutine());
			m_isAttacking = true; // 攻撃中フラグを立てる
		}
		// 攻撃対象の敵がいなくなり、かつ攻撃コルーチンが実行中の場合
		else if (m_currentAttackTargets.Count == 0 && m_attackCoroutine != null)
		{
			// 攻撃コルーチンを停止する
			StopCoroutine(m_attackCoroutine);
			m_attackCoroutine = null; // コルーチン参照をクリア
			m_isAttacking = false; // 攻撃中フラグを下ろす
		}
	}

	// 攻撃範囲内の敵を検出し、ターゲットリストを更新する。
	private void DetectAndSelectTargets()
	{
		m_unitsInRange.Clear(); // 検出済みのユニットリストをクリア
		m_currentAttackTargets.Clear(); // 現在の攻撃ターゲットリストをクリア

		// タレットの位置を中心に、設定された攻撃範囲で球状の衝突判定を行う
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_autoAttackRange);

		// 検出された全てのコライダーの中から、条件に合うオブジェクトを絞り込む
		// 1. コライダーが属するGameObjectを取得
		// 2. 自身（タレット）のGameObjectを除外
		// 3. "Enemy"タグを持つGameObjectのみを対象とする
		List<GameObject> potentialTargets = hitColliders
			.Select(collider => collider.gameObject)
			.Where(gameObject => gameObject != this.gameObject && gameObject.CompareTag("Enemy"))
			.ToList(); // 一時的なリストに変換

		// 絞り込んだ敵の中から、タレットからの距離が近い順にソートする
		m_unitsInRange = potentialTargets
			.OrderBy(gameObject => (transform.position - gameObject.transform.position).sqrMagnitude)
			.ToList();

		// ソートされた敵リストから、同時に攻撃できる数だけを実際の攻撃ターゲットとして選定する
		for (int i = 0; i < m_simultaneous && i < m_unitsInRange.Count; i++)
		{
			m_currentAttackTargets.Add(m_unitsInRange[i]);
		}
	}

	// 攻撃を繰り返すコルーチン。
	IEnumerator AttackLoopCoroutine()
	{
		// 攻撃ターゲットがいる間はループを続ける
		while (m_currentAttackTargets.Count > 0)
		{
			// 実際の攻撃処理を実行
			PerformAttackLogic();

			// 設定された攻撃間隔だけ待機
			yield return new WaitForSeconds(m_autoAttackInterval);
		}
		// ターゲットがいなくなってコルーチンが終了したら、コルーチン参照をクリアし、攻撃中フラグを下ろす
		m_attackCoroutine = null;
		m_isAttacking = false;
	}

	// 実際の攻撃ロジックを実行する。
	private void PerformAttackLogic()
	{
		// 攻撃ターゲットがいなければ何もしない
		if (m_currentAttackTargets.Count == 0) return;

		// ターゲットの方向を向く処理
		RotateTowardsTarget();

		// 遠距離攻撃のロジックを実行
		ExecuteFarAttack();
	}

	// タレットをターゲットの方向に向ける処理
	// 攻撃対象がいる場合に、ターゲット方向へ滑らかに向きを変える
	private void RotateTowardsTarget()
	{
		// ターゲットがいなければ何もしない
		if (m_currentAttackTargets.Count == 0 || m_currentAttackTargets.First() == null) return;

		// 最初のターゲットの位置からタレットの位置を引いて、方向ベクトルを算出
		Vector3 targetDirection = (m_currentAttackTargets.First().transform.position - transform.position).normalized;
		targetDirection.y = 0; // Y軸方向の回転は無視し、水平方向のみを考慮する

		// 方向ベクトルがゼロでなければ、タレットをターゲットの方向に向ける
		if (targetDirection != Vector3.zero)
		{
			Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
			// モデルの向きの補正が必要な場合は、ここで追加（例：X軸が前方のモデルの場合）
			// targetRotation *= Quaternion.Euler(0, 90f, 0);

			// Slerpを使って滑らかに方向転換させる
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_rotationSpeed * Time.deltaTime);
		}
	}

	// 遠距離攻撃のロジックを実行する
	private void ExecuteFarAttack()
	{
		// 弾のプレハブが設定されていない場合は警告を出して処理を終了
		if (m_bulletPrefab == null)
		{
			Debug.LogWarning("弾のプレハブが設定されていません！");
			return;
		}
		// 弾の生成地点が設定されていない場合は警告を出して処理を終了
		if (m_generateTransform == null)
		{
			Debug.LogWarning("弾の生成地点が設定されていません！");
			return;
		}

		// 現在の攻撃ターゲットそれぞれに対して処理を行う
		foreach (GameObject target in m_currentAttackTargets.ToList())
		{
			// ターゲットがnullになっているか、アクティブでなくなっている場合はスキップ
			if (target == null || !target.activeSelf)
			{
				continue; // 次のターゲットへ
			}

			// 弾のプレハブから弾を生成
			GameObject bullet = Instantiate(m_bulletPrefab, m_generateTransform.position, Quaternion.identity);

			// TrackingBulletコンポーネントを取得し、ターゲットとステータスを設定
			TrackingBullet trackingBulletComponent = bullet.GetComponent<TrackingBullet>();
			if (trackingBulletComponent != null)
			{
				if (m_status != null)
				{
					trackingBulletComponent.SetStatus(target, m_status.GetAttackPower());
				}
				else
				{
					Debug.LogWarning("TurretのStatusコンポーネントが設定されていません！");
					Destroy(bullet); // Statusがないので弾は無効
				}
			}
			else
			{
				Debug.LogWarning("弾のプレハブにTrackingBulletコンポーネントがありません！");
				Destroy(bullet); // コンポーネントがないので弾は無効
			}
		}
	}

	// ★追加★ アニメーションイベントなどから呼び出すことを想定
	public void OnAttackStart()
	{
		// アニメーションスタートのタイミングで呼び出す
		// このメソッドが呼ばれる時点では、m_currentAttackTargetsに攻撃対象がいる想定
		if (m_currentAttackTargets.Count > 0 && m_currentAttackTargets.First() != null)
		{
			// ターゲット方向を向く
			Vector3 direction = (m_currentAttackTargets.First().transform.position - transform.position).normalized;
			direction.y = 0; // 水平方向のみを考慮

			if (direction != Vector3.zero)
			{
				Quaternion targetRotation = Quaternion.LookRotation(direction);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_rotationSpeed); // m_rotationSpeedを直接使用
			}
		}
	}
}