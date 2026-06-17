using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetros.engine
{
    public class TimeEventsManager
    {
        float elapsedTime = 0.0f;
        private List<TimeEvent> timeEvents = new List<TimeEvent>();
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
            

        }
    }
}