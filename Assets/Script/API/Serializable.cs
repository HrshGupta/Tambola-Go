using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultParamter
{
    public string game_id;
}

[System.Serializable]
public class ClaimData
{
    public string game_id;
    public string user_id;
    public string claim_name;
}


[System.Serializable]
public class ResultData
{
    public string game_id;
    public string username;
    public int user_id;
    public List<string> claim_name = new List<string>();
}


[System.Serializable]
public class FinalResultResponse
{
    public string status;
    public string message;
    public List<ResultData> items = new List<ResultData>();

}


[System.Serializable]
public class Wallet
{
    public int user_id;
    public int chip_amount;
    public string utype;
}

[System.Serializable]
public class WalletResponse
{
    public int status;
    public int message;
    //public 
}


[System.Serializable]
public class ClaimsDetail
{
    public string claim_name;
    public string description;
    public List<string> claimList = new List<string>();
}

[System.Serializable]
public class ClaimsHolder
{
    public List<ClaimsDetail> claim_holder = new List<ClaimsDetail>();
}