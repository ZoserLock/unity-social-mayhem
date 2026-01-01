using System;

namespace Zen.Core
{
    public sealed class Watch
    {
        private static  DateTime _staticTikTime = DateTime.Now;
        
        private DateTime _tikTime = DateTime.Now;

        // Sets the time to the current time.
        public void Reset()
        {
            _tikTime = DateTime.Now;
        }
        
        // Returns the time in seconds since the last Reset call.
        // if Reset was never called, it will return the time since the Watch was created.
        public float Time()
        {
            return (float)(DateTime.Now - _tikTime).TotalSeconds;
        }
        
        // Sets the time to the current time.
        public static void Tik()
        {
            _staticTikTime = DateTime.Now;
        }
        
        // Returns the time in seconds since the last Tik call.
        // if Tik was never called, it will return the time since the Watch was created.
        public static float Tok()
        {
            return (float)(DateTime.Now - _staticTikTime).TotalSeconds;
        }
        
    }
}