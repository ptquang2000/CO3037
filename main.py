import traceback
import paho.mqtt.client as mqttclient
import time
import json
import serial.tools.list_ports


class SerialPort:

    def __init__(self, port, baudrate=115200):
        self._mess = ""
        self._bbc_port = port
        self._baudrate = baudrate
        if len(self._bbc_port) > 0:
            self._ser = serial.Serial(port=self._bbc_port, baudrate=baudrate)

        self._collected_data = {
            'humidity': 0,
            'temperature': 0,
            'light': 60,
            'longitude': 106.661136,
            'latitude': 10.773261,
        }
        self._DATA = {
            'HUMI': 'humidity',
            'TEMP': 'temperature',
            'LIGHT': 'light',
        }
        self._METHOD = {
            'setLED': 'LED',
            'setPUMP': 'PUMP',
            'setLED2': 'LED2',
            'setPUMP2': 'PUMP2',
        }


    def writeSerial(self, rc_data):
        if self._ser:
            if rc_data['method'] == 'setLED':
                data = f"{1 if rc_data['params'] else 0}#"
            elif rc_data['method'] == 'setPUMP':
                data = f"{3 if rc_data['params'] else 2}#"
            elif rc_data['method'] == 'setLED2':
                data = f"{5 if rc_data['params'] else 4}#"
            elif rc_data['method'] == 'setPUMP2':
                data = f"{7 if rc_data['params'] else 6}#"
            print("Sent " + data)
            self._ser.write(data.encode())


    def _processData(self, data):
        data = data.replace("!", "")
        data = data.replace("#", "")
        splitData = data.split(":")
        print(splitData)
        self._collected_data[self._DATA[splitData[1]]] = int(splitData[2])
        


    def readSerial(self):
        bytesToRead = self._ser.inWaiting()
        if (bytesToRead > 0):
            self._mess = self._mess + self._ser.read(bytesToRead).decode("UTF-8")
            while ("#" in self._mess) and ("!" in self._mess):
                start = self._mess.find("!")
                end = self._mess.find("#")
                try:
                    self._processData(self._mess[start:end + 1])
                except:
                    pass
                if (end == len(self._mess)):
                    self._mess = ""
                else:
                    self._mess = self._mess[end+1:]
            return True


    @property
    def collected_data(self):
        return json.dumps(self._collected_data)


class ThingsBoardClient:

    def __init__(self, addr, token, port=1883, com="COM5"):
        self._addr = addr
        self._port = port
        self._token = token

        self._sp = SerialPort(com)
        self._setup()

    
    def _setup(self):
        self._client = mqttclient.Client("Gateway_Thingsboard")
        self._client.username_pw_set(self._token)

        self._client.on_connect = self._connected
        self._client.connect(self._addr, self._port)

        self._client.on_subscribe = self._subscribed
        self._client.on_message = self._recv_message


    def _subscribed(self, client, userdata, mid, granted_qos):
        print("Subscribed...")


    def _recv_message(self, client, userdata, message):
        print("Received: ", message.payload.decode("utf-8"))
        temp_data = {'value': True}
        try:
            jsonobj = json.loads(message.payload)
            temp_data['value'] = jsonobj['params']
            client.publish('v1/devices/me/attributes', json.dumps(temp_data), 1)
            self._sp.writeSerial(jsonobj)
        except:
            pass


    def _connected(self, client, usedata, flags, rc):
        if rc == 0:
            print("Thingsboard connected successfully!!")
            client.subscribe("v1/devices/me/rpc/request/+")
        else:
            print("Connection is failed")

    
    def run(self, delay=1):
        self._client.loop_start()
        while True:
            if self._sp.readSerial():
                self._client.publish('v1/devices/me/telemetry', self._sp.collected_data, 1)
            time.sleep(delay)
            

def main():
    print("Xin chào ThingsBoard")
    BROKER_ADDRESS = "demo.thingsboard.io"
    PORT = 1883
    THINGS_BOARD_ACCESS_TOKEN = "ALijlvbiUqTZJ4G710q3"
    COM = "COM6"
    client = ThingsBoardClient(BROKER_ADDRESS, THINGS_BOARD_ACCESS_TOKEN, PORT, COM)
    client.run()

    
if __name__ == '__main__':
    main()