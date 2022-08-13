using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class EventJoinScript : MonoBehaviour
{
    [SerializeField] Transform ticketImage;
    [SerializeField] Transform ticketContent;
    [SerializeField] Transform ticketDots;
    [SerializeField] Transform ticketDotsContent;
    [SerializeField] GameObject ticketPanel;
    [SerializeField] Text dateTime;
    public static EventJoinScript instance;


    [SerializeField] Transform qrPrefab;
    [SerializeField] Transform qrPrefabHolder;
    List<RawImage> qrRawImages = new List<RawImage>();
    List<string> urls = new List<string>();
    Dictionary<Texture2D, string> qrHolder = new Dictionary<Texture2D, string>();
    [SerializeField] Button generateQRBtn;
    [SerializeField] Button showTicketBtn;

    void Start()
    {
        instance = this;
        
        
    }

    public void OnClickEvent(string eventID)
    {
        if(ticketContent.childCount != 0)
        {
            for(int i = 0; i < ticketContent.childCount; i++)
            {
                Destroy(ticketContent.GetChild(i).gameObject);
                Destroy(ticketDotsContent.GetChild(i).gameObject);
            }
        }
        if(qrPrefabHolder.childCount != 0)
        {
            for(int i = 0; i < qrPrefabHolder.childCount; i++)
            {
                Destroy(qrPrefabHolder.GetChild(i).gameObject);
            }
        }

        StartCoroutine(CheckEvents(eventID));
        generateQRBtn.onClick.AddListener(GenerateQR);
    }

    IEnumerator CheckEvents(string eventID)
    {

        qrHolder.Clear();
        urls.Clear();
        var url = StaticDetatils.baseUrl + "Api/Users/login.php";

        LogIn logIn = new LogIn();

        logIn.email = PlayerPrefs.GetString("UserEmail");
        logIn.password = PlayerPrefs.GetString("UserPassword");

        string data = JsonUtility.ToJson(logIn);
        Debug.Log(data);


        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
 
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            LoggedIn info = JsonUtility.FromJson<LoggedIn>(json);
            foreach(Event item in info.events)
            {
                if(item.event_id == eventID)
                {
                    dateTime.text = item.event_date.ToString() + "\n" + item.event_time.ToString();
                    for(int i = 0; i < item.tickets.Count; i++)
                    { 
                        StartCoroutine(DownloadTickets(item.tickets[i].ticket_image));
                    }
                }
            }

            ticketPanel.SetActive(true);
        }
    }

    IEnumerator DownloadTickets(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        for (int i = 0; i < ticketContent.childCount; i++)
            Destroy(ticketContent.GetChild(0));

        Texture texture = DownloadHandlerTexture.GetContent(www);
        Transform img = Instantiate(ticketImage, ticketContent);
        img.GetComponent<RawImage>().texture = texture;
        // img.GetComponent<Button>().onClick
        img.GetComponent<Button>().onClick.AddListener(() => OnClickShareLink(url));
        Instantiate(ticketDots,ticketDotsContent);
        qrHolder.Add(new Texture2D(256, 256), url);
        //qrRawImages.Add(qr.GetComponent<RawImage>());
    }

    public void OnClickShareLink(string url)
    {
        Application.OpenURL("https://wa.me/?text=" + url);
    }


    #region QRCode Generator

    public void GenerateQR()
    {
        Debug.Log("### Generate");
        
        //int i = 0;
        foreach (KeyValuePair<Texture2D, string> k in qrHolder)
        {
            Transform qr = Instantiate(qrPrefab, qrPrefabHolder);
            qr.GetComponent<Button>().onClick.AddListener(() => ShareQRImage(qr));
            Encode(k.Value, k.Key, qr.GetComponent<RawImage>());
            //i++;
        }

        
        //generateQRBtn.onClick.AddListener(() => );

    }

    Color32[] QREncoder(string textToEncode, int width, int height)
    {
        BarcodeWriter barCodeWriter = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            },
        };


        return barCodeWriter.Write(textToEncode);
    }

    void Encode(string textToEncode, Texture2D qrTexture, RawImage qrUI)
    {
        Color32[] qr = QREncoder(textToEncode, qrTexture.height, qrTexture.width);
        qrTexture.SetPixels32(qr);
        qrTexture.Apply();
        qrUI.texture = qrTexture;
        generateQRBtn.onClick.RemoveAllListeners();
    }

    #endregion


    void ShareQRImage(Transform t)
    {
        Texture2D qrImage = (Texture2D)t.GetComponent<RawImage>().texture;
        //byte[] data = qrImage.EncodeToPNG();
        
        new NativeShare().AddFile(qrImage, "").Share();
    }
   
}
