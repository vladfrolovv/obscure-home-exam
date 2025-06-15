using System;
using UnityEngine.EventSystems;
namespace OGClient.Utils
{
    public static class EventTriggerUtils
    {

        public static void EventTriggerSubscription(this EventTrigger eventTrigger, EventTriggerType type, Action callback)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = type,
            };

            entry.callback.AddListener(_ => callback?.Invoke());
            eventTrigger.triggers.Add(entry);
        }

    }
}
