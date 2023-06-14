import RPi.GPIO as GPIO
import time
import os

from pyairtable import Table

from lcd_api import LcdApi
from i2c_lcd import I2cLcd

GPIO.setmode(GPIO.BCM)

# airtable setup
AIRTABLE_TOKEN="patjkhzNh77ta0pAA.22879e8bba74b3c0cfc3da97f7ff13eca9129e7b6dad6c805d996a6204ae5cc6"
Apikey="keyBWmSRLypBsJUvo"
AIRTABLE_BASE_ID="appMyhojgXDHPge6s"
AIRTABLE_RAIN="recHIIbfGqdSEDiKR"
AIRTABLE_SUNBRIGHT="reclZM4dzrlFPbRmn"
AIRTABLE_TEMPERATURE="recnrKp7BbQOarpCm"
AIRTABLE_HUMIDITY="recxxnqEI0F3Ahz42"
AIRTABLE_URL = f"https://api.airtable.com/v0/{AIRTABLE_BASE_ID}"

def ShowRecords():
	global AIRTABLE_TOKEN
	global AIRTABLE_BASE_ID
	table = Table(AIRTABLE_TOKEN, AIRTABLE_BASE_ID, "WeatherTable")
	for each in table.all():
		print(each)

# Rain Sensor Setup
RainPin = 12 # GPIO 12

GPIO.setup(RainPin, GPIO.IN)

# Rain Detection
'''
while True:
    if(not GPIO.input(rainPin)):
        print("RAINING!!!")

    time.sleep(0.5)
    
'''

def CheckRain():
	global rainWater
	if(not GPIO.input(RainPin)): rainWater = 1
	else: rainWater = 0


# Light Sensor setup
LightPin = 5 # GPIO 5

def rc_time(pin_to_circuit):
    count = 0

    # output on the pin for
    GPIO.setup(pin_to_circuit, GPIO.OUT)
    GPIO.output(pin_to_circuit, GPIO.LOW)
    time.sleep(0.1)

    # Change the pin back to input
    GPIO.setup(pin_to_circuit, GPIO.IN)

    # Count until the pin goes high
    while (GPIO.input(pin_to_circuit) == GPIO.LOW):
        count += 1

    return count

def CheckDayLight():
	global LightPin
	global dayLight
	if(rc_time(LightPin) > 10000): dayLight = 0
	else: dayLight = 1

# display setup
I2C_ADDR = 0x27
I2C_NUM_ROWS = 2
I2C_NUM_COLS = 16

lcd = I2cLcd(1, I2C_ADDR, I2C_NUM_ROWS, I2C_NUM_COLS)

def LcdPrint(temp, humid, dayL, rainW):
	try:
		lcd.clear()
		lcd.putstr(f"Temp:{temp} Humid:{humid}")
		lcd.move_to(0, 1)
		lcd.putstr(f"Day:{dayL} Rain:{rainW}")
	except:
		print("Not displaying")

# HumiTure setup
DHTPIN = 13			# DHT data pin

MAX_UNCHANGE_COUNT = 100

STATE_INIT_PULL_DOWN = 1
STATE_INIT_PULL_UP = 2
STATE_DATA_FIRST_PULL_DOWN = 3
STATE_DATA_PULL_UP = 4
STATE_DATA_PULL_DOWN = 5

def read_dht11_dat():
	GPIO.setup(DHTPIN, GPIO.OUT)
	GPIO.output(DHTPIN, GPIO.HIGH)
	time.sleep(0.05)
	GPIO.output(DHTPIN, GPIO.LOW)
	time.sleep(0.02)
	GPIO.setup(DHTPIN, GPIO.IN, GPIO.PUD_UP)

	unchanged_count = 0
	last = -1
	data = []
	while True:
		current = GPIO.input(DHTPIN)
		data.append(current)
		if last != current:
			unchanged_count = 0
			last = current
		else:
			unchanged_count += 1
			if unchanged_count > MAX_UNCHANGE_COUNT:
				break

	state = STATE_INIT_PULL_DOWN

	lengths = []
	current_length = 0

	for current in data:
		current_length += 1

		if state == STATE_INIT_PULL_DOWN:
			if current == GPIO.LOW:
				state = STATE_INIT_PULL_UP
			else:
				continue
		if state == STATE_INIT_PULL_UP:
			if current == GPIO.HIGH:
				state = STATE_DATA_FIRST_PULL_DOWN
			else:
				continue
		if state == STATE_DATA_FIRST_PULL_DOWN:
			if current == GPIO.LOW:
				state = STATE_DATA_PULL_UP
			else:
				continue
		if state == STATE_DATA_PULL_UP:
			if current == GPIO.HIGH:
				current_length = 0
				state = STATE_DATA_PULL_DOWN
			else:
				continue
		if state == STATE_DATA_PULL_DOWN:
			if current == GPIO.LOW:
				lengths.append(current_length)
				state = STATE_DATA_PULL_UP
			else:
				continue
	if len(lengths) != 40:
		#print ("Data not good, skip")
		return False

	shortest_pull_up = min(lengths)
	longest_pull_up = max(lengths)
	halfway = (longest_pull_up + shortest_pull_up) / 2
	bits = []
	the_bytes = []
	byte = 0

	for length in lengths:
		bit = 0
		if length > halfway:
			bit = 1
		bits.append(bit)
	#print ("bits: %s, length: %d" % (bits, len(bits)))
	for i in range(0, len(bits)):
		byte = byte << 1
		if (bits[i]):
			byte = byte | 1
		else:
			byte = byte | 0
		if ((i + 1) % 8 == 0):
			the_bytes.append(byte)
			byte = 0
	#print (the_bytes)
	checksum = (the_bytes[0] + the_bytes[1] + the_bytes[2] + the_bytes[3]) & 0xFF
	if the_bytes[4] != checksum:
		#print ("Data not good, skip")
		return False

	return the_bytes[0], the_bytes[2]

def CheckHT():
	global temperature
	global humidity
	ht = read_dht11_dat()
	if ht:
		humidity, temperature = ht
		

# flgVar
temperature = 0
humidity = 0.0
dayLight = 0
rainWater = 0

def UpdateAirtable(temp, humid, dayL, rainW):
	global AIRTABLE_TEMPERATURE
	global AIRTABLE_HUMIDITY
	global AIRTABLE_SUNBRIGHT
	global AIRTABLE_RAIN
	global AIRTABLE_TOKEN
	global AIRTABLE_BASE_ID
	table = Table(AIRTABLE_TOKEN, AIRTABLE_BASE_ID, "WeatherTable")
	table.update(AIRTABLE_TEMPERATURE, {"Data": temp})
	table.update(AIRTABLE_HUMIDITY, {"Data": humid})
	table.update(AIRTABLE_SUNBRIGHT, {"Data": dayL})
	table.update(AIRTABLE_RAIN, {"Data": rainW})
	

# main loop
try:
	while True:
		CheckRain()
		time.sleep(0.1)
		CheckDayLight()
		time.sleep(0.1)
		CheckHT()
		time.sleep(0.5)
		#print(f"Temperature: {temperature}\nHumidity: {humidity}\nDayLight: {dayLight}\nRainWater: {rainWater}\n==============================")
		LcdPrint(temperature, humidity, dayLight, rainWater)
		UpdateAirtable(temperature, humidity, dayLight, rainWater)
except KeyboardInterrupt:
    GPIO.cleanup()
