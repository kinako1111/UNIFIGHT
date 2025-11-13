using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDelete : MonoBehaviour
{
    [SerializeField] float aliveTime;
    private float stayTime;
    // Start is called before the first frame update
    void Start()
    {
        stayTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        stayTime += Time.deltaTime;
        if(aliveTime<=stayTime)
        {
            Debug.Log("íœ");
            Destroy(this.gameObject);
        }
    }
}
