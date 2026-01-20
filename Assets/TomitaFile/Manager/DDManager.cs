using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDManager : MonoBehaviour
{
    [SerializeField] GameObject[] manager;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < manager.Length; i++)
        {
            DontDestroyOnLoad(manager[i]);
        }
    }
}
