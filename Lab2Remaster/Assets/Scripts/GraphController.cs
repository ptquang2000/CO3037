using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class Point {
    public GameObject point;
    public float y;
    public float x;
    public Text xText;
    public Image pointImg;
    public Text yText;
    
    public void setLocalPosition(Vector3 localPosition)
    {
        point.transform.localPosition = localPosition;
    }

    public void setXTextPosition(float offset)
    {
        xText.transform.position = new Vector3(
            xText.transform.position.x,
            offset,
            0
        );
    }

    public Vector3 getPosition()
    {
        return point.transform.position; 
    }
    
    public Point(GameObject go, Transform parent, float value, Color32 color)
    {
        point = go;
        
        for (int i = 0; i < point.transform.childCount; i++)
        {
            Transform child = point.transform.GetChild(i);
            switch (child.name)
            {
                case "Point":
                    pointImg = child.GetComponent<Image>(); 
                    pointImg.color = color;
                break;

                case "X":
                    xText = child.GetComponent<Text>(); 
                    xText.text = System.DateTime.Now.ToString("mm:ss");
                    x = Time.time;
                break;

                case "Y":
                    yText = child.GetComponent<Text>(); 
                    yText.text = ((int) value).ToString();
                    y = value;
                break;
            }
        }
        point.transform.SetParent(parent);
        point.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }
}

public class GraphController : MonoBehaviour
{
    public int rangeY;
    public int rangeX;
    public int interval;
    public float xTextOffset;
    public Image container;
    public int borderOffset;

    public List<Color32> pointColor;
    public List<LineRenderer> lines = new List<LineRenderer>();
    private List<List<Point>> points = new List<List<Point>>();
    private float xMax = 0.0f;
    private float yMax = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        SetupGraph();
        // GenerateData();
    }

    void SetupGraph()
    {
        yMax = container.rectTransform.sizeDelta.y - 2 * borderOffset;
        xMax = container.rectTransform.sizeDelta.x - 2 * borderOffset;
        for (int j = 0; j < lines.Count; j++)
        {
            lines[j].positionCount = 0;
            points.Add(new List<Point>());
        }
    }

    void Update()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            lines[i].positionCount = points[i].Count;
            for (int j = 0; j < lines[i].positionCount; j++)
            {
                lines[i].SetPosition(j, points[i][j].getPosition());
            }
        }
    }

    void UpdateGraph()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            List<Point> pt = points[i];

            for (int j = 0; j < pt.Count; j++)
            {
                float yIndex = pt[j].y / rangeY * yMax - yMax / 2;
                float percent = (pt[j].x - pt[0].x) / (rangeX * interval);
                float xIndex = (percent * xMax) - xMax / 2;
                
                pt[j].setLocalPosition(new Vector3(xIndex, yIndex, 0));
                pt[j].setXTextPosition(container.transform.position.y - xTextOffset);
            }
        }
    }


    public void UpdateData(List<float> data)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (points[i].Count == rangeX) points[i].RemoveAt(0);

            Point point = new Point(
                Instantiate(Resources.Load("Point")) as GameObject,
                container.transform,
                data[i],
                pointColor[i]);
            
            points[i].Add(point);

            if (points[i].Count > 1 && point.x - points[i][points[i].Count - 2].x < interval)
            {
                GameObject removePt = points[i][points[i].Count - 2].point;
                points[i].RemoveAt(points[i].Count - 2);
                Destroy(removePt);
            }

            while (point.x - points[i][0].x > interval * rangeX)
            {
                GameObject removePt = points[i][0].point;
                points[i].RemoveAt(0);
                Destroy(removePt);
            }
        }
        UpdateGraph();
    }

    IEnumerator _GenerateData()
    {
        while (true)
        {
            List<float> data = new List<float>(new float[2]{Random.Range(0f, (float) rangeY), Random.Range(0f, (float) rangeY)});
            UpdateData(data);
            yield return new WaitForSeconds(1f);
        }
    }

    private void GenerateData()
    {
        StartCoroutine(_GenerateData());
    }

}
