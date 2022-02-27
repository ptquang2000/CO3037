using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Net;

public class MqttUnityClient : MonoBehaviour 
{
    private MqttClient client;
    public InputField brokerURIInputField;
    public InputField usernameInputField;
    public InputField passwordInputField;
    public InputField errorReport;

    void Start()
    {
    }

    void Update()
    {
    }

    public void connect()
    {
    //     var thread = new System.Threading.Thread(doConnect);
    //     thread.Start();
    // }

    // void doConnect()
    // {
        Debug.Log("Connecting");
        string username = "ALijlvbiUqTZJ4G710q3";
        string clientId = "unity-client";
        string brokerURI = "demo.thingsboard.io";
        this.client = new MqttClient(brokerURI);
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
        client.Connect(clientId, username, null);
        client.Subscribe(new string[] { "v1/devices/me/rpc/request/+" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        Debug.Log("Received Message");
        this.errorReport.text = System.Text.Encoding.UTF8.GetString((e.Message));
    }
}
