using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using M2MqttUnity;

[Serializable]
public class DataControl {
    public string device;
    public bool status;

    public DataControl(string name, bool isOn)
    {
        device = name;
        status = isOn;
    }
}

[Serializable]
public class DataStatus {
    public float temperature;
    public float humidity;
    public DataStatus (float agr1, float agr2)
    {
        temperature = agr1;
        humidity = agr2;
    }
}

public class MqttUnityClient : M2MqttUnityClient
{
    public Text errorReport;
    public Text temperatureValue;
    public Text humidityValue;
    public Toggle ledToggle;
    public Toggle pumpToggle;

    public InputField addressInputField;
    public InputField usernameInputField;
    public InputField passwordInputField;
    public Button connectButton;

    public GraphController graphController;

    public List<string> topics = new List<string>();
    private List<string> eventMessages = new List<string>();
    private bool updateUI = false;

    public void PublishTopics(String topic, String msg)
    {
        client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
        Debug.Log("Publishing " + msg);
        AddUiMessage("Test message published.");
    }

    public void onValueChangeLed()
    {
        ledToggle.interactable = false;
        string msg = JsonUtility.ToJson(new DataControl("LED", this.ledToggle.isOn));
        PublishTopics(topics[1], msg);
    }

    public void onValueChangePump()
    {
        pumpToggle.interactable = false;
        string msg = JsonUtility.ToJson(new DataControl("PUMP", this.pumpToggle.isOn));
        PublishTopics(topics[2], msg);
    }

    public void SetBrokerAddress(string brokerAddress)
    {
        if (addressInputField && !updateUI)
        {
            this.brokerAddress = brokerAddress;
        }
    }

    public void SetUsername(string username)
    {
        if (usernameInputField && !updateUI)
        {
            this.mqttUserName = username;
        }
    }


    public void SetPassword(string password)
    {
        if (passwordInputField && !updateUI)
        {
            this.mqttPassword = password;
        }
    }

    public void SetUiMessage(string msg)
    {
        if (errorReport != null)
        {
            errorReport.text = msg;
            updateUI = true;
        }
    }

    public void AddUiMessage(string msg)
    {
        if (errorReport != null)
        {
            errorReport.text += msg + "\n";
            updateUI = true;
        }
    }

    protected override void OnConnecting()
    {
        base.OnConnecting();
        SetUiMessage("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        SetUiMessage("Connected to broker on " + brokerAddress + "\n");
        SubscribeTopics();
        
        // client.Publish(topics[0], System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new DataStatus(31f, 70f))), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);

        SwitchScene();
        GetData();
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { topics[0] }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        client.Subscribe(new string[] { topics[1] }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        client.Subscribe(new string[] { topics[2] }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    protected override void UnsubscribeTopics()
    {
        // client.Unsubscribe(new string[] { "M2MQTT_Unity/test" });
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        AddUiMessage("CONNECTION FAILED! " + errorMessage);
    }

    protected override void OnDisconnected()
    {
        AddUiMessage("Disconnected.");
    }

    protected override void OnConnectionLost()
    {
        AddUiMessage("CONNECTION LOST!");
    }

    private void UpdateUI()
    {
        if (client == null)
        {
            if (connectButton != null)
            {
                connectButton.interactable = true;
            }
        }
        else
        {
            if (connectButton != null)
            {
                connectButton.interactable = !client.IsConnected;
            }
        }
        if (addressInputField != null && connectButton != null)
        {
            addressInputField.interactable = connectButton.interactable;
            addressInputField.text = brokerAddress;
        }
        if (usernameInputField != null && connectButton != null)
        {
            usernameInputField.interactable = connectButton.interactable;
            usernameInputField.text = mqttUserName;
        }
        if (passwordInputField != null && connectButton != null)
        {
            passwordInputField.interactable = connectButton.interactable;
            passwordInputField.text = mqttPassword;
        }
        updateUI = false;
    }

    protected override void Start()
    {
        SetUiMessage("Ready.");
        updateUI = true;
        base.Start();
    }

    public void connectBtnOnClickStart()
    {
        autoConnect = true;
        SetBrokerAddress(this.addressInputField.text);
        SetUsername(this.usernameInputField.text);
        SetPassword(this.passwordInputField.text);
        base.Start();
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("Received: " + msg);

        if (String.Equals(topics[0], topic))
            StoreMessage(msg);
        else if (String.Equals(topics[1], topic))
        {
            DataControl data = JsonUtility.FromJson<DataControl>(msg);
            ledToggle.isOn = data.status;
            ledToggle.interactable = true;
        }
        else if (String.Equals(topics[2], topic))
        {
            DataControl data = JsonUtility.FromJson<DataControl>(msg);
            pumpToggle.isOn = data.status;
            pumpToggle.interactable = true;
        }
    }

    private void StoreMessage(string eventMsg)
    {
        eventMessages.Add(eventMsg);
    }

    private void ProcessMessage(string msg)
    {
        AddUiMessage("Received: " + msg);
        DataStatus data = JsonUtility.FromJson<DataStatus>(msg);
        if (this.temperatureValue != null)
        {
            this.temperatureValue.text = (data.temperature).ToString() + " °C";
        }
        if (this.humidityValue != null)
        {
            this.humidityValue.text = (data.humidity).ToString() + " %";
        }
    }

    protected override void Update()
    {
        base.Update(); // call ProcessMqttEvents()

        if (eventMessages.Count > 0)
        {
            foreach (string msg in eventMessages)
            {
                ProcessMessage(msg);
            }
            eventMessages.Clear();
        }
        if (updateUI)
        {
            UpdateUI();
        }

        if (updateScene == true)
        {
            SwitchOut(canvasScene1);
            SwitchIn(canvasScene2);
        }
        else if (updateScene == false)
        {
            SwitchOut(canvasScene2);
            SwitchIn(canvasScene1);
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    private void OnValidate()
    {
    }

    // Scene Transition
    public CanvasGroup canvasScene1;
    public CanvasGroup canvasScene2;
    public int offsetPositionX;
    public int offsetPositionY;
    public float speed = 1.0F;
    private float startTime;
    private bool updateScene = false;

    void SwitchIn(CanvasGroup _canvas)
    {
        Vector3 targetPos = new Vector3(0, offsetPositionY ,0);
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / offsetPositionX;
        _canvas.transform.localPosition = Vector3.Lerp(_canvas.transform.localPosition, targetPos, fractionOfJourney);
    }
    void SwitchOut(CanvasGroup _canvas)
    {
        Vector3 targetPos;
        if (string.Equals(_canvas.name, "Scene1"))
            targetPos = new Vector3(-offsetPositionX, offsetPositionY, 0);
        else
            targetPos = new Vector3(offsetPositionX, offsetPositionY, 0);
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / offsetPositionX;
        _canvas.transform.localPosition = Vector3.Lerp(_canvas.transform.localPosition, targetPos, fractionOfJourney);
    }

    void SwitchScene()
    {
        startTime = Time.time;
        updateScene = !updateScene;
        if (canvasScene1.interactable == true)
        {
            canvasScene1.interactable = false;
            canvasScene1.blocksRaycasts = false;
            canvasScene2.interactable = true;
            canvasScene2.blocksRaycasts = true;
        }
        else
        {
            canvasScene2.interactable = false;
            canvasScene2.blocksRaycasts = false;
            canvasScene1.interactable = true;
            canvasScene1.blocksRaycasts = true;
        }
    }

    IEnumerator _GetData()
    {
        while (true)
        {
            float value = float.Parse(temperatureValue.text.Split(' ')[0]);
            graphController.UpdateData(graphController.waitQueue1, value);
            value = float.Parse(humidityValue.text.Split(' ')[0]);
            graphController.UpdateData(graphController.waitQueue2, value);
            yield return new WaitForSeconds(graphController.interval);
        }
    }

    void GetData()
    {
        StartCoroutine(_GetData());
    }
}
