// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO;
// using System.Net;
// using UnityEngine.Networking;
// using UnityEngine.UI;

// public class APIScript : MonoBehaviour
// {
//     [SerializeField] RawImage img;
//     void Start()
//     {
//         FetchNFT();
//     }

//     private void FetchNFT()
//     {
//         var url = "http://ec2-3-109-71-196.ap-south-1.compute.amazonaws.com/tambola/Api/Category/read.php";
//         HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
//         HttpWebResponse response = (HttpWebResponse)request.GetResponse();
//         StreamReader reader = new StreamReader(response.GetResponseStream());
//         string json = reader.ReadToEnd();
//         // Debug.Log(json);
//         Data info = JsonUtility.FromJson<Data>(json);

//         Debug.Log(info.status);
//         foreach(Item item in info.items)
//         {
//             Debug.Log(item.category_id + " " + item.category_name);
//         }
//         // StartCoroutine(DownloadImage(info));
//     }

//     // IEnumerator DownloadImage(Data data)
//     // {
//     //     UnityWebRequest www = UnityWebRequest.Get(data);
//     //     yield return www.SendWebRequest();

//     //     Texture texture = DownloadHandlerTexture.GetContent(www);
//     //     img.texture = texture;
//     // }
// }
