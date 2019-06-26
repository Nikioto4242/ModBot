﻿using System;

namespace ModLibrary
{
    public static class Delayed
    {
        public static void TriggerAfterDelay(fakeAction action, TimeSpan time)
        {
            WaitThenCallClass.Instance.AddCallback(action, (float)time.TotalSeconds);
        }

        public static void TriggerAfterDelay(fakeAction action, float seconds)
        {
            WaitThenCallClass.Instance.AddCallback(action, seconds);
        }

        public static void TriggerAfterDelay(FakeAction action, float secondsToWait)
        {
            WaitThenCallClass.Instance.AddCallback(action, secondsToWait);
        }
    }
}
