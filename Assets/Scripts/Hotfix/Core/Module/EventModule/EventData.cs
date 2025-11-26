using System;

namespace HotfixCore.Module
{
    /// <summary>
    /// 事件分发参数
    /// </summary>
    public interface IEventParameter
    {
    }

    public interface IEventDataBase
    {
        int GetEvent();
        public string LogDebugInfo();
        public bool EventDataShouldBeInvoked();
        void UnSubscribeSelf();
    }

    public interface IEventData : IEventDataBase
    {
        Action Listener { get; }
        object Handle { get; }
    }

    public interface IEventData<in T1> : IEventDataBase where T1 : IEventParameter
    {
        Action<T1> Listener { get; }
        object Handle { get; }
    }

    public class EventData : IEventData
    {
        public int Event;
        public Action Listener;
        public object Handle;

        public EventData(int eventName, Action listener, object handle = null)
        {
            Event = eventName;
            Listener = listener;
            Handle = handle;
        }

        public int GetEvent()
        {
            return Event;
        }

        Action IEventData.Listener => Listener;
        object IEventData.Handle => Handle;

        public void UnSubscribeSelf()
        {
            G.EventModule.RemoveEventListener(this.Event, this.Listener, this.Handle);
        }

        public string LogDebugInfo()
        {
            return $"[{RuntimeId.ToString(Event)}] [{Listener.Method.Name}]";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            EventData other = (EventData)obj;
            return Event == other.Event && Listener.Equals(other.Listener);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Event.GetHashCode();
                hash = hash * 23 + Listener.GetHashCode();
                return hash;
            }
        }

        public bool EventDataShouldBeInvoked()
        {
            if (Handle == null || Handle.Equals(null))
            {
                return false;
            }

            return true;
        }
    }

    public class EventData<T1> : IEventData<T1> where T1 : IEventParameter
    {
        public int Event;
        public Action<T1> Listener;
        public object Handle;

        public EventData(int eventName, Action<T1> listener, object handle = null)
        {
            Event = eventName;
            Listener = listener;
            Handle = handle;
        }

        public int GetEvent()
        {
            return Event;
        }

        Action<T1> IEventData<T1>.Listener => Listener;
        object IEventData<T1>.Handle => Handle;

        public void UnSubscribeSelf()
        {
            G.EventModule.RemoveEventListener(this.Event, this.Listener, this.Handle);
        }

        public string LogDebugInfo()
        {
            return $"[{RuntimeId.ToString(Event)}] [{Listener.Method.Name}]";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            EventData<T1> other = (EventData<T1>)obj;
            return Event == other.Event && Listener.Equals(other.Listener);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Event.GetHashCode();
                hash = hash * 23 + Listener.GetHashCode();
                return hash;
            }
        }

        public bool EventDataShouldBeInvoked()
        {
            if (Handle == null || Handle.Equals(null))
            {
                return false;
            }

            return true;
        }
    }
}