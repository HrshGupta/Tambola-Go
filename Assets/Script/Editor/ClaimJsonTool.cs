using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class ClaimJsonTool : Editor
{
    [MenuItem("Tambola/Tools/Claims JSON Creator")]
    public static void CreateJSON()
    {
        string savePath = Application.dataPath + "/Resources/JSON/" + "claimsJSON.json";

        if (!File.Exists(savePath))
        {
            Debug.Log(savePath);
            File.Create(savePath).Dispose();
        }

        string[] levelText = File.ReadAllLines(Application.dataPath + "/Resources/claims.txt");
        for (int i = 0; i < 5; i++)
            levelText = levelText.Where((val, idx) => idx != 0).ToArray();

        
        ClaimsHolder holder = new ClaimsHolder();
        for(int i = 0; i < levelText.Length; i++)
        {
            ClaimsDetail details = new ClaimsDetail();
            string[] subStrings = levelText[i].Split(';');
            details.claim_name = subStrings[0];
            details.description = subStrings[1];
            string[] claims = subStrings[2].Split('-');
            for(int j = 0; j < claims.Length; j++)
            {
                details.claimList.Add(claims[j]);
            }
            holder.claim_holder.Add(details);
        }

        string jsonString = JsonUtility.ToJson(holder, true);
        Debug.Log(jsonString);
        File.WriteAllText(savePath, jsonString);
    }
}
