﻿using System;

namespace Dash2
{
    interface DataSource
    {
        void InitDataSource();
        void CloseDataSource();

        decimal getThrottlePos(); //percentage
        decimal getDiodeTemp();   //Error range <= -25C and >= 95C. Likely range is -10C - 85C
        decimal getBatteryVoltage(); //Around 55 - 90V???
        DataSourceError getErrorStatus();  //See below.
        decimal getOutputCurrent(); //amps.  0-400amps
        decimal getBatteryCurrent(); //amps.  0-400amps
    }

    public enum DataSourceError
    {
        ThrottlePosOverRange, UnderTemp, ThrottleNotGoneZero,
        OverTemp, BatteryUnderVoltage, BatteryOverVoltage, NoError
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