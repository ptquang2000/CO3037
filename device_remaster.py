print("Xin chào ThingsBoard")
import paho.mqtt.client as mqttclient
import time
import json
import random

BROKER_ADDRESS = "mqttserver.tk"
PORT = 1883
USERNAME = "bkiot"
PASSWORD = "12345678"

temp = 0
humidity = 0
collect_data = {
    "project_id": "SMARTFARM",
    "project_name": "SMART FARM",
    "station_id": "STATION_ID_001",
    "station_name": "STATION_NAME_001",
    "longitude": 106.660172,
    "latitude": 10.762622,
    "volt_battery": 12.2,
    "volt_solar": 5.3,
    "data_ss": [
        {"ss_name": "temperature", "ss_unit": "°C", "ss_value": 0.0},
        {"ss_name": "humidity", "ss_unit": "%", "ss_value": 0.0},
        {"ss_name": "led_status", "ss_unit": "", "ss_value": "ON"},
        {"ss_name": "pump_status", "ss_unit": "", "ss_value": "OFF"}
        ],
    "device_status": 0
}

topics = [
    "/bkiot/1813678/status",
    "/bkiot/1813678/led",
    "/bkiot/1813678/pump"
]

def subscribed(client, userdata, mid, granted_qos):
    print("Subscribed...")


def recv_message(client, userdata, message):
    print("Received: ", message.payload.decode("utf-8"))
    jsonobj = json.loads(message.payload)

    if message.topic == topics[1]:
        collect_data["data_ss"][2]["ss_value"] = jsonobj["status"]
        time.sleep(1)
        publish(1)

    if message.topic == topics[2]:
        collect_data["data_ss"][3]["ss_value"] = jsonobj["status"]
        time.sleep(1)
        publish(1)


def connected(client, usedata, flags, rc):
    if rc == 0:
        print("MqttServer connected successfully!!")
        for topic in topics:
            client.subscribe(topic, 1)
    else:
        print("Connection is failed", rc)


client = mqttclient.Client("Gateway_MQTTSERVER_TK")
client.username_pw_set(USERNAME, PASSWORD)

client.on_connect = connected
client.connect(BROKER_ADDRESS, 1883)
client.loop_start()
client.on_subscribe = subscribed
client.on_message = recv_message


def publish(device_status=0):
    temp = round(random.uniform(0, 100), 1)
    humidity = round(random.uniform(0, 100), 1)

    collect_data["data_ss"][0]["ss_value"] = temp
    collect_data["data_ss"][1]["ss_value"] = humidity

    collect_data["device_status"] = device_status
    client.publish(topics[0], json.dumps(collect_data), 1, True)


while True:
    publish()
    time.sleep(random.randint(0, 4))