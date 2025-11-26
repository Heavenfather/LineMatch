using System;

namespace HotfixCore.Module
{
    public interface IMessageManager
    {
        void AddEventListener(int eventId, Action listener, object handle = null);

        void AddEventListener<T>(int eventId, Action<T> listener, object handle = null)
            where T : struct, IEventParameter;

        void RemoveEventListener(int eventId, Action listener, object handle = null);

        void RemoveEventListener<T>(int eventId, Action<T> listener, object handle = null)
            where T : IEventParameter;

        void DispatchEvent(int eventId);

        void DispatchEvent<T>(int eventId, T arg1) where T : struct, IEventParameter;

        void Clear();
    }
}