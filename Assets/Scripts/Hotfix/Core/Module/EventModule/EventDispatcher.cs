using System;
using System.Collections.Generic;
using HotfixCore.MemoryPool;

namespace HotfixCore.Module
{
    /// <summary>
    /// 事件派发与监听
    /// </summary>
    public class EventDispatcher : IMemory
    {
        private static EventDispatcher _globalDispatcher;

        public static EventDispatcher GlobalDispatcher => _globalDispatcher ??= new EventDispatcher();

        //事件id对应的所有监听事件
        private Dictionary<int, HashSet<IEventDataBase>> events = new Dictionary<int, HashSet<IEventDataBase>>();

        // 存储待删除的事件处理器列表
        private List<IEventDataBase> delects = new List<IEventDataBase>();

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="eventId">事件唯一Id</param>
        /// <param name="listener">监听回调</param>
        /// <param name="handle">事件拥有者</param>
        public void AddEventListener(int eventId, Action listener, object handle = null)
        {
            if (!events.ContainsKey(eventId))
            {
                events[eventId] = new HashSet<IEventDataBase>();
            }

            EventData eventData = new EventData(eventId, listener, handle);
            events[eventId].Add(eventData);

            G.EventModule.AddEventListener(eventId, listener, handle);
        }

        /// <summary>
        /// 添加事件监听（带参）
        /// </summary>
        /// <param name="eventId">事件唯一Id</param>
        /// <param name="listener">带参数监听回调</param>
        /// <param name="handle">事件拥有者</param>
        public void AddEventListener<T>(int eventId, Action<T> listener, object handle = null)
            where T : struct, IEventParameter
        {
            if (!events.ContainsKey(eventId))
            {
                events[eventId] = new HashSet<IEventDataBase>();
            }

            EventData<T> eventData = new EventData<T>(eventId, listener, handle);
            events[eventId].Add(eventData);

            G.EventModule.AddEventListener<T>(eventId, listener, handle);
        }

        /// <summary>
        /// 移除指定监听事件
        /// </summary>
        /// <param name="eventId">事件唯一Id</param>
        public void RemoveEventListener(int eventId)
        {
            if (events.ContainsKey(eventId))
            {
                if (events[eventId].Count > 0)
                {
                    HashSet<IEventDataBase> ebs = events[eventId];
                    if (ebs.Count < 0)
                    {
                        return;
                    }

                    delects.Clear();

                    foreach (var item in ebs)
                    {
                        if (item is EventData eb)
                        {
                            item.UnSubscribeSelf();
                            delects.Add(eb);
                        }
                    }

                    foreach (var deletion in delects)
                    {
                        ebs.Remove(deletion);
                    }

                    delects.Clear();
                }
            }
        }

        /// <summary>
        /// 移除指定带参数监听事件
        /// </summary>
        /// <param name="eventId">事件唯一Id</param>
        public void RemoveEventListener<T>(int eventId)
            where T : IEventParameter
        {
            if (events.ContainsKey(eventId))
            {
                if (events[eventId].Count > 0)
                {
                    HashSet<IEventDataBase> ebs = events[eventId];
                    if (ebs.Count < 0)
                    {
                        return;
                    }

                    delects.Clear();

                    foreach (var item in ebs)
                    {
                        if (item is EventData<T> eb)
                        {
                            item.UnSubscribeSelf();
                            delects.Add(eb);
                        }
                    }

                    foreach (var deletion in delects)
                    {
                        ebs.Remove(deletion);
                    }

                    delects.Clear();
                }
            }
        }

        /// <summary>
        /// 删除此事件所有监听（慎用）
        /// </summary>
        public void ClearAllEventListener(int eventId)
        {
            if (events.ContainsKey(eventId))
            {
                if (events[eventId].Count > 0)
                {
                    events.Remove(eventId);
                    G.EventModule.RemoveEventListener(eventId);
                }
            }
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="eventId"></param>
        public void DispatchEvent(int eventId)
        {
            G.EventModule.DispatchEvent(eventId);
        }

        /// <summary>
        /// 派发事件 带参数
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="arg1"></param>
        public void DispatchEvent<T>(int eventId, T arg1) where T : struct, IEventParameter
        {
            G.EventModule.DispatchEvent(eventId, arg1);
        }

        public void Clear()
        {
            foreach (var eventSet in events.Values)
            {
                foreach (IEventDataBase eventData in eventSet)
                {
                    eventData.UnSubscribeSelf();
                }
            }

            events.Clear();
            delects.Clear();
        }
    }
}