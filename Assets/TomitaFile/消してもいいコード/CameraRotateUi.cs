using UnityEngine;

public class CameraRotateUi : MonoBehaviour
{
    private Camera m_targetCamera;

    public void SetTargetCamera(Camera cam)
    {
        m_targetCamera = cam;
    }

    void LateUpdate()
    {
        // カメラがセットされるまでは回転しない
        if (m_targetCamera == null) return;

        transform.rotation = m_targetCamera.transform.rotation;
    }
}