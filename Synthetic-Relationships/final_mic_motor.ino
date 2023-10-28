// #include <stdio.h>
#include "config.h"
#define FEED_OWNER "pfyuan110"

// set up the `sharedFeed`
AdafruitIO_Feed *sharedFeed = io.feed("p_feed", FEED_OWNER);
AdafruitIO_Feed *microphoneValue = io.feed("microphonevalue");
int getMsg = 0;
int motorCycle = 0;

// microphone set up
#define potPin A3
int potValue = 0;

// set up motor
int motor1Pin1 = 33;
int motor1Pin2 = 32;
int enable1Pin = 27;

// Setting PWM properties
const int freq = 30000;
const int pwmChannel = 0;
const int resolution = 8;
int dutyCycle = 200;

int threshold = 1000;

void setup() {
  // for motor
  // sets the pins as outputs:
  pinMode(motor1Pin1, OUTPUT);
  pinMode(motor1Pin2, OUTPUT);
  pinMode(enable1Pin, OUTPUT);
  pinMode(potPin, INPUT);

  // configure LED PWM functionalitites
  ledcSetup(pwmChannel, freq, resolution);

  // attach the channel to the GPIO to be controlled
  ledcAttachPin(enable1Pin, pwmChannel);

  // serial setting
  // start the serial connection
  Serial.begin(115200);

  // wait for serial monitor to open
  while (!Serial)
    ;

  // connect to io.adafruit.com
  Serial.print("Connecting to Adafruit IO");
  io.connect();

  // set up a message handler for the 'sharedFeed' feed.
  // the handleMessage function (defined below)
  // will be called whenever a message is
  // received from adafruit io.
  sharedFeed->onMessage(handleMessage);

  // wait for a connection
  while (io.status() < AIO_CONNECTED) {
    Serial.print(".");
    delay(500);
  }

  // we are connected
  Serial.println();
  Serial.println(io.statusText());
  sharedFeed->get();
  delay(1000);
}

void loop() {
  // io.run(); is required for all sketches.
  // it should always be present at the top of your loop
  // function. it keeps the client connected to
  // io.adafruit.com, and processes any incoming data.
  io.run();

  // upload microphone value
  potValue = analogRead(potPin);
  Serial.print("Sending Microphone Value -> ");
  Serial.println(potValue);
  microphoneValue->save(potValue);

  getMsg = min(getMsg, 2500);
  if (getMsg > threshold) {
    motorCycle = map(getMsg, threshold, 2500, 170, 220);
    ledcWrite(pwmChannel, motorCycle);
    Serial.print("Forward Duty cycle: ");
    Serial.println(motorCycle);
    digitalWrite(motor1Pin1, LOW);
    digitalWrite(motor1Pin2, HIGH);
  } else {
    motorCycle = 160;
    ledcWrite(pwmChannel, motorCycle);
    Serial.print("LOW LIGHT Duty cycle: ");
    Serial.println(motorCycle);
    digitalWrite(motor1Pin1, LOW);
    digitalWrite(motor1Pin2, LOW);
  }


  delay(2000);
  // limitation of Adafruit IO
  // delay(2000);
}

// this function is called whenever an 'sharedFeed' feed message
// is received from Adafruit IO. it was attached to
// the 'digital' feed in the setup() function above.
void handleMessage(AdafruitIO_Data *data) {
  Serial.print("received <-  ");
  getMsg = data->toInt();
  Serial.println(getMsg);
}
