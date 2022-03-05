using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThermostatRenderer : MonoBehaviour
{
    public int steps;
    public int offset;
    public float distance = 1;
    public int width;
    public int height;
    public int max = 100;
    public Image relative;
    private Image[] images;
    private Color offColor = new Color32(36,41,66,255);
    private Color onColor = new Color32(70,45,177,255);
    private int curPc;
    private int newPc;
    
    // Start is called before the first frame update
    void Start()
    {
        RenderCircle();
    }

    public void RenderCircle()
    {
        curPc = steps;
        images = new Image[steps+1];
        for (int i = steps / 2 + 2 * offset, j = 0; j <= steps; i++, j++){
            
            float theta = i * 2 * Mathf.PI / (steps + offset * 2);
            float x = Mathf.Sin(theta) * distance;
            float y = Mathf.Cos(theta) * distance;

            GameObject circle = new GameObject("pillshape" + i.ToString());
            var img = circle.AddComponent<Image>();
            img.sprite = Resources.Load<Sprite>("pill");
            img.rectTransform.sizeDelta = new Vector2(width, height);
            img.color = onColor;
            images[j] = img;
        
            circle.transform.SetParent(relative.transform);
            circle.transform.localPosition = new Vector3(x, y, 0);
            var rotation = Quaternion.LookRotation(Vector3.forward, circle.transform.position - relative.transform.position);
            circle.transform.rotation = rotation;
        }
    }

    IEnumerator _IEupdateThermostat(float value)
    {
        newPc =  (int)(value / max * (steps + 1));
        if (curPc < newPc)
        {
            for (int i = curPc; i < newPc; ++i)
            // for (int i = 0; i < (int) newPercentage; ++i)
            {
                images[i].color = onColor;
                yield return new WaitForSeconds(0.025f);
            }

        } else 
        {
            for (int i = curPc; i > newPc; --i)
            // for (int i = steps; i > (int) newPercentage; --i)
            {
                images[i].color = offColor;
                yield return new WaitForSeconds(0.025f);
            }
        }
        curPc = newPc;
    }

    public void updateThermostat(float value)
    {
        StartCoroutine(_IEupdateThermostat(value));
    }
}
