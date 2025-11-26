using System.Collections.Generic;
using GameCore.Log;
using GameCore.SDK;
using Hotfix.Define;
using HotfixLogic.Match;

namespace HotfixCore.Module
{
    public class SwitchModule : IModuleAwake, IModuleDestroy
    {
        private Dictionary<string, object> _abDict;
        private Dictionary<string, object> _switchDict;

        public void Awake(object parameter)
        {

        }

        public void Destroy()
        {
            
        }

        public void InitABConfig(Dictionary<string, object> ABConfig) {
            _abDict = ABConfig;

            G.EventModule.DispatchEvent(GameEventDefine.OnABConfigInitFinish);
        }

        public void InitSwitchData(SwitchData switchData) {
            _switchDict = new Dictionary<string, object>();

            if (SDKMgr.Instance.GetDeviceSystemInfo().Platform == "ios") {
                if (switchData.ios != null) {
                    _switchDict = switchData.ios;
                }
            } else {
                if (switchData.android!= null) {
                    _switchDict = switchData.android;
                }
            }

            G.EventModule.DispatchEvent(GameEventDefine.OnSwitchDataInitFinish);
        }

        public int GetSwitchIntValue(string key) {
            if (_switchDict == null || !_switchDict.ContainsKey(key)) return 0;

            if (int.TryParse(_switchDict[key].ToString(), out int value)) {
                return value;
            }

            return 0;
        }

        public string GetSwitchStringValue(string key) {
            if (_switchDict == null || !_switchDict.ContainsKey(key)) return "";

            return (string)_switchDict[key];
        }

        public int GetABIntValue(string key) {
            if (_abDict == null || !_abDict.ContainsKey(key)) return 0;
            return (int)_abDict[key];
        }

        public string GetABStringValue(string key) {
            if (_abDict == null || !_abDict.ContainsKey(key)) return "";
            
            return (string)_abDict[key];
        }

        public MatchLevelType GetMatchABLevelType()
        {
            var levelType = GetABStringValue("stage_group").ToLower();
            if (string.IsNullOrEmpty(levelType))
                return MatchLevelType.B; //默认都是玩B关
            if (levelType == "a")
                return MatchLevelType.A;
            if(levelType == "c")
                return MatchLevelType.C;
            return MatchLevelType.B;
        }

        public bool IsOpenPlay() {
            if (_switchDict == null) return false;

            var lvOpen = GetSwitchIntValue("pay_switch");            
            return lvOpen <= MatchManager.Instance.MaxLevel;
        }

        public bool IsInitAbGroup() {
            return _abDict != null;
        }
    }
}
