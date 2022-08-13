using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using UnityEngine.Networking;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class EventCreateScript : MonoBehaviour
{
    // public RawImage[] testTicket;
    [SerializeField] InputField date;
    [SerializeField] InputField month;
    [SerializeField] InputField year;
    [SerializeField] InputField hours;
    [SerializeField] InputField minutes;
    [SerializeField] InputField seconds;
    [SerializeField] InputField ticketCost;
    [SerializeField] InputField totalTickets;
    [SerializeField] Transform ticketImage;
    [SerializeField] Transform ticketContent;
    [SerializeField] Transform ticketDots;
    [SerializeField] Transform ticketDotsContent;
    [SerializeField] GameObject ticketPanel;
    [SerializeField] LoginScript loginScript;
    string am_Pm = "AM";
    [SerializeField] GameObject[] onOff;

    [SerializeField] Transform qrPrefab;
    [SerializeField] Transform qrPrefabHolder;
    List<RawImage> qrRawImages = new List<RawImage>();
    List<string> urls = new List<string>();
    Dictionary<Texture2D, string> qrHolder = new Dictionary<Texture2D, string>();
    public Button generateQRBtn;


    public void OnClickCreate()
    {
        StartCoroutine(TicketDownload());
        for (int i = 0; i < ticketContent.childCount; i++)
        {
            Destroy(ticketContent.GetChild(i).gameObject);
        }
        for (int i = 0; i < qrPrefabHolder.childCount; i++)
        {
            Destroy(qrPrefabHolder.GetChild(i).gameObject);
        }
        //generateQRBtn.onClick.AddListener(() => GenerateQR());
    }

    public void OnButtonPress()
    {
        if (am_Pm == "AM")
        {
            am_Pm = "PM";
            onOff[0].SetActive(false);
            onOff[1].SetActive(true);
        }
        else
        {

            am_Pm = "AM";
            onOff[0].SetActive(true);
            onOff[1].SetActive(false);
        }
    }



    IEnumerator TicketDownload()
    {
        var url = StaticDetatils.baseUrl + "Api/Tickets/create.php";

        string data = JsonUtility.ToJson(Data());
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
            CreatedEvent info = JsonUtility.FromJson<CreatedEvent>(json);

            Debug.Log(json);
            Debug.Log(info.status);
            Debug.Log(info.message);
            Debug.Log(info.events.tickets.Count);

            if(info.status == 200)
            {
                if (info.events.tickets != null)
                {
                    foreach (Ticket item in info.events.tickets)
                    {
                        Debug.Log(item.ticket_id + " " + item.ticket_name + " " + item.ticket_image);
                    }
                }

                for (int i = 0; i < info.events.tickets.Count; i++)
                {
                    StartCoroutine(DownloadTickets(info.events.tickets[i].ticket_image));
                }

                ticketPanel.SetActive(true);
                loginScript.CheckEvents();
                generateQRBtn.onClick.AddListener(GenerateQR);
            }
            else
            {
                AndroidToastMsg.ShowAndroidToastMessage(info.message);
            }
           
        }
    }

    IEnumerator DownloadTickets(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        
            
        Texture texture = DownloadHandlerTexture.GetContent(www);
        Transform img = Instantiate(ticketImage, ticketContent);
        img.GetComponent<RawImage>().texture = texture;
        // img.GetComponent<Button>().onClick
        img.GetComponent<Button>().onClick.AddListener(() => OnClickShareLink(url));
        urls.Add(url);
        qrHolder.Add(new Texture2D(256, 256), url);
        //Transform qr = Instantiate(qrPrefab, qrPrefabHolder);
        //qrRawImages.Add(qr.GetComponent<RawImage>());

        Instantiate(ticketDots,ticketDotsContent);
    }

    public void OnClickShareLink(string url)
    {
        Application.OpenURL("https://wa.me/?text=" + url);
    }

    public void OnClickTicketPanelBack()
    {
        ticketPanel.SetActive(false);
        gameObject.SetActive(false);
    }

    CreateData Data()
    {
        CreateData createData = new CreateData();
        createData.event_date = date.text.ToString() + "-" + month.text.ToString() + "-" + year.text.ToString();
        createData.event_time = hours.text.ToString() + ":" + minutes.text.ToString() + ":" + seconds.text.ToString() + am_Pm;
        createData.user_id = PlayerPrefs.GetString("UserID");
        createData.category_id = PlayerPrefs.GetString("Category");
        createData.sub_category_id = PlayerPrefs.GetString("SubCategory");
        createData.cost_of_ticket = ticketCost.text.ToString();
        createData.number_of_ticket = totalTickets.text.ToString();

        return createData;
    }

    #region QRCode Generator

    public void GenerateQR()
    {
        //int i = 0;
        for(int i = 0; i < qrPrefabHolder.childCount; i++)
        {
            Destroy(qrPrefabHolder.GetChild(i).gameObject);
        }

        foreach(KeyValuePair<Texture2D , string> k in qrHolder)
        {
            Transform qr = Instantiate(qrPrefab, qrPrefabHolder);
            Encode(k.Value , k.Key , qr.GetComponent<RawImage>());
            //i++;
        }


    }

    Color32[] QREncoder(string textToEncode , int width , int height)
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

    void Encode(string textToEncode ,Texture2D qrTexture , RawImage qrUI)
    {
        Color32[] qr = QREncoder(textToEncode , qrTexture.height , qrTexture.width);
        qrTexture.SetPixels32(qr);
        qrTexture.Apply();
        qrUI.texture = qrTexture;
        
    }

    #endregion

}
