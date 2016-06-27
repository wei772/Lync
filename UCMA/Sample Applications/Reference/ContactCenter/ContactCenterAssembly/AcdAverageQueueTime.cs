/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Signaling;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    /// <summary>
    /// AcdAverageQueueTime
    /// This is a very approximate implementation to compute the average time spent by caller in a virtual portal queue based on an empirical 
    /// moving average. This does not really conform to the queuing theory and shall be revisited for a real-life implementation.
    /// </summary>
    internal class AcdAverageQueueTime
    {
        private AcdPortal _portal;
        private const int MovingAverageLength = 10;
        private TimeSpan InitialAverageQueueTime = new TimeSpan(0,0,0,30); // 30s is the initial matching time
        private TimeSpan _averageQueueTime;
        private TimeSpan[] arrayOfRecentMatchesDurations= new TimeSpan[MovingAverageLength];
        private int _index = 0;
        private bool _wrapped = false;
        private long _weightedMovingAverageNumerator = 0;
        private TimerWheel _tmrWheel = new TimerWheel();
        private TimerItem _tmr;
        private object _syncRoot = new object();

        internal AcdAverageQueueTime(AcdPortal portal)
        {
          _portal=portal;

          _averageQueueTime = InitialAverageQueueTime;

           
        }

        internal TimeSpan Value
        {
            get { return _averageQueueTime; }
        }

        internal void RefreshTimerOnMatchRequested()
        {
            if (null == _tmr)
            {
              _tmr = new TimerItem(_tmrWheel, _averageQueueTime);
              _tmr.Expired += new EventHandler(OnTimerExpired);
              _tmr.Start();
            }
            else
            {
              _tmr.Reset(_averageQueueTime);
            }


        }

        internal void ReEvaluate(TimeSpan timeToMatch)
        {
            lock(_syncRoot)
            {

               if (_wrapped)
               { 
                   long arraySum = 0;

                   for (int i=0; i < MovingAverageLength; i++)
                   {
                      arraySum += arrayOfRecentMatchesDurations[i].Ticks;
                   }

                   arrayOfRecentMatchesDurations[_index++] = timeToMatch;

                   _weightedMovingAverageNumerator = _weightedMovingAverageNumerator + MovingAverageLength * timeToMatch.Ticks - arraySum;

                   long weightedMovingAverageDenominator = MovingAverageLength * (MovingAverageLength + 1) / 2;

                   long averageQueueTimeInTicks = _weightedMovingAverageNumerator / weightedMovingAverageDenominator;

                   _averageQueueTime = new TimeSpan(averageQueueTimeInTicks);

               
               }
               else
               {
                   _weightedMovingAverageNumerator = 0;

                   arrayOfRecentMatchesDurations[_index++] = timeToMatch;
 
                   for (int i=0; i < _index; i++)
                   {
                     _weightedMovingAverageNumerator += arrayOfRecentMatchesDurations[i].Ticks * (i+1);
                   }

                   int weightedMovingAverageDenominator = _index * (_index + 1) / 2;

                   long averageQueueTimeInTicks = _weightedMovingAverageNumerator / weightedMovingAverageDenominator;

                   _averageQueueTime = new TimeSpan(averageQueueTimeInTicks);

               }

               
               if (_index == MovingAverageLength)
               {
                 if (!_wrapped)
                 {
                     _wrapped = true;
                 }
                 _index = 0;

               }

            }
        
        }

        private void OnTimerExpired(object sender, EventArgs args)
        {
            lock (_syncRoot)
            {
                //resetting initial settings
                _averageQueueTime = InitialAverageQueueTime;
                _wrapped = false;
                _index = 0;
            
            }
        }


    }

}
