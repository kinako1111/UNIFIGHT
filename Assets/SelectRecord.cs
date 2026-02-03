using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectRecord : MonoBehaviour
{
	[Header("プレイヤーの人数"),SerializeField] int m_selectPlayerCount = 1;

	[Header("選んだキャラの"),SerializeField] List<int> m_selectCharaID = new() ;

}
