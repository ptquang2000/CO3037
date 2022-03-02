print("Xin chào ThingsBoard")
import paho.mqtt.client as mqttclient
import time
import json
import random

BROKER_ADDRESS = "mqttserver.tk"
PORT = 1883
USERNAME = "smart_fan"
PASSWORD = "IndustrySmartFanHT"


def subscribed(client, userdata, mid, granted_qos):
    print("Subscribed...")


def recv_message(client, userdata, message):
    print("Received: ", message.payload.decode("utf-8"))
    try:
        jsonobj = json.loads(message.payload)
        collect_data["data_ss"][2]["ss_value"] = jsonobj["temperature_max"]
        collect_data["data_ss"][3]["ss_value"] = jsonobj["temperature_min"]
        collect_data["data_ss"][4]["ss_value"] = jsonobj["mode_fan_auto"]
        collect_data["device_status"] = not jsonobj["mode_fan_auto"]
    except KeyError:
        collect_data["data_ss"][5]["ss_value"] = jsonobj["fan_status"]
        collect_data["device_status"] = jsonobj["device_status"]


def connected(client, usedata, flags, rc):
    if rc == 0:
        print("MqttServer connected successfully!!")
        client.subscribe("/industrial_fan_ht/smart_fan/STATION_ID_001/config", 1)
        client.subscribe("/industrial_fan_ht/smart_fan/STATION_ID_001/fan", 1)
    else:
        print("Connection is failed", rc)


client = mqttclient.Client("Gateway_MQTTSERVER_TK")
client.username_pw_set(USERNAME, PASSWORD)

client.on_connect = connected
client.connect(BROKER_ADDRESS, 1883)
client.loop_start()

client.on_subscribe = subscribed
client.on_message = recv_message

temp = 0
humidity = 0
collect_data = {
"project_id": "SMARTFAN",
"project_name": "SMART FAN",
"station_id": "STATION_ID_001",
"station_name": "STATION_NAME_001",
"longitude": 106.660172,
"latitude": 10.762622,
"volt_battery": 12.2,
"volt_solar": 5.3,
"data_ss": [
    {"ss_name": "fan_temperature", "ss_unit": "0C", "ss_value": 33.3},
    {"ss_name": "fan_humidity", "ss_unit": "%", "ss_value": 88.8},
    {"ss_name": "temperature_max", "ss_unit": "0C", "ss_value": 30.1},
    {"ss_name": "temperature_min", "ss_unit": "0C", "ss_value": 15.0},
    {"ss_name": "mode_fan_auto", "ss_unit": "", "ss_value": 0},
    {"ss_name": "fan_status", "ss_unit": "", "ss_value": 0}
    ],
"device_status": 0}


while True:
    temp = round(random.uniform(0, 100), 1)
    humidity = round(random.uniform(0, 100), 1)

    collect_data["data_ss"][0]["ss_value"] = temp
    collect_data["data_ss"][1]["ss_value"] = humidity
    if collect_data["data_ss"][4]["ss_value"] == 1:
        if temp > collect_data["data_ss"][2]["ss_value"]:
            collect_data["data_ss"][5]["ss_value"] = 1
        else:
            collect_data["data_ss"][5]["ss_value"] = 0

    client.publish('/industrial_fan_ht/smart_fan/STATION_ID_001/status', json.dumps(collect_data), 1, True)

    time.sleep(10)