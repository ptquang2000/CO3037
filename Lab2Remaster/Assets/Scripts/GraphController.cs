using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphController : MonoBehaviour
{
    public int rangeY;
    public int rangeX;
    public int interval;
    public Image container;
    public int pointTextSize;
    public int pointTextOffset;
    public int unitOffset;
    public int borderOffset;
    public int pointWidth;
    public int fontSize;
    public Color32 unitColor;

    public List<Color32> pointColor;
    public List<LineRenderer> lines = new List<LineRenderer>();
    
    private List<GameObject> timeLine = new List<GameObject>();
    private List<float> timeBuffer = new List<float>();
    
    private List<List<float>> queuesList = new List<List<float>>();
    private List<List<GameObject>> pointsList = new List<List<GameObject>>();
    private float xMax = 0.0f;
    private float yMax = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        queuesList.Add(new List<float>());
        queuesList.Add(new List<float>());
        yMax = container.rectTransform.sizeDelta.y - 2 * borderOffset;
        xMax = container.rectTransform.sizeDelta.x - 2 * borderOffset;
        SetupGraph();
        // GenerateData();
    }

    void SetupGraph()
    {
        for (int j = 0; j < 2; j++)
        {
            float width = xMax / rangeX;
            List<GameObject> points = new List<GameObject>();
            for (int i = 0; i < rangeX; i++)
            {
                // Point
                GameObject point = new GameObject("point" + i.ToString());
                var img = point.AddComponent<Image>();
                img.rectTransform.sizeDelta = new Vector2(pointWidth, pointWidth);
                var tmpColor = pointColor[j];
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
                point.transform.localPosition = new Vector3(width * i, 0, 0);
                // point.transform.Rotate(0.0f, 0.0f, 45.0f, Space.Self);
                point.transform.localScale = new Vector3(1f, 1f, 1f);

                RectTransform rt = (RectTransform) point.transform;
                rt.anchorMax = new Vector2(0, 0);
                rt.anchorMin = new Vector2(0, 0);

                points.Add(point);

                if (j != 0) continue;

                // Timeline
                unit = new GameObject("unit" + i.ToString());
                text = unit.AddComponent<Text>();
                text.fontSize = fontSize;
                text.font =  Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                text.color = unitColor;
                text.alignment = TextAnchor.MiddleCenter;

                unit.transform.SetParent(container.transform);
                unit.transform.localPosition = new Vector3(width * i + borderOffset, -unitOffset, 0);
                unit.transform.localScale = new Vector3(1f, 1f, 1f);

                rt = (RectTransform) unit.transform;
                rt.anchorMax = new Vector2(0, 0);
                rt.anchorMin = new Vector2(0, 0);

                timeLine.Add(unit);
            
                timeBuffer.Add(0);
            }
            lines[j].positionCount = 0;
            pointsList.Add(points);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (queuesList[0].Count != 0 && queuesList[1].Count != 0)
        {
            UpdateGraph();
            for (int i = 0; i < lines[0].positionCount; i++)
            {
                lines[0].SetPosition(i, pointsList[0][i].transform.position);
                lines[1].SetPosition(i, pointsList[1][i].transform.position);
            }
        }
    }

    void UpdateGraph()
    {
        float curTime = Time.time;
        int x = 0;
        while (x != rangeX && timeBuffer[timeBuffer.Count - 1] == 0 && timeBuffer[x] != 0)
        {
            x++;
        }
        if (timeBuffer[timeBuffer.Count - 1] != 0)
        {
            int times = (int) (curTime - timeBuffer[timeBuffer.Count - 1]) / interval + 1;
            shiftGraph(times);
            x = rangeX - times;
        }
        lines[0].positionCount = x + 1;
        lines[1].positionCount = x + 1;
        
        timeBuffer[x] = curTime;
        timeLine[x].GetComponent<Text>().text = System.DateTime.Now.ToString("HH:mm:ss");

        for (int i = 0; i < lines.Count; i++)
        {
            List<float> waitQueue = queuesList[i];
            List<GameObject> points = pointsList[i];
            LineRenderer lr = lines[i];

            int count = waitQueue.Count;
            for (int j = 0; j < count; j++)
            {
                
                float value =  waitQueue[0];
                float yIndex = value / rangeY * yMax - yMax / 2;
                waitQueue.RemoveAt(0);

                float xIndex = ((float) x / rangeX * xMax) - xMax / 2;

                var pointTf = points[x].transform;
                pointTf.localPosition = new Vector3(xIndex, yIndex, 0);
                // points[x].GetComponentInChildren<Text>().text = value.ToString();

                if (pointTf.GetComponent<Image>().color.a == 0)
                {
                    Color32 tmpColor = pointTf.GetComponent<Image>().color;
                    tmpColor.a = 255;
                    pointTf.GetComponent<Image>().color = tmpColor;
                }
            }

        }
    }

    public void shiftGraph(int times = 1)
    {
        for (int i = 0; i < rangeX - times; i++)
        {   
            foreach (List<GameObject> points in pointsList)
            {
                Vector3 nextPoint = points[i + times].transform.localPosition;
                points[i].transform.localPosition = new Vector3(points[i].transform.localPosition.x, nextPoint.y, 0);
            }
            timeLine[i].GetComponent<Text>().text = timeLine[i + times].GetComponent<Text>().text;
            timeBuffer[i] = timeBuffer[i + times];
        }
        for (int i = rangeX - times; i < rangeX; i++)
        {
            foreach (List<GameObject> points in pointsList)
            {
                var pointTf = points[i].transform;
                Color32 tmpColor = pointTf.GetComponent<Image>().color;
                tmpColor.a = 0;
                pointTf.GetComponent<Image>().color = tmpColor;
            }
            timeLine[i].GetComponent<Text>().text = "";
            timeBuffer[i] = 0;
        }
    }

    public void UpdateData(float temperature, float humidity)
    {
        queuesList[0].Add(temperature);
        queuesList[1].Add(humidity);
    }

    IEnumerator _GenerateData()
    {
        while (true)
        {
            UpdateData(Random.Range(0f, (float) rangeY), Random.Range(0f, (float) rangeY));
            yield return new WaitForSeconds(1f);
        }
    }

    private void GenerateData()
    {
        StartCoroutine(_GenerateData());
    }

}
