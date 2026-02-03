using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSplitManager : MonoBehaviour
{
    public Camera[] m_cameras;

    public void Setup(List<Camera> cameras)
    {

        int count = cameras.Count;

        if (count == 0) return;

        // èâä˙âª
        foreach (var cam in cameras)
            cam.rect = new Rect(0, 0, 1, 1);

        switch(count)
        {
            case 1:
				m_cameras[0].rect = new Rect(0, 0, 1, 1);
                break;

            case 2:
			    m_cameras[0].rect = new Rect(0, 0.5f, 1, 0.5f);
				m_cameras[1].rect = new Rect(0, 0, 1, 0.5f);
                break;
            case 3:
				m_cameras[0].rect = new Rect(0, 0.5f, 0.5f, 0.5f);
				m_cameras[1].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
				m_cameras[2].rect = new Rect(0, 0, 1, 0.5f);
                break;
            case 4:
				m_cameras[0].rect = new Rect(0, 0.5f, 0.5f, 0.5f);
				m_cameras[1].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
				m_cameras[2].rect = new Rect(0, 0, 0.5f, 0.5f);
				m_cameras[3].rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                break;
        }
    }
}
