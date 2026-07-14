using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpStack.engine
{
    public class TimeEventsManager
    {
        float elapsedTime = 0.0f;
        private List<TimeEvent> timeEvents = new List<TimeEvent>();
        private Dictionary<int, TimeEvent> changableTimeEvents = new Dictionary<int, TimeEvent>();
        internal class TimeEvent
        {
            public float elapsedTime;
            public float TriggerTime { get; set; }
            public Action EventAction { get; set; }
            public TimeEvent(float triggerTime, Action eventAction)
            {
                TriggerTime = triggerTime;
                EventAction = eventAction;
            }
        }

        public void SortTimeEvents()
        {
            timeEvents = timeEvents.OrderBy(te => te.TriggerTime).ToList();
        }

        public void AddTimeEvent(float triggerTime, Action eventAction)
        {
            timeEvents.Add(new TimeEvent(triggerTime, eventAction));
            SortTimeEvents();
        }

        public void RemoveTimeEvent(Action eventAction)
        {
            timeEvents.RemoveAll(te => te.EventAction == eventAction);
        }

        public int AddChangableTimeEvent(float triggerTime, Action eventAction)
        {
            int id = changableTimeEvents.Count > 0 ? changableTimeEvents.Keys.Max() + 1 : 0;
            var timeEvent = new TimeEvent(triggerTime, eventAction);
            changableTimeEvents.TryAdd(id, timeEvent);
            
            return id;
        }

        public void RemoveChangableTimeEvent(int id)
        {
            if (changableTimeEvents.TryGetValue(id, out TimeEvent timeEvent))
            {
                changableTimeEvents.Remove(id);
            }
        }

        public void UpdateChangableTimeEventTriggerTime(int id, float newTriggerTime)
        {
            if (changableTimeEvents.TryGetValue(id, out TimeEvent timeEvent))
            {
                timeEvent.TriggerTime = newTriggerTime;
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var timeEvent in timeEvents.ToList())
            {
                timeEvent.elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timeEvent.elapsedTime > timeEvent.TriggerTime)
                {
                    timeEvent.EventAction.Invoke();
                    timeEvent.elapsedTime = 0.0f;
                }
            }

            foreach (var kvp in changableTimeEvents)
            {
                var timeEvent = kvp.Value;
                timeEvent.elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timeEvent.elapsedTime > timeEvent.TriggerTime)
                {
                    timeEvent.EventAction.Invoke();
                    timeEvent.elapsedTime = 0.0f;
                }
            }
        }
    }
}