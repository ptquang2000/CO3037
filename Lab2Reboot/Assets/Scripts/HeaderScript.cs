using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeaderScript : MonoBehaviour
{
    public Text dataTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        dataTime.text = System.DateTime.Now.ToString("dd/MM/yyyy   HH:mm:ss ");
    }
}
