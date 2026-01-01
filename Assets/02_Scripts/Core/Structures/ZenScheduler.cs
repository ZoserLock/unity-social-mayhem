using System;
using System.Collections.Generic;

namespace StrangeSpace
{
    public interface IZenScheduler
    {
        IScheduleHandle Schedule(TimeSpan interval, Action handler, int priority = 0);
        void Tick();
        void Clear();
    }

    public interface IScheduleHandle
    {
        void Cancel();
    }

    public class ZenScheduler : IZenScheduler
    {
        private readonly List<ScheduleItem> _scheduledItems;
        private readonly List<ScheduleItem> _itemsToExecute;
        private readonly List<ScheduleItem> _itemsToRemove;

        public ZenScheduler()
        {
            _scheduledItems = new List<ScheduleItem>();
            _itemsToExecute = new List<ScheduleItem>();
            _itemsToRemove = new List<ScheduleItem>();
        }

        public IScheduleHandle Schedule(TimeSpan interval, Action handler, int priority = 0)
        {
            var item = new ScheduleItem
            {
                Interval = interval,
                Handler = handler,
                NextExecutionTime = DateTime.UtcNow + interval,
                Priority = priority
            };

            _scheduledItems.Add(item);
            return item;
        }

        public void Tick()
        {
            var now = DateTime.UtcNow;
            
            _itemsToExecute.Clear();
            _itemsToRemove.Clear();
            
            for (int i = 0; i < _scheduledItems.Count; i++)
            {
                var item = _scheduledItems[i];
                
                if (item.IsCancelled)
                {
                    _itemsToRemove.Add(item);
                    continue;
                }
                
                if (now >= item.NextExecutionTime)
                {
                    _itemsToExecute.Add(item);
                }
            }
            
            if (_itemsToExecute.Count > 1)
            {
                SortByPriority(_itemsToExecute);
            }
            
            for (int i = 0; i < _itemsToExecute.Count; i++)
            {
                var item = _itemsToExecute[i];
                
                if (item.IsCancelled)
                {
                    if (!_itemsToRemove.Contains(item))
                    {
                        _itemsToRemove.Add(item);
                    }
                    continue;
                }
                
                item.Handler();
                item.NextExecutionTime = now + item.Interval;
            }
            
            for (int i = 0; i < _itemsToRemove.Count; i++)
            {
                _scheduledItems.Remove(_itemsToRemove[i]);
            }
        }
        
        private void SortByPriority(List<ScheduleItem> items)
        {
            items.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        public void Clear()
        {
            _scheduledItems.Clear();
            _itemsToExecute.Clear();
            _itemsToRemove.Clear();
        }

        private class ScheduleItem : IScheduleHandle
        {
            public TimeSpan Interval { get; set; }
            public Action Handler { get; set; }
            public DateTime NextExecutionTime { get; set; }
            public int Priority { get; set; }
            public bool IsCancelled { get; private set; }

            public void Cancel()
            {
                IsCancelled = true;
            }
        }
    }
}