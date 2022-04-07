import random
import sys
import traceback
import time
import serial.tools.list_ports
import threading
import signal



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
            'led': 0,
            'pump': 0,
        }
       

    def _serialWrite(self):
        if self._ser:
            data = f"!1:HUMI:{self._collected_data['humidity']}#"
            data += f"!1:TEMP:{self._collected_data['temperature']}#"
            print("Sent " + data)
            self._ser.write(data.encode())


    def _processData(self, data):
        data = data.replace("!", "")
        data = data.replace("#", "")
        splitData = data.split(":")
        print(splitData)
        self._collected_data[self._collected_data[splitData[1].lower()]] = int(splitData[2])


    def _serialRead(self):
        bytesToRead = self._ser.inWaiting()
        if (bytesToRead > 0):
            self._mess = self._mess + self._ser.read(bytesToRead).decode("UTF-8")
            while ("#" in self._mess) and ("!" in self._mess):
                start = self._mess.find("!")
                end = self._mess.find("#")
                self._processData(self._mess[start:end + 1])
                if (end == len(self._mess)):
                    self._mess = ""
                else:
                    self._mess = self._mess[end+1:]
            return True


    def _writter(self):
        while True:
            self._collected_data['humidity'] = random.randint(1, 100)
            self._collected_data['temperature'] = random.randint(1, 100)
            self._serialWrite()
            time.sleep(1)

    
    def run(self, delay=0.1):
        writter = threading.Thread(target=self._writter, args=())
        writter.daemon = True
        writter.start()

        while True:
            self._serialRead()
            time.sleep(delay)
            

def main():
    device = SerialPort("COM4")
    device.run()

    
if __name__ == '__main__':
    print("Xin chào Device")
    main()