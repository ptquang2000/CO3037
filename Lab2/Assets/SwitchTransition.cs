using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTransition : MonoBehaviour
{
    public UnityEngine.UI.Toggle toggle;
    public UnityEngine.UI.Image buttonImage;

    private const int offButtonPosition = -100;
    private const int onButtonPosition = 94;
    public Color offColor = new Color(138,138,138);
    public Color onColor = new Color(255,255,255);

    // Start is called before the first frame update
    void Start()
    {
        this.offColor.a = 1;
        this.onColor.a = 1;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 targetPos;
        Color targetColor;
        if (this.toggle.isOn) 
        {
            targetPos = new Vector2(SwitchTransition.onButtonPosition, this.buttonImage.transform.localPosition[1]);
            targetColor = this.onColor;
        } else 
        {
            targetPos = new Vector2(SwitchTransition.offButtonPosition, this.buttonImage.transform.localPosition[1]);
            targetColor = this.offColor;
        }
        this.buttonImage.transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, 10f * Time.deltaTime);
        this.buttonImage.color = Color.Lerp(this.buttonImage.color, targetColor, 10f * Time.deltaTime);
    }
}
