# Synthetic-Relationship
## Diagram Week 01
![Frame 1](https://github.com/BakariSp/Creative_tech_03/assets/46394756/ea29f6b2-0ce3-4445-bd5c-0a375a3e56a1)
![Diagram](https://github.com/BakariSp/Creative_tech_03/assets/46394756/d8c67860-3c8f-4231-8e1f-c578955f9f95)

## Diagram Week 02
![Frame 2](https://github.com/BakariSp/Creative_tech_03/assets/46394756/a4ea000c-a136-47b7-a75e-57541653db46)

'''
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

'''
