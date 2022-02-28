using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using M2MqttUnity;

[Serializable]
public class sendMsg {
    public bool led;
    public bool pump;

    public sendMsg(bool led, bool pump)
    {
        this.led = led;
        this.pump = pump;
    }
}

public class MqttUnityClient : M2MqttUnityClient
{
    public bool autoTest = false;
    public Text errorReport;
    public Text temperatureValue;
    public Text humidityValue;
    public Toggle ledToggle;
    public Toggle pumpToggle;

    public InputField addressInputField;
    public InputField usernameInputField;
    public InputField passwordInputField;
    public Button connectButton;

    private List<string> eventMessages = new List<string>();
    private bool updateUI = false;

    public void PublishTopics()
    {
        string msg = JsonUtility.ToJson(new sendMsg(this.ledToggle.isOn, this.pumpToggle.isOn));
        client.Publish("v1/devices/me/attributes", System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        Debug.Log("Publishing " + msg);
        AddUiMessage("Test message published.");
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
        SceneManager.LoadScene("Scenes/MainScene", LoadSceneMode.Single);

        if (autoTest)
        {
            PublishTopics();
        }
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { "v1/devices/me/rpc/request/+" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
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

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        base.Awake();
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
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
        StoreMessage(msg);
    }

    private void StoreMessage(string eventMsg)
    {
        eventMessages.Add(eventMsg);
    }

    private void ProcessMessage(string msg)
    {
        AddUiMessage("Received: " + msg);
        Data data = JsonUtility.FromJson<Data>(msg);
        if (this.temperatureValue != null)
        {
            this.temperatureValue.text = (data.temperature).ToString() + "°C";
        }
        if (this.humidityValue != null)
        {
            this.humidityValue.text = (data.humidity).ToString() + "%";
        }
        if (this.ledToggle != null)
        {
            this.ledToggle.isOn = data.led;
        }
        if (this.pumpToggle != null)
        {
            this.pumpToggle.isOn = data.pump;
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
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    private void OnValidate()
    {
        if (autoTest)
        {
            autoConnect = true;
        }
    }
}
