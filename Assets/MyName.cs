using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MyName : MonoBehaviour
{
    void Start()
    {
        GetComponent<TMP_Text>().text = transform.parent.name;
    }
}
