#include "Arduino.h"

void setup()
{
  pinMode(LED_BUILTIN,OUTPUT);
}

void loop()
{
  digitalWriteFast(LED_BUILTIN,!digitalReadFast(LED_BUILTIN));
  delay(500);
}
