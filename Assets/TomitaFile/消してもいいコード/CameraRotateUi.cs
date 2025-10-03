using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotateUi : MonoBehaviour
{
    void LateUpdate()
    {
        // ƒJƒƒ‰‚Æ“¯‚¶Œü‚«‚Éİ’è
        transform.rotation = Camera.main.transform.rotation;    
    }
}
