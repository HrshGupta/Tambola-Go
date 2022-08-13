using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SavePlayer(List<string> sorterPlayer)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/player.fun";
        FileStream stream = new FileStream(path, FileMode.Create);
        
        formatter.Serialize(stream, sorterPlayer);
        stream.Close();
    }

    public static List<string> LoadPlayer()
    {
        string path = Application.persistentDataPath + "/player.fun";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            List<string> data = formatter.Deserialize(stream) as List<string>;

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;   
        }
    }

}
