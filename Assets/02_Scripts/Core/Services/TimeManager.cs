using System;
using UnityEngine;

namespace StrangeSpace
{

    public struct GameTime
    {
        public long Milliseconds;

        public float Seconds => Milliseconds * 0.001f;
    }
    
    public interface ITimeManager
    {
        bool IsProcessing { get; }
        bool IsPaused { get; }
        
        long ProcessingTime { get; }
        
        string   CurrentTimeString();
        GameTime CurrentTime();
        
        long CurrentTicks();
        
        void AddTimeMs(long ticks);

        // Pause
        void Pause(bool enabled);
        void TogglePause();

        TimeManagerData GetSaveData();
        void InitializeAsNew();
        void InitializeWithData(TimeManagerData dataTimeManager);

        // Catchup
        void BeginProcessing(long currentTime);
        void EndProcessing();
        void SetProcessingTime(long time);

        long SaveTimeTicks();


    }


    public class TimeManagerData
    {
        public long SaveTimeTicks;
        public long GameStartTicks;
        public long TickOffset;
    }
    
    public class TimeManager : ITimeManager
    {
        private long _sessionStartTicks;
        private long _gameStartTicks;
        private long _saveGameTicks;
        
        private long _tickOffset;
        
        private long _pauseTime;
        
        private long _pauseTimeTick;
        private bool _paused;

        private bool _processing;
        private long _processingTime;
        
        private TimeManagerData _data;


        // Get / Set
        public bool IsProcessing => _processing;
        public bool IsPaused => _paused;
        public long ProcessingTime => _processingTime;

        public TimeManager()
        {
            _sessionStartTicks = DateTime.UtcNow.Ticks;
            _gameStartTicks = DateTime.UtcNow.Ticks;

            _data = new TimeManagerData();
        }
        
        public string CurrentTimeString()
        {
            return TimeUtils.TicksToString(CurrentTicks());
        }
        
        public GameTime CurrentTime()
        {
            long timeMs = CurrentTicks() / TimeSpan.TicksPerMillisecond;
            
            return new GameTime()
            {
                Milliseconds = timeMs
            };
        }

        // TODO: This functions should never return a real utcnow value. Always should send a current proccessed time. That is the real current time.
        public long CurrentTicks()
        {
            if (_paused)
            {
                return _pauseTime;
            }
            
            if (_processing)
            {
                return _processingTime;
            }
            
            return DateTime.UtcNow.Ticks - _gameStartTicks + _tickOffset;
        }

        public TimeManagerData GetSaveData()
        {
            _data.TickOffset = _tickOffset;
            _data.GameStartTicks = _gameStartTicks;
            _data.SaveTimeTicks = DateTime.UtcNow.Ticks;
            
            return _data;
        }

        public void InitializeAsNew()
        {
           // Do nothing
        }

        public void InitializeWithData(TimeManagerData data)
        {
            _data = data;
            
            _tickOffset = _data.TickOffset;
            _gameStartTicks = _data.GameStartTicks;
            _saveGameTicks = _data.SaveTimeTicks;
        }

        public void BeginProcessing(long end)
        {
            _processing = true;
        }

        public void EndProcessing()
        {
            _processing = false;
        }
        
        public void SetProcessingTime(long time)
        {
            _processingTime = time;
        }

        public long SaveTimeTicks()
        {
            return _data.SaveTimeTicks;
        }

        public void AddTimeMs(long time)
        {
            time = Math.Max(time, 0);
            
            _tickOffset += time * TimeSpan.TicksPerMillisecond;
        }

        public void Pause(bool enabled)
        {
            if (_paused != enabled)
            {
                if (enabled)
                {
                    _pauseTimeTick = DateTime.UtcNow.Ticks;
                    _pauseTime = _processingTime;
                }
                else
                {
                    _tickOffset -= DateTime.UtcNow.Ticks - _pauseTimeTick;
                }
                
                _paused = enabled;
            }
        }

        public void TogglePause()
        {
            Pause(!_paused);
        }
    }
}