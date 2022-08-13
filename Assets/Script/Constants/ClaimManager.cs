using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClaimManager : MonoBehaviour
{
    public static ClaimManager instance;
    public ClaimsHolder holder;

    [SerializeField] GameObject claimPrefab;
    public GameObject claimDescription;
    public GameObject ticket;
    List<string> buttons = new List<string>();
    Transform holderObj;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

    }


    private void Start()
    {
        string jsonData = Resources.Load<TextAsset>("JSON/claimsJSON").text;
        holder = JsonUtility.FromJson<ClaimsHolder>(jsonData);
    }

    public void GenerateClaims(Transform holderObject)
    {
        holderObj = holderObject;
        for(int i = 0; i < holder.claim_holder.Count; i++)
        {
            GameObject gb = Instantiate(claimPrefab, holderObject);
            gb.name = holder.claim_holder[i].claim_name;
            buttons.Add(gb.name);
            gb.GetComponent<Button>().onClick.AddListener(() => OnClaimButtonClick());
            gb.transform.GetChild(0).GetComponent<Text>().text = holder.claim_holder[i].claim_name;
            
        }
    }

    void OnClaimButtonClick()
    {
        foreach (Transform child in ticket.transform)
            child.GetComponent<Image>().enabled = false;
        
        GameObject gb = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        int index = buttons.IndexOf(gb.name);
        Debug.Log(index);
        claimDescription.SetActive(true);
        //Debug.Log(holder.claim_holder[index].description);
        claimDescription.transform.GetChild(0).Find("des").GetComponent<Text>().text = "Description : " + holder.claim_holder[index].description;
        claimDescription.transform.GetChild(0).Find("Title").GetComponent<Text>().text = "Claim : " + holder.claim_holder[index].claim_name;
        foreach(string s in holder.claim_holder[index].claimList)
        {
            ticket.transform.Find(s).GetComponent<Image>().enabled = true;
            //ticket.transform.Find(s).GetComponent<Image>().colo = true;
        }
    }

    public void Search(InputField searchIF)
    {

        for(int i = 0; i < holder.claim_holder.Count; i++)
        {
            if(holder.claim_holder[i].claim_name.ToLower().Contains(searchIF.text.ToLower()))
            {
                holderObj.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                holderObj.GetChild(i).gameObject.SetActive(false);
            }

        }
    }
}
