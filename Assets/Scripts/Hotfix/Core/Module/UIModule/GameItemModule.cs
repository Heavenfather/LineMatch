using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public class GameItemModule : IModuleAwake, IModuleDestroy
    {
        const string itemSpritePath = "uisprites/item/";
        const string headTexturePath = "uitexture/head/";
        const string frameTexturePath = "uitexture/frame/";
        private Dictionary<string, ItemData> _itemData = new Dictionary<string, ItemData>();
        private ItemEnumDB _itemEnumDB;

        private Dictionary<string, int> _buffData = new Dictionary<string, int>();

        private Dictionary<string, string> _buffDict = new Dictionary<string, string>{
            {"live", "liveBuff"},
            {"eliminateRock", "eliminateRockBuff"},
            {"eliminateBoom", "eliminateBoomBuff"},
            {"eliminateBall", "eliminateBallBuff"},
            {"eliminateDice", "eliminateDiceBuff"},
            {"eliminateHammer", "eliminateHammerBuff"},
            {"eliminateArrow", "eliminateArrowBuff"},
            {"eliminateBullet", "eliminateBulletBuff"},
            {"eliminateColored", "eliminateColoredBuff"},
            {"eliminateDyeBuff", "eliminateDyeBuffBuff"},
        };

        private Dictionary<int, string> _nameLineDict = new Dictionary<int, string> {
            {1502, "#cb8643"},
            {1503, "#b53f36"},
            {1504, "#61438c"},
        };

        private Dictionary<int, string[]> _colorDict = new Dictionary<int, string[]> {
            {1502, new string[]{"#ffc257", "#ffe26e"}},
            {1503, new string[]{"#ff7061", "#ff8686"}},
            {1504, new string[]{"#72a7ff", "#e49dff"}},
        };

        private Dictionary<int, Color> _nameColorDict = new Dictionary<int, Color> {
            {1501, Color.white},
            {1502, new Color(201f/255f, 170f/255f, 98f/255f)},
            {1503, new Color(84f/255f, 151f/255f, 104f/255f)},
            {1504, new Color(204f/255f, 92f/255f, 94f/255f)},
        };

        // 体力使用的时间
        private int _liveUseTime = 0;
        private int _liveTimer = 0;
        private int _buffTimer = 0;

        public void Awake(object parameter)
        {
            _itemEnumDB = ConfigMemoryPool.Get<ItemEnumDB>();
            InitBuffDict();
        }

        public void Destroy()
        {
            _itemData.Clear();
        }

        private void InitBuffDict() {
            foreach (var item in _itemEnumDB.All) {
                if (item.name.Contains("Buff")) {
                    _buffData.Add(item.name, 0);
                }
            }
        }

        // 获取道具数量
        public int GetItemCount(string itemName)
        {
            if (_itemEnumDB.IsNoneKey(itemName)) return 0;

            if (!_itemData.ContainsKey(itemName)) {
                _itemData.Add(itemName, new ItemData(itemName, 0));
            }
            return _itemData[itemName].Count;
        }

        public int GetItemCount(int itemId)
        {
            var item = _itemEnumDB[itemId];
            return GetItemCount(item.name);
        }

        public ItemData GetItemData(string itemName) {
            if (_itemEnumDB.IsNoneKey(itemName)) return null;

            if (!_itemData.ContainsKey(itemName)) {
                _itemData.Add(itemName, new ItemData(itemName, 0));
            }
            return _itemData[itemName];
        }

        public ItemData GetItemData(int itemId) {
            if (_itemEnumDB.IsNoneKey(itemId)) return null;

            var item = _itemEnumDB[itemId];
            return GetItemData(item.name);
        }

        public int SetItemCount(ItemData item, bool fireEvent = true) {
            if (_itemEnumDB.IsNoneKey(item.Name)) {
                return 0;
            }

            if (!_itemData.ContainsKey(item.Name)) {
                _itemData.Add(item.Name, item);
            }
            _itemData[item.Name] = item;

            // 如果是buff，添加buff时间
            UpdateBuffTime(item);

            CheckLiveUse(item);
            CheckBuffUse(item);

            if (fireEvent)G.EventModule.DispatchEvent(GameEventDefine.OnUpdteGameItem);
            return _itemData[item.Name].Count;
        }

        public int SetItemCount(string itemName, int count, bool fireEvent = true) {
            var itemData = GetItemData(itemName);
            itemData.Count = count;
            return SetItemCount(itemData, fireEvent);
        }

        public int SetItemCount(int itemId, int count, bool fireEvent = true)
        {
            var itemData = GetItemData(itemId);
            itemData.Count = count;
            return SetItemCount(itemData, fireEvent);
        }

        public void SetItemCount(List<ItemData> items) {
            foreach (var item in items) {
                SetItemCount(item, false);
            }
            G.EventModule.DispatchEvent(GameEventDefine.OnUpdteGameItem);
        }


        // 增加道具数量
        public int AddItemCount(ItemData item, bool fireEvent = true) {
            var itemData = GetItemData(item.Name);
            itemData.Count += item.Count;

            return SetItemCount(itemData,fireEvent);
        }
        public int AddItemCount(string itemName, int count = 1, bool fireEvent = true) {
            var itemData = GetItemData(itemName);
            itemData.Count += count;

            return SetItemCount(itemData,fireEvent);
        }
        public int AddItemCount(int itemId, int count = 1, bool fireEvent = true) {
            var itemData = GetItemData(itemId);
            itemData.Count += count;

            return SetItemCount(itemData,fireEvent);
        }
        
        public void AddItemCount(List<ItemData> items) {
            foreach (var item in items) {
                AddItemCount(item, false);
            }
            G.EventModule.DispatchEvent(GameEventDefine.OnUpdteGameItem);
        }

        public async UniTask<Sprite> GetItemSprite(string itemName) {
            return await G.ResourceModule.LoadAssetAsync<Sprite>(GetItemPath(itemName));
        }

        public async UniTask<Sprite> GetItemSprite(int itemID) {
            var item = _itemEnumDB[itemID];
            return await GetItemSprite(item.name);
        }

        public void SetItemSprite(int itemID, Action<Sprite> callback) {
            var item = _itemEnumDB[itemID];
            SetItemSprite(item.name, callback);
        }

        public void SetItemSprite(int itemId, Image img, bool isNative = false)
        {
            var item = _itemEnumDB[itemId];
            SetItemSprite(item.name, (sp) =>
            {
                img.sprite = sp;
                if (isNative)
                    img.SetNativeSize();
            });
        }

        public void SetItemSprite(string itemName, Action<Sprite> callback) {
            G.ResourceModule.LoadAssetAsync(GetItemPath(itemName), callback).Forget();
        }

        public string GetItemPath(string itemName) {
            string path = "";
            if (itemName != "liveBuff") {
                itemName = itemName.Replace("Buff", "");
            }

            if (itemName.Contains("Head")) {
                path = headTexturePath + itemName;
            } else if (itemName.Contains("Frame")) {
                path = frameTexturePath + itemName;
            } else {
                path = itemSpritePath + itemName;
            }

            return path;
        }

        // 开始游戏消耗体力
        public void GameConsumLive() {
            if (!CheckHasBuff("liveBuff")) {
                AddItemCount("live", -ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("LiveConsum"));
            }
        }

        public void UseItem(int itemId, int count = 1, Action<bool> callback = null, bool isUseBuff = true) {
            var item = _itemEnumDB[itemId];
            UseItem(item.name, count, callback, isUseBuff);
        }

        public void UseItem(string itemName, int count = 1, Action<bool> callback = null, bool isUseBuff = true) {
            if (itemName.Contains("Buff") && CheckHasBuff(itemName)) {
                callback?.Invoke(true);
            } else {
                if (isUseBuff) {
                    string buffName = _buffDict.ContainsKey(itemName) ? _buffDict[itemName] : "";
                    if (buffName != "" && CheckHasBuff(buffName)) {
                        callback?.Invoke(true);
                        return;
                    }
                }

                G.HttpModule.UseItem(new ItemData(_itemEnumDB[itemName].Id, count), (result, code) => {
                    callback?.Invoke(code == 0);
                });
            }
        }

        public void UseItem(List<ItemData> items, Action<List<int>> callback = null, bool isUseBuff = true) {
            List<int> cbItemIDs = new List<int>();

            List<ItemData> requestItems = new List<ItemData>();
            foreach (var item in items) {
                var itemName = item.Name;
                if (itemName.Contains("Buff") && CheckHasBuff(itemName)) {
                    cbItemIDs.Add(item.Id);
                } else {
                    if (isUseBuff) {
                        if (CheckHasBuffByItemName(itemName)) {
                            cbItemIDs.Add(item.Id);
                        } else {
                            requestItems.Add(item);
                        }
                    } else {
                        requestItems.Add(item);
                    }
                }
            }

            if (requestItems.Count > 0) {
                G.HttpModule.UseItem(requestItems, (result, code) => {
                    if (code == 0) {
                        foreach (var item in requestItems) {
                            cbItemIDs.Add(item.Id);
                        }
                    }

                    callback?.Invoke(cbItemIDs);
                });
            } else {
                callback?.Invoke(cbItemIDs);
            }
        }


        public bool CheckHasBuffByItemName(string itemName) {
            if(string.IsNullOrEmpty(itemName))
                return false;
            if (itemName.Contains("Buff")) {
                return CheckHasBuff(itemName);
            } else {
                string buffName = _buffDict.ContainsKey(itemName) ? _buffDict[itemName] : "";
                return CheckHasBuff(buffName);
            }
        }

        public bool CheckHasBuffByItemId(int itemId) {
            string itemName = GetItemName(itemId);
            return CheckHasBuffByItemName(itemName);
        }

        public bool CheckHasBuff(string buffName) {
            if (!_buffData.ContainsKey(buffName)) return false;

            var time = _buffData[buffName];
            if (time == 0 || (int)CommonUtil.GetNowTime() > time) {
                _buffData[buffName] = 0;
                return false;
            }

            return true;
        }

        public int GetBuffExpireTime(string buffName) {
            if (!_buffData.ContainsKey(buffName)) return 0;

            return _buffData[buffName];
        }

        private void CheckLiveUse(ItemData item) {
            if (item.Name == "live") {
                if (item.Count < 100) {
                    if (item.Time != 0) {
                        SetLiveUseTime(item.Time);    
                    } else {
                        SetLiveUseTime((int)CommonUtil.GetNowTime());   
                    }
                }
                UpdateLiveTimer();
            }
        }

        private void CheckBuffUse(ItemData item) {
            if (_buffData.ContainsKey(item.Name)) {
                if (item.Time != 0 && CommonUtil.GetNowTime() < item.Time) {
                    _buffData[item.Name] = item.Time;

                    UpdateBuffTimer();
                }
            }
        }

        private void SetLiveUseTime(int time) {
            _liveUseTime = time;
            GetItemData("live").Time = time;
        }

        private void UpdateLiveTimer() {
            if (GetItemCount("live") >= 100) {
                if (_liveTimer != 0) {
                    G.TimerModule.RemoveTimer(_liveTimer);
                    G.EventModule.DispatchEvent(GameEventDefine.OnUpdateLiveTime, EventOneParam<int>.Create(0));
                }
                _liveUseTime = 0;
            } else {
                if (_liveTimer == 0 && _liveUseTime != 0) {
                    _liveTimer = G.TimerModule.AddTimer(() => {
                        int timeout = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("LiveRestorTime") * 60;
                        int lastTime = timeout - ((int)CommonUtil.GetNowTime() - _liveUseTime);
                        if (lastTime <= 0) {
                            SetLiveUseTime((int)CommonUtil.GetNowTime());                            
                            AddItemCount("live", 1);
                        }

                        G.EventModule.DispatchEvent(GameEventDefine.OnUpdateLiveTime, EventOneParam<int>.Create(lastTime));
                    }, 1.0f, true);
                }
            }
        }

        private void UpdateBuffTimer() {
            if (_buffTimer != 0) return;

            var hasBuff = false;    

            // 将键转换为List
            List<string> keys = new List<string>(_buffData.Keys);
            foreach (string key in keys) {
                if (CheckHasBuff(key)) {
                    hasBuff = true;
                    break;
                }
            }

            if (!hasBuff) return;

            _buffTimer = G.TimerModule.AddTimer(() => {
                var hasTimeOut = false;
                foreach (string key in keys) {
                    var buffExpireTime = _buffData[key];
                    if (buffExpireTime != 0) {
                        hasTimeOut = true;
                        var lastTime = buffExpireTime - (int)CommonUtil.GetNowTime();
                        
                        if (lastTime <= 0) {
                            lastTime = 0;
                            _buffData[key] = 0;
                        }

                        G.EventModule.DispatchEvent(GameEventDefine.OnUpdateBuffTime, EventTwoParam<string, int>.Create(key, lastTime));
                    }
                }     

                if (!hasTimeOut) {
                    G.TimerModule.RemoveTimer(_buffTimer);
                    _buffTimer = 0;
                }           
            }, 1.0f, true);
        }


        // 更新buff时间
        private void UpdateBuffTime(ItemData item) {
            if (_buffData.ContainsKey(item.Name) && item.Count > 0) {
                // 判断是否到期，到期置零
                if (item.Time == 0 || (int)CommonUtil.GetNowTime() > item.Time) {
                    _buffData[item.Name] = (int)CommonUtil.GetNowTime() + item.Count * 60;
                } else {
                    _buffData[item.Name] += item.Count * 60;
                }

                item.Count = 0;
                item.Time = _buffData[item.Name];
            }
        }

        public int GetItemID(string itemName) {
            if (_itemEnumDB.IsNoneKey(itemName)) return 0;

            return _itemEnumDB[itemName].Id;
        }

        public string GetItemName(int itemId) {
            if (_itemEnumDB.IsNoneKey(itemId)) return "";

            return _itemEnumDB[itemId].name;
        }

        public List<ItemData> GetPackItems() {
            List<ItemData> items = new List<ItemData>();
            for (int i = 1301; i <= 1305; i++) {
                var itemData = GetItemData(i);
                if (itemData != null && itemData.Count > 0) {
                    items.Add(itemData);
                }
            }
            return items;
        }

        public Color GetNameColor(int nameItemId) {
            if (_nameColorDict.ContainsKey(nameItemId)) {
                return _nameColorDict[nameItemId];
            } else {
                return Color.white;
            }
        }

        public void SetNameColor(int nameItemId, TextMeshProUGUI text) {
            var colorGradient = new VertexGradient();

            var topColor = Color.white;
            var bottomColor = Color.white;

            if (_colorDict.ContainsKey(nameItemId)) {
                var colorStr = _colorDict[nameItemId];

                bottomColor = ColorUtility.TryParseHtmlString(colorStr[0], out Color color0) ? color0 : Color.white;
                topColor = ColorUtility.TryParseHtmlString(colorStr[1], out Color color1) ? color1 : Color.white;

                text.color = Color.white;

                text.outlineColor = ColorUtility.TryParseHtmlString(_nameLineDict[nameItemId], out Color outLineColor) ? outLineColor : Color.white;
                text.outlineWidth = 0.3f;
            } else {
                text.outlineColor = Color.white;
                text.outlineWidth = 0;
            }

            colorGradient.topLeft = topColor;
            colorGradient.topRight = topColor;
            colorGradient.bottomLeft = bottomColor;
            colorGradient.bottomRight = bottomColor;

            text.colorGradient = colorGradient;
        }
    }


    public class ItemData {
        public int Id;

        public string Name;

        public int Count;
        public int Time;

        public ItemData(int id, int count) {
            var item = ConfigMemoryPool.Get<ItemEnumDB>()[id];
            Id = item.Id;
            Count = count;
            Name = item.name;
        }

        public ItemData(string name, int count) {
            var item = ConfigMemoryPool.Get<ItemEnumDB>()[name];
            Id = item.Id;
            Count = count;
            Name = item.name;
        }

        // 格式 live*3
        public ItemData(string itemStr) {
            try
            {
                var infos = itemStr.Split("*");
                var item = ConfigMemoryPool.Get<ItemEnumDB>()[infos[0]];

                Id = item.Id;
                Count = int.Parse(infos[1]);
                Name = item.name;
            }
            catch (Exception e)
            {
                Logger.Error("ItemData parse error: " + e.Message);
            }
        }
    }

}
