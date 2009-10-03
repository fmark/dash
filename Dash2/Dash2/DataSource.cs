using System;

namespace Dash2
{
    interface DataSource
    {
        void InitDataSource();
        void CloseDataSource();

        float GetThrottlePos(); //percentage
        float GetDiodeTemp();   //Error range <= -25C and >= 95C. Likely range is -10C - 85C
        float GetBatteryVoltage(); //Around 55 - 90V???
        DataSourceError getErrorStatus();  //See below.
        float GetOutputCurrent(); //amps.  0-400amps
        float GetBatteryCurrent(); //amps.  0-400amps
    }

    public enum DataSourceError
    {
        NoError = 0,
        ThrottlePosOverRange = 1, 
        UnderTemp = 2, 
        ThrottleNotGoneZero = 3,
        OverTemp = 4, 
        BatteryUnderVoltage = 6, 
        BatteryOverVoltage = 7 
    }

   /*
1 Red = Throttle Position Sensor Over Range. Check for open wires.
2 Red = Under Temperature. Controller below -25C.
3 Red = HPD. Throttle hasn't gone to zero during this power on cycle.
4 Red = Over Temperature. Controller over 95C.
5 Red = unused for series controllers.
6 Red = Battery Under Voltage detected. Battery V < undervoltage slider.
7 Red = Battery Over Voltage detected. Battery V > overvoltage slider.
    */
}
