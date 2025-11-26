using System;
using System.Collections.Generic;
using GameCore.LitJson;
using GameCore.Singleton;
using Hotfix.Define;
using Hotfix.Utils;
using HotfixCore.Module;
using HotfixLogic.Match;

namespace HotfixLogic
{
    /// <summary>
    /// 任务类型，需要前端统计数量进行上报
    /// </summary>
    public enum TaskTag : int
    {
        /// <summary>
        /// 生成火箭
        /// </summary>
        GenRocket = 13,

        /// <summary>
        /// 生成炸弹
        /// </summary>
        GenBomb = 14,

        /// <summary>
        /// 生成彩球
        /// </summary>
        GenColorBall = 15,

        /// <summary>
        /// 使用火箭和炸弹
        /// </summary>
        UseBombAndRocket = 16,

        /// <summary>
        /// 使用炸弹和炸弹
        /// </summary>
        UseBombAndBomb = 17,

        /// <summary>
        /// 使用彩球和火箭
        /// </summary>
        UseColorBallAndRocket = 18,

        /// <summary>
        /// 使用火箭和火箭
        /// </summary>
        UseRocketAndRocket = 19,

        /// <summary>
        /// 使用彩球和炸弹
        /// </summary>
        UseColorBallAndBomb = 20,

        /// <summary>
        /// 使用彩球和彩球
        /// </summary>
        UseDoubleColorBall = 21,
    }

    public struct TaskPopData
    {
        public int TaskId;

        public int OldValue;
        
        public int NewValue;
    }
    
    public class TaskManager : LazySingleton<TaskManager>
    {
        private int _currentPlayLevel;
        private bool _ticking = false;
        private Dictionary<int, int> _matchLevelCalTask;
        private List<TaskPopData> _taskPopDatas;

        protected override void OnInitialized()
        {
            G.EventModule.AddEventListener(GameEventDefine.OnUpdteGameItem, OnUpdateGameItem, this);

            _ticking = false;
            _matchLevelCalTask = new Dictionary<int, int>(10);
            _taskPopDatas = new List<TaskPopData>(100);
        }

        public void SetCurrentPlayLevel(int levelId)
        {
            _currentPlayLevel = levelId;
        }

        public void AddTaskCalculate(TaskTag tag)
        {
            if (!IsCanAddTaskNum())
                return;
            if (_matchLevelCalTask.ContainsKey((int)tag))
            {
                _matchLevelCalTask[(int)tag]++;
            }
            else
            {
                _matchLevelCalTask.Add((int)tag, 1);
            }
        }

        public void TickTaskChanged()
        {
            if (_ticking)
                return;
            _ticking = true;
            // int redCnt = G.RedDotModule.GetRedDotCount(RedDotDefine.Task);
            // if (redCnt > 0)
            // {
            //     //已经有红点，没必要继续检测
            //     _ticking = false;
            //     return;
            // }

            G.HttpModule.ReqTaskList(data =>
            {
                _ticking = false;
                SetTaskRed(data);
            });
        }

        public void GetTaskIconCanShow(Action<bool, bool> callback)
        {
            G.HttpModule.ReqTaskList(data =>
            {
                if (data == null)
                {
                    callback(false, false);
                    return;
                }

                _taskPopDatas?.Clear();
                if (data.seven_day_task != null && data.seven_day_task.Count > 0)
                {
                    foreach (var kp in data.seven_day_task)
                    {
                        int day = int.Parse(kp.Key);
                        if(IsSevenTaskIsLock(day))
                            continue;
                        for (int i = 0; i < kp.Value.Count; i++)
                        {
                            var task = kp.Value[i];
                            AddOldPopTask(task);
                        }
                    }

                    if (IsSevenDayTimeout())
                        callback(false, false);
                    else
                        callback(true, false);
                }
                else if (data.daily_task != null && data.daily_task.Count > 0)
                {
                    for (int i = 0; i < data.daily_task.Count; i++)
                    {
                        AddOldPopTask(data.daily_task[i]);
                    }
                    callback(false, true);
                }
                else
                {
                    callback(false, false);
                }
                SetTaskRed(data);
            });
        }

        public bool IsSevenDayTimeout()
        {
            int createTime = G.UserInfoModule.CreateTime;

            var dateTime = CommonUtil.UnixToLocalDateTime(createTime);
            dateTime = dateTime.Date;

            createTime = (int)CommonUtil.LocalDateTimeToUnix(dateTime);

            long sevenTimeTick = createTime + 7 * 86400;
            var now = CommonUtil.GetNowTime();
            return now > sevenTimeTick;
        }

        public bool IsSevenTaskIsLock(int day)
        {
            int createTime = G.UserInfoModule.CreateTime;

            var dateTime = CommonUtil.UnixToLocalDateTime(createTime);
            dateTime = dateTime.Date;

            var beginTime = (int)CommonUtil.LocalDateTimeToUnix(dateTime);

            long dayTick = beginTime + ((day - 1) * 86400);
            long now = CommonUtil.GetNowTime();
            return now < dayTick;
        }
        
        private void SetTaskRed(ServerTaskData data)
        {
            if (data != null)
            {
                bool havaCanGetReward = false;
                if (data.task_type == 1)
                {
                    //七日任务
                    if (data.seven_day_task != null)
                    {
                        foreach (var (day, tasks) in data.seven_day_task)
                        {
                            int dayInt = int.Parse(day);
                            if (IsSevenTaskIsLock(dayInt))
                                continue;
                            for (int i = 0; i < tasks.Count; i++)
                            {
                                if (tasks[i].state == 1)
                                {
                                    havaCanGetReward = true;
                                }
                                UpdateTaskPopData(tasks[i]);
                            }
                        }
                    }
                }
                else if (data.task_type == 8)
                {
                    //每日任务
                    if (data.daily_task != null)
                    {
                        for (int i = 0; i < data.daily_task.Count; i++)
                        {
                            if (data.daily_task[i].state == 1)
                            {
                                havaCanGetReward = true;
                            }
                            UpdateTaskPopData(data.daily_task[i]);
                        }
                    }
                }

                if (havaCanGetReward)
                {
                    G.RedDotModule.SetRedDotCount(RedDotDefine.Task, 1);
                }
            }
        }
        
        private void AddOldPopTask(ServerTaskItem task)
        {
            if(task.state != 0)
                return;
            _taskPopDatas.Add(new TaskPopData
            {
                TaskId = task.id,
                OldValue = task.num,
                NewValue = 0
            });
        }

        private void UpdateTaskPopData(ServerTaskItem task)
        {
            if(task.state == 2)
                return;
            var index = _taskPopDatas.FindIndex(x => x.TaskId == task.id);
            if(index < 0)
                return;
            var data = _taskPopDatas[index];
            bool showPop = false;
            if (task.state == 1)
            {
                showPop = true;
                data.NewValue = task.num;
            }
            else if (task.state == 0)
            {
                data.OldValue = task.num;
            }
            _taskPopDatas[index] = data;
            if (showPop)
            {
                _taskPopDatas.RemoveAt(index);
            }
        }
        
        public string GetCalculateTaskJson()
        {
            if (_matchLevelCalTask == null)
                return "";
            string json = JsonMapper.ToJson(_matchLevelCalTask);
            return json;
        }

        public void ClearTaskCalculate()
        {
            _matchLevelCalTask.Clear();
        }

        private void OnUpdateGameItem()
        {
            TickTaskChanged();
        }

        private bool IsCanAddTaskNum()
        {
            int maxLevel = MatchManager.Instance.MaxLevel;
            return _currentPlayLevel == maxLevel;
        }
    }
}