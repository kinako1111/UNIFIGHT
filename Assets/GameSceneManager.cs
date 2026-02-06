using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameSceneManager : MonoBehaviour
{
    SelectRecord m_selectRecord;

    [Header("生成用プレハブ設定")]
    [SerializeField] GameObject[] m_playerPrefab;
    [SerializeField] Transform[] m_genelatePos;

    [Header("カメラ設定")]
    [SerializeField] Camera cameraPrefab; // CameraFollowがアタッチされたカメラ

    [Header("入力設定")]
    [SerializeField] InputActionAsset m_actionAsset;

    void Start()
    {
        // 1. 選択データの取得
        GameObject controllerObj = GameObject.FindWithTag("GameController");
        if (controllerObj == null) return;

        m_selectRecord = controllerObj.GetComponent<SelectRecord>();
        var selectionDict = m_selectRecord.GetDictionary();

        int index = 0;
        int totalPlayers = selectionDict.Count;

        foreach (var entry in selectionDict)
        {
            InputDevice[] devices = entry.Key;
            int charId = entry.Value;

            if (index >= m_genelatePos.Length) break;

            // 2. カメラとプレイヤーの生成
            Camera playerCamera = Instantiate(cameraPrefab);
            PlayerInput newPlayerInput = PlayerInput.Instantiate(
                m_playerPrefab[charId],
                pairWithDevices: devices
            );

            // 3. 基本設定（追従・PlayerInputへの登録）
            newPlayerInput.camera = playerCamera;
            CameraFollow followScript = playerCamera.GetComponent<CameraFollow>();
            if (followScript != null)
            {
                followScript.SetTarget(newPlayerInput.transform);
            }

            // 4. 【重要】カメラのアタッチ（TakeDamageへの参照渡し）
            // これにより、ダメージUIが「自分のカメラ」を向くようになります
            TakeDamage td = newPlayerInput.GetComponent<TakeDamage>();
            if (td != null)
            {
                td.SetPlayerCamera(playerCamera);
            }
            else
            {
                Debug.LogError($"{newPlayerInput.name} に TakeDamage が見つかりません！");
            }

            // 5. UI Canvas の分割画面対応
            Canvas[] canvases = newPlayerInput.GetComponentsInChildren<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = playerCamera;
                canvas.planeDistance = 1;
            }

            // 6. 画面分割の適用
            SetupCameraRect(playerCamera, index, totalPlayers);

            // 7. 位置とその他の初期化
            newPlayerInput.transform.position = m_genelatePos[index].position;
            newPlayerInput.GetComponent<PlayerController>()?.SetCamera(playerCamera);
            newPlayerInput.GetComponent<SkillActivation>()?.Setup(newPlayerInput, playerCamera);

            index++;
        }

        // 8. 3人プレイ時のTower監視カメラ生成
        if (totalPlayers == 3)
        {
            CreateTowerCamera();
        }
    }

    /// <summary>
    /// 右下の空きスペースにTowerを映すカメラを作成
    /// </summary>
    void CreateTowerCamera()
    {
        GameObject tower = GameObject.FindWithTag("Target");
        if (tower == null) return;

        Camera towerCam = Instantiate(cameraPrefab);

        // 監視カメラなので追従や音声リスナーは不要
        if (towerCam.TryGetComponent<CameraFollow>(out var follow)) follow.enabled = false;
        if (towerCam.TryGetComponent<AudioListener>(out var listener)) listener.enabled = false;

        // UIが映り込まないように Culling Mask から UI レイヤーを除外
        towerCam.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));

        towerCam.transform.position = tower.transform.position + new Vector3(0, 10, -10);
        towerCam.transform.LookAt(tower.transform);
        towerCam.rect = new Rect(0.5f, 0f, 0.5f, 0.5f); // 右下
    }

    void SetupCameraRect(Camera cam, int index, int total)
    {
        if (total <= 1)
        {
            cam.rect = new Rect(0, 0, 1, 1);
            return;
        }

        if (total == 2)
        {
            cam.rect = new Rect(index * 0.5f, 0, 0.5f, 1.0f);
        }
        else
        {
            float x = (index % 2) * 0.5f;
            float y = (index < 2) ? 0.5f : 0f;
            cam.rect = new Rect(x, y, 0.5f, 0.5f);
        }
    }
}