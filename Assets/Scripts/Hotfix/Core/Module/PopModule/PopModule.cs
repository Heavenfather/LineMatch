using System;
using System.Collections.Generic;
using GameConfig;
using GameCore.Utils;
using Hotfix.Utils;

namespace HotfixCore.Module
{
    public class PopData
    {
        public string popKey;
        public Action onPop;

        public PopData(string popKey, Action onPop)
        {
            this.popKey = popKey;
            this.onPop = onPop;
        }
    }

    public class PopModule : IModuleAwake, IModuleDestroy
    {
        Queue<PopData> _popQueue = new Queue<PopData>();

        PopData _curPopData;

        MainPageType _curMainPageType;

        public void Awake(object parameter)
        {
        }

        public void Destroy()
        {
        }

        public void InsertPopWindow(string popKey, Action onPop)
        {

            var newQueue = new Queue<PopData>();
            newQueue.Enqueue(new PopData(popKey, onPop));

            while (_popQueue.Count > 0)
            {
                var popData = _popQueue.Dequeue();
                if (popData.popKey == popKey)
                {
                    newQueue.Enqueue(popData);
                }
            }

            while (newQueue.Count > 0)
            {
                _popQueue.Enqueue(newQueue.Dequeue());
            }
        }

        public void AddPopWindow(string popKey, Action onPop)
        {
            _popQueue.Enqueue(new PopData(popKey, onPop));
        }

        public void ClearPopWindow()
        {
            _popQueue.Clear();
        }

        public void ShowPop()
        {
			if (G.SceneModule.CurSceneType != SceneType.Main || _curMainPageType != MainPageType.Lobby) {
				return;
			}

            if (_popQueue.Count > 0)
            {
                PopData popData = _popQueue.Dequeue();
                popData.onPop();

                _curPopData = popData;
            } else {
                _curPopData = null;
            }
        }

        public void CheckPopClose(string popKey) {
            if (_curPopData!= null && popKey.Contains(_curPopData.popKey))
            {
                _curPopData = null;
                ShowPop();
            }
        }

        public bool CheckDailyIsPop(string popKey) {
            var key = "DailyPop_" + popKey;
            var value = PlayerPrefsUtil.GetInt(key, -1);
            var dateNow = CommonUtil.GetNowDateTime();
            return value == -1 || dateNow.Day != value;
        }

        public void RecordDailyIsPop(string popKey) {
            var key = "DailyPop_" + popKey;
            var dateNow = CommonUtil.GetNowDateTime();
            PlayerPrefsUtil.SetInt(key, dateNow.Day);
        }

        public void SetCurMainPageType(MainPageType mainPageType) {
            _curMainPageType = mainPageType;
        }

        public bool HasPopWindow() {
            return _popQueue.Count > 0;
        }
    }
}
