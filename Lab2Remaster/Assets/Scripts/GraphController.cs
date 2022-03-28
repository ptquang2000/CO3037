using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphController : MonoBehaviour
{
    public int rangeY;
    public int rangeX;
    public float interval;
    public Image container;
    public int pointTextSize;
    public int pointTextOffset;
    public int unitOffset;
    public int pointOffset;
    public int pointWidth;
    public int fontSize;
    public Color32 unitColor;

    public Color32 pointColor1;
    public Color32 pointColor2;
    public LineRenderer lr1;
    public LineRenderer lr2;
    
    public List<float> waitQueue1 = new List<float>();
    public List<float> waitQueue2 = new List<float>();
    private List<GameObject> points1 = new List<GameObject>();
    private List<GameObject> points2 = new List<GameObject>();
    private List<GameObject> timeLine = new List<GameObject>();
    private int x1 = 0;
    private int x2 = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        SetupGraph();
        // GenerateData();
    }

    void SetupGraph()
    {
        float width = container.rectTransform.sizeDelta.x / rangeX;
        for (int i = 0; i < rangeX; i++)
        {
            // Point
            GameObject point = new GameObject("point" + i.ToString());
            var img = point.AddComponent<Image>();
            img.rectTransform.sizeDelta = new Vector2(pointWidth, pointWidth);
            var tmpColor = pointColor1;
            tmpColor.a = 0;
            img.color = tmpColor;

            // Point value
            GameObject unit = new GameObject("value" + i.ToString());
            var text = unit.AddComponent<Text>();
            text.fontSize = pointTextSize;
            text.font =  Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.color = unitColor;
            text.alignment = TextAnchor.MiddleCenter;

            unit.transform.SetParent(point.transform);
            unit.transform.localPosition = new Vector3(0, pointTextOffset, 0);
            unit.transform.localScale = new Vector3(1f, 1f, 1f);

            point.transform.SetParent(container.transform);
            point.transform.localPosition = new Vector3(width * i + pointOffset, 0, 0);
            // point.transform.Rotate(0.0f, 0.0f, 45.0f, Space.Self);
            point.transform.localScale = new Vector3(1f, 1f, 1f);

            RectTransform rt = (RectTransform) point.transform;
            rt.anchorMax = new Vector2(0, 0);
            rt.anchorMin = new Vector2(0, 0);

            points1.Add(point);

            point = Instantiate(point);
            tmpColor = pointColor2;
            tmpColor.a = 0;
            point.GetComponent<Image>().color = tmpColor;

            point.transform.SetParent(container.transform);
            point.transform.localPosition = new Vector3(width * i + pointOffset - container.rectTransform.sizeDelta.x / 2, 0, 0);
            // point.transform.Rotate(0.0f, 0.0f, 45.0f, Space.Self);
            point.transform.localScale = new Vector3(1f, 1f, 1f);

            points2.Add(point);

            // X axis
            unit = new GameObject("unit" + i.ToString());
            text = unit.AddComponent<Text>();
            text.text = (i * interval).ToString();
            text.fontSize = fontSize;
            text.font =  Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.color = unitColor;
            text.alignment = TextAnchor.MiddleCenter;

            unit.transform.SetParent(container.transform);
            unit.transform.localPosition = new Vector3(width * i + pointOffset, unitOffset, 0);
            unit.transform.localScale = new Vector3(1f, 1f, 1f);

            rt = (RectTransform) unit.transform;
            rt.anchorMax = new Vector2(0, 0);
            rt.anchorMin = new Vector2(0, 0);

            timeLine.Add(unit);
        }
        lr1.positionCount = 0;
        lr2.positionCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (waitQueue1.Count != 0)
        {
            if (lr1.positionCount < points1.Count) lr1.positionCount += waitQueue1.Count;
            x1 = UpdateGraph(points1, waitQueue1, x1);

            for (int i = 0; i < lr1.positionCount; i++)
            {
                lr1.SetPosition(i, points1[i].transform.position);
            }
        }
        if (waitQueue2.Count != 0)
        {
            if (lr2.positionCount < points2.Count) lr2.positionCount += waitQueue2.Count;
            x2 = UpdateGraph(points2, waitQueue2, x2);

            for (int i = 0; i < lr2.positionCount; i++)
            {
                lr2.SetPosition(i, points2[i].transform.position);
            }
        }
    }

    int UpdateGraph(List<GameObject> points, List<float> waitQueue, int x)
    {
        int count = waitQueue.Count;
        for (int j = 0; j < count; j++)
        {
            float value =  waitQueue[0];
            waitQueue.RemoveAt(0);
            
            float yIndex = value / rangeY * container.rectTransform.sizeDelta.y - container.rectTransform.sizeDelta.y / 2;

            if (x == rangeX)
            {
                for (int i = 0; i < rangeX - 1; i++)
                {    
                    Vector3 nextPoint = points[i + 1].transform.localPosition;
                    points[i].transform.localPosition = new Vector3(points[i].transform.localPosition.x, nextPoint.y, 0);
                    timeLine[i].GetComponent<Text>().text = timeLine[i + 1].GetComponent<Text>().text; 
                }
                timeLine[rangeX - 1].GetComponent<Text>().text = (float.Parse(timeLine[rangeX - 2].GetComponent<Text>().text) + interval).ToString(); 
                x--; 
            }

            var pointTf = points[x].transform;
            // points[x].GetComponentInChildren<Text>().text = value.ToString();
            pointTf.localPosition = new Vector3(points[x].transform.localPosition.x, yIndex, 0);

            if (pointTf.GetComponent<Image>().color.a == 0)
            {
                Color32 tmpColor = pointTf.GetComponent<Image>().color;
                tmpColor.a = 255;
                pointTf.GetComponent<Image>().color = tmpColor;
            }
            x++;
        }
        return x;
    }

    public void UpdateData(List<float> waitQueue, float data)
    {
        waitQueue.Add(data);
    }

    IEnumerator _GenerateData()
    {
        while (true)
        {
            UpdateData(waitQueue1, Random.Range(0f, (float) rangeY));
            yield return new WaitForSeconds(1f);
            UpdateData(waitQueue2, Random.Range(0f, (float) rangeY));
        }
    }

    private void GenerateData()
    {
        StartCoroutine(_GenerateData());
    }

}
