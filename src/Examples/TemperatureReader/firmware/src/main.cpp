#include "Arduino.h"

extern "C" uint32_t set_arm_clock(uint32_t frequency); 

void setup()
{
    pinMode(LED_BUILTIN, OUTPUT);
}

elapsedMillis stopwatch;

void loop()
{
    if (Serial.available() > 0)
    {
        String cmd = Serial.readStringUntil('\n');
        if (cmd.length() > 2 && cmd[1] == ' ')
        {
            switch (cmd[0])
            {
                case 'v': {                                                // firmware version
                    Serial.println("TempMon v1.0");
                    break;
                }

                case 's': {                                                // set cpu freqency
                    uint32_t val = (uint32_t)cmd.substring(2).toInt();
                    val = max(50'000'000u, min(800'000'000u, val));
                    set_arm_clock(val);
                    Serial.println(F_CPU_ACTUAL);
                    break;
                }

                case 'r': {                                                // read temperature
                    Serial.println(tempmonGetTemp());
                    break;
                }

                default:
                    break;
            }
        }
    }

    if (stopwatch > 250)
    {
        stopwatch -= 250;
        digitalToggleFast(LED_BUILTIN);
    }
}
