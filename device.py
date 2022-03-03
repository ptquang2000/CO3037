print("Xin chào ThingsBoard")
import paho.mqtt.client as mqttclient
import time
import json
import random

BROKER_ADDRESS = "mqttserver.tk"
PORT = 1883
USERNAME = "smart_fan"
PASSWORD = "IndustrySmartFanHT"

temp = 0
humidity = 0
collect_data = {
"project_id": "SMARTFAN",
"project_name": "SMART FAN",
"station_id": "STATION_ID_002",
"station_name": "STATION_NAME_002",
"longitude": 106.660172,
"latitude": 10.762622,
"volt_battery": 12.2,
"volt_solar": 5.3,
"data_ss": [
    {"ss_name": "fan_temperature", "ss_unit": "0C", "ss_value": 0.0},
    {"ss_name": "fan_humidity", "ss_unit": "%", "ss_value": 0.0},
    {"ss_name": "temperature_max", "ss_unit": "0C", "ss_value": 30.1},
    {"ss_name": "temperature_min", "ss_unit": "0C", "ss_value": 15.0},
    {"ss_name": "mode_fan_auto", "ss_unit": "", "ss_value": 0},
    {"ss_name": "fan_status", "ss_unit": "", "ss_value": 0}
    ],
"device_status": 0}

topics = [
    "/industrial_fan_ht/smart_fan/STATION_ID_002/config",
    "/industrial_fan_ht/smart_fan/STATION_ID_002/fan"
]

def subscribed(client, userdata, mid, granted_qos):
    print("Subscribed...")


def recv_message(client, userdata, message):
    print("Received: ", message.payload.decode("utf-8"))
    jsonobj = json.loads(message.payload)
    if message.topic == topics[0]:
        collect_data["data_ss"][2]["ss_value"] = jsonobj["temperature_max"]
        collect_data["data_ss"][3]["ss_value"] = jsonobj["temperature_min"]
        collect_data["data_ss"][4]["ss_value"] = jsonobj["mode_fan_auto"]
        time.sleep(2)
        publish(1)
    if message.topic == topics[1]:
        if jsonobj["device_status"] == 0:
            collect_data["data_ss"][5]["ss_value"] = jsonobj["fan_status"]
            jsonobj["device_status"] = 1
            time.sleep(2)
            client.publish(topics[1], json.dumps(jsonobj), 1, True)


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
    if collect_data["data_ss"][4]["ss_value"] == 1:
        if temp > collect_data["data_ss"][2]["ss_value"]:
            collect_data["data_ss"][5]["ss_value"] = 1
        else:
            collect_data["data_ss"][5]["ss_value"] = 0

    collect_data["device_status"] = device_status
    client.publish('/industrial_fan_ht/smart_fan/STATION_ID_002/status', json.dumps(collect_data), 1, True)


while True:
    publish()
    time.sleep(10)