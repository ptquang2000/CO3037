using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    private MqttUnityClient mqttUnityClient;
    public Text temperatureValue;
    public Text humidityValue;
    public Toggle ledToggle;
    public Toggle pumpToggle;

    // Start is called before the first frame update
    void Start()
    {
        this.mqttUnityClient = GameObject.Find("MqttClient").GetComponent(typeof(MqttUnityClient)) as MqttUnityClient;
        mqttUnityClient.temperatureValue = temperatureValue;
        mqttUnityClient.humidityValue = humidityValue;
        mqttUnityClient.ledToggle = ledToggle;
        ledToggle.onValueChanged.AddListener(delegate {
            mqttUnityClient.PublishTopics();
        });
        mqttUnityClient.pumpToggle = pumpToggle;
        pumpToggle.onValueChanged.AddListener(delegate {
            mqttUnityClient.PublishTopics();
        });
    }

    // Update is called once per frame
    void Update()
    {
    }
}
