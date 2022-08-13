using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;
public class TambolaPlayer : MonoBehaviourPun
{
    public List<string> AllActivePlayer = new List<string>();
    public List<int> PlayerNumbers = new List<int>();
    public Dictionary<int, bool> NumberStatus = new Dictionary<int, bool>();
    public Action<bool> OnPlayerBoardComplete;
    public Action<bool> OnPlayerClaim;
 

}
