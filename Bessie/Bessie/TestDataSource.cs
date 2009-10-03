
using System;

namespace Bessie
{
	
	
	public class TestDataSource : DataSource
	{
		
		private int lastThrottlePos = 0, lastBatteryCurrent = 0, lastDiodeTemp = -5,
			lastBatteryVoltage = 72, lastOutputCurrent = 0;
		
		private Random rng = new Random();
		
        public void InitDataSource()
        {
			; // do nothing!
        }

        public void CloseDataSource()
        {
            ; // do nothing!
        }

		private bool isTPosAsc = true;
        public float GetThrottlePos()
        {
            if (isTPosAsc) lastThrottlePos += 3;
			else lastThrottlePos -= 3; 
			
			if (lastThrottlePos >= 100) { isTPosAsc = false; lastThrottlePos = 100; }
			else if (lastThrottlePos <= 0) isTPosAsc = true;
			
			return lastThrottlePos;
        }

		private bool isBCurAsc = true;
        public float GetBatteryCurrent()
        {
            if (isBCurAsc) lastBatteryCurrent += 3;
			else lastBatteryCurrent -= 3; 
			
			if (lastBatteryCurrent >= 390) isBCurAsc = false;
			else if (lastBatteryCurrent <= 120) isBCurAsc = true;
			
			return lastBatteryCurrent;
        }

		private bool isDTempAsc = true;
        public float GetDiodeTemp()
        {
             if (isDTempAsc) lastDiodeTemp += 1;
			else lastDiodeTemp -= 1; 
			
			if (lastDiodeTemp >= 90) isDTempAsc = false;
			else if (lastDiodeTemp <= 75) isDTempAsc = true;
			
			return lastDiodeTemp;
        }

        public float GetBatteryVoltage()
        {
			if (lastBatteryVoltage <= 55) lastBatteryVoltage = 60;
			else if (lastBatteryVoltage >= 92) lastBatteryVoltage = 88;
			else lastBatteryVoltage += rng.Next(-5, 5);
            return lastBatteryVoltage;
        }

		private int countSinceLastError = 0;
        public DataSourceError getErrorStatus()
        {	
			if (countSinceLastError >= 150) countSinceLastError = 0;
			else countSinceLastError++;
			
            switch (countSinceLastError / 10) {
			case 2:
				return DataSourceError.ThrottlePosOverRange;
			case 4: 
				return DataSourceError.BatteryOverVoltage;
			case 6:
				return DataSourceError.BatteryUnderVoltage;
			case 8:
				return DataSourceError.OverTemp;
			case 10:
				return DataSourceError.UnderTemp;
			case 12:
				return DataSourceError.ThrottleNotGoneZero;
			default:
				return DataSourceError.NoError;
			}
        }

		private bool isOCurAsc = true;
        public float GetOutputCurrent()
        {
             if (isOCurAsc) lastOutputCurrent += 5;
			else lastOutputCurrent -= 2; 
			
			if (lastOutputCurrent >= 380) isOCurAsc = false;
			else if (lastOutputCurrent <= 140) isOCurAsc = true;
			
			return lastOutputCurrent;
        }

	}
}
