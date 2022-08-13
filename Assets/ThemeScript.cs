using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ThemeScript : MonoBehaviour
{
    [SerializeField] Transform categoryContent;
    [SerializeField] Transform subCategoryContent;
    [SerializeField] Transform categoryButton;
    [SerializeField] GameObject categoryPanel;
    [SerializeField] GameObject subCategoryPanel;
    [SerializeField] GameObject createOrJoinPanel;
    [SerializeField] GameObject eventCreatePanel;
    [SerializeField] GameObject eventJoinPanel;

    public void OnClickThemeGamesButton()
    {
        if(categoryContent.transform.childCount != 0)
        {
            for(int i = 0; i < categoryContent.transform.childCount; i++)
            {
                Destroy(categoryContent.GetChild(i).gameObject);
            }
        }

        var url = StaticDetatils.baseUrl + "Api/Category/read.php";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string json = reader.ReadToEnd();
        CategoryData info = JsonUtility.FromJson<CategoryData>(json);

        foreach(Category item in info.items)
        {
            // Debug.Log(item.category_id + " " + item.category_name);
            Transform button = Instantiate(categoryButton, categoryContent);
            button.GetChild(0).GetComponent<Text>().text = item.category_name;
            button.GetComponent<Button>().onClick.AddListener(() => OnClickCategoryButton(item.category_id));
        }

        categoryPanel.SetActive(true);
    }

    public void OnClickCategoryButton(int id)
    {
        if(subCategoryContent.transform.childCount != 0)
        {
            for(int i = 0; i < subCategoryContent.transform.childCount; i++)
            {
                Destroy(subCategoryContent.GetChild(i).gameObject);
            }
        }

        var url = StaticDetatils.baseUrl + "Api/SubCategory/read.php?catId=" + id;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string json = reader.ReadToEnd();
        SubCategoryData info = JsonUtility.FromJson<SubCategoryData>(json);

        PlayerPrefs.SetString("Category", id.ToString());

        foreach(SubCategory item in info.items)
        {
            // Debug.Log(item.sub_category_id + " " + item.sub_category_name);
            Transform button = Instantiate(categoryButton, subCategoryContent);
            button.GetChild(0).GetComponent<Text>().text = item.sub_category_name;
            button.GetComponent<Button>().onClick.AddListener(() => OnClickSubCategoryButton(item.sub_category_id));
        }

        subCategoryPanel.SetActive(true);
    }

    public void OnClickSubCategoryButton(int id)
    {
        PlayerPrefs.SetString("SubCategory", id.ToString());
        createOrJoinPanel.SetActive(true);
    }

    public void OnClickCreateOrJoinBackButton()
    {
        createOrJoinPanel.SetActive(false);
    }

    public void OnClickEventCreateBackButton()
    {
        eventCreatePanel.SetActive(false);
    }

    public void OnClickEventJoinBackButton()
    {
        eventJoinPanel.SetActive(false);
    }

    public void OnClickCreateEvent()
    {
        eventCreatePanel.SetActive(true);
    }

    public void OnClickJoinEvent()
    {
        eventJoinPanel.SetActive(true);
    }
}
