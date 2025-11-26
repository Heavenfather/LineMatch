using System;
using System.Collections.Generic;
using GameCore.Log;

namespace HotfixCore.Module
{
    public class EventModule : IMessageManager, IModuleDestroy
    {
        // 存储事件ID与事件处理器列表的字典
        private Dictionary<int, List<IEventDataBase>> events = new Dictionary<int, List<IEventDataBase>>(500);

        // 存储待删除的事件处理器列表
        private List<IEventDataBase> delects = new List<IEventDataBase>(50);

        // 用于检测死循环调用的调用栈
        private HashSet<IEventDataBase> callStack = new HashSet<IEventDataBase>();

        // 存储待触发的事件处理器列表
        private Dictionary<int, Queue<IEventDataBase>> dispatchInvokes = new Dictionary<int, Queue<IEventDataBase>>();

        // 输出消息死循环的函数
        private void MessageLoop(string debugInfo)
        {
            Logger.WarningFormat("消息被循环调用多次：{0}", debugInfo);
        }

        // 输出不存在事件处理函数的警告
        private void NotActionLog(int eventId, string actionName)
        {
            Logger.WarningFormat("函数不存在：【{0}】【{1}】", RuntimeId.ToString(eventId), actionName);
        }

        // 输出不存在监听者的警告
        private void NotListenerLog(string debugInfo)
        {
            Logger.WarningFormat("监听者不存在：{0}", debugInfo);
        }

        // 输出不存在事件的警告
        private void NotEventLogDispatch(int eventId)
        {
            Logger.DebugFormat("没有创建监听，发送事件：【{0}】", RuntimeId.ToString(eventId));
        }

        // 输出不存在事件的警告
        private void NotEventLogRemove(int eventId)
        {
            Logger.WarningFormat("没有创建监听，移除监听：【{0}】", RuntimeId.ToString(eventId));
        }

        // 清空调用栈
        private void ClearCallStack()
        {
            callStack.Clear();
        }

        // 判断事件是否在调用栈中
        private bool IsInCallStack(IEventDataBase eventData)
        {
            return callStack.Contains(eventData);
        }

        /// <summary>
        /// 添加事件监听器（不带参数）
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="listener"></param>
        /// <param name="handle"></param>
        public void AddEventListener(int eventId, Action listener, object handle)
        {
            // 创建事件数据对象
            IEventDataBase eventData = new EventData(eventId, listener, handle);

            // 检查是否存在相同的事件数据
            if (!events.ContainsKey(eventId))
            {
                events[eventId] = new List<IEventDataBase>(); // 如果不存在，则创建一个新列表
            }
            else
            {
                if (events[eventId].Contains(eventData))
                {
                    Logger.Error("不允许存在重复的事件处理函数。");
                    return;
                }
            }

            events[eventId].Add(eventData);
        }

        /// <summary>
        /// 添加带参事件监听
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="listener"></param>
        /// <param name="handle"></param>
        /// <typeparam name="T"></typeparam>
        public void AddEventListener<T>(int eventId, Action<T> listener, object handle)
            where T : struct, IEventParameter
        {
            IEventDataBase eventData = new EventData<T>(eventId, listener, handle);
            if (!events.ContainsKey(eventId))
            {
                events[eventId] = new List<IEventDataBase>();
            }
            else
            {
                if (events[eventId].Contains(eventData))
                {
                    Logger.Error("不允许存在重复的事件处理函数。");
                    return;
                }
            }

            events[eventId].Add(eventData);
        }
        
        /// <summary>
        /// 移除事件监听器（不带参数）
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="listener"></param>
        /// <param name="handle"></param>
        public void RemoveEventListener(int eventId, Action listener, object handle = null)
        {
            if (!events.ContainsKey(eventId))
            {
                NotEventLogRemove(eventId);
                return;
            }

            var eventList = events[eventId];
            if (eventList.Count == 0)
            {
                NotActionLog(eventId, listener.Method.Name);
                return;
            }

            delects.Clear();

            foreach (var itemObj in eventList)
            {
                if (itemObj is EventData eventData && eventData.Listener == listener && eventData.Handle == handle)
                {
                    eventData.Handle = null;
                    delects.Add(eventData);
                }
            }

            foreach (var deletion in delects)
            {
                eventList.Remove(deletion);
            }

            delects.Clear();
        }

        /// <summary>
        /// 移除带参事件监听
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="listener"></param>
        /// <param name="handle"></param>
        public void RemoveEventListener<T>(int eventId, Action<T> listener, object handle = null)
            where T : IEventParameter
        {
            if (!events.TryGetValue(eventId, out var eventList))
            {
                NotEventLogRemove(eventId);
                return;
            }

            if (eventList.Count == 0)
            {
                NotActionLog(eventId, listener.Method.Name);
                return;
            }

            delects.Clear();

            foreach (var itemObj in eventList)
            {
                if (itemObj is EventData<T> eventData && eventData.Listener == listener &&
                    eventData.Handle == handle)
                {
                    eventData.Handle = null;
                    delects.Add(itemObj);
                }
            }

            foreach (var deletion in delects)
            {
                eventList.Remove(deletion);
            }

            delects.Clear();
        }

        /// <summary>
        /// 移除事件所有监听
        /// </summary>
        /// <param name="eventId"></param>
        public void RemoveEventListener(int eventId)
        {
            if (events.ContainsKey(eventId))
            {
                if (events[eventId].Count > 0)
                {
                    events.Remove(eventId);
                }
            }
        }

        /// <summary>
        /// 分发事件（不带参数）
        /// </summary>
        /// <param name="eventId"></param>
        public void DispatchEvent(int eventId)
        {
            if (!events.TryGetValue(eventId, out List<IEventDataBase> eventDatas))
            {
                NotEventLogDispatch(eventId);
                return;
            }

            foreach (IEventDataBase obj in eventDatas)
            {
                if (IsInCallStack(obj))
                {
                    MessageLoop(obj.LogDebugInfo());
                    continue;
                }

                if (obj.EventDataShouldBeInvoked())
                {
                    if (!dispatchInvokes.ContainsKey(eventId))
                    {
                        dispatchInvokes[eventId] = new Queue<IEventDataBase>();
                    }

                    dispatchInvokes[eventId].Enqueue(obj);
                    callStack.Add(obj);
                }
                else
                {
                    NotListenerLog(obj.LogDebugInfo());
                    continue;
                }
            }

            while (dispatchInvokes.ContainsKey(eventId) && dispatchInvokes[eventId].Count > 0)
            {
                var obj = dispatchInvokes[eventId].Dequeue();
                if (obj is EventData eventData)
                {
                    eventData.Listener.Invoke();
                }
            }

            ClearCallStack(); // 清除调用栈
        }

        /// <summary>
        /// 分发事件（带参数）
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="arg1"></param>
        public void DispatchEvent<T>(int eventId, T arg1) where T : struct, IEventParameter
        {
            if (!events.TryGetValue(eventId, out List<IEventDataBase> eventDatas))
            {
                NotEventLogDispatch(eventId);
                return;
            }

            foreach (IEventDataBase obj in eventDatas)
            {
                if (IsInCallStack(obj))
                {
                    MessageLoop(obj.LogDebugInfo());
                    continue;
                }

                if (obj.EventDataShouldBeInvoked())
                {
                    if (!dispatchInvokes.ContainsKey(eventId))
                    {
                        dispatchInvokes[eventId] = new Queue<IEventDataBase>();
                    }

                    dispatchInvokes[eventId].Enqueue(obj);
                    callStack.Add(obj);
                }
                else
                {
                    NotListenerLog(obj.LogDebugInfo());
                    continue;
                }
            }

            while (dispatchInvokes.ContainsKey(eventId) && dispatchInvokes[eventId].Count > 0)
            {
                var obj = dispatchInvokes[eventId].Dequeue();
                if (obj is EventData<T> eventData1)
                {
                    eventData1.Listener.Invoke(arg1);
                }
            }

            ClearCallStack(); // 清除调用栈
        }

        // 清空事件管理器
        public void Clear()
        {
            events.Clear();
            delects.Clear();
            callStack.Clear();
            dispatchInvokes.Clear();
        }

        public void Destroy()
        {
            Clear();
        }
    }
}