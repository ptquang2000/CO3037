using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchTransition : MonoBehaviour
{
    public Toggle toggle;
    public Image swImage;
    public int offButtonPosition;
    public int onButtonPosition;
    public Color offColor;
    public Color onColor;

    // Start is called before the first frame update
    void Start()
    {
        this.offColor.a = 1;
        this.onColor.a = 1;
        onValueChange();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void onValueChange()
    {
        Vector3 targetPos;
        Color targetColor;
        if (this.toggle.isOn == true)
        {
            targetPos = new Vector3(this.onButtonPosition, 0, 0);
            targetColor = this.onColor;
        } else 
        {
            targetPos = new Vector3(this.offButtonPosition, 0, 0);
            targetColor = this.offColor;
        }
        swImage.transform.localPosition = targetPos;
        swImage.color = targetColor;
    }
}
