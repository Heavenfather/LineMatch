using System.Collections.Generic;
using GameConfig;
using GameCore.LitJson;
using GameCore.SDK;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixLogic;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public class HotFixPayInfo
    {
        public double price = 0.0;
        public string order_no = "";
        public string product_id = "";
        public string product_name = "";
        public string extra = "";
        public string notify_url = "";

        public override string ToString()
        {
            return string.Format("PayInfo[price:{0}, order_no:{1}, product_id:{2}, product_name:{3}, extra:{4}, notify_url:{5}]", (object) this.price, (object) this.order_no, (object) this.product_id, (object) this.product_name, (object) this.extra, (object) this.notify_url);
        }
    }

    public class GradeGiftData {
        public GradeGiftType type;
        public int[] shopID = new int[2];
        public int dailyCount;
        public long recordTime;
    }

    public class IAPModule : IModuleAwake, IModuleDestroy
    {
        private int _checkOrderCount;
        public Dictionary<string, string> _orderDict;
        public List<GradeGiftData> _gradeGiftList;
        public List<BuyItemData> _buyItemDatas;

        private int _monthlyPayAmount;

        public void Awake(object parameter)
        {
            InitGradeGiftList();
            InitOrderDict();
        }

        public void Destroy()
        {
            
        }

        // 创建订单
        public void CreateOrder(int shopID, int gift_id = 0, int popType = 0, bool isShop = false) {
            Logger.Info("Iap CreateOrder");

            int sceneID = 0;
            if (isShop) {
                sceneID = (int)IapSceneType.Shop;
            } else {
                if (G.SceneModule.CurSceneType == SceneType.Main) {
                    sceneID = (int)IapSceneType.Lobby;
                } else if (G.SceneModule.CurSceneType == SceneType.Match) {
                    sceneID = (int)IapSceneType.Match;
                } else if (G.SceneModule.CurSceneType == SceneType.Puzzle) {
                    sceneID = (int)IapSceneType.Puzzle;
                }
            }

            G.HttpModule.CreateOrder(shopID, sceneID, gift_id, popType);
        }


        // 调用SDK支付
        public void PayOrder(int shopID, string order_no, int scene_id = 0) {
            Logger.Info("Iap PayOrder");

            HotFixPayInfo payInfo = new HotFixPayInfo();


            var shopItem = ConfigMemoryPool.Get<ShopItemsDB>()[shopID];
            var dict = new Dictionary<string, object>();
            dict.Add("order_no", order_no);
            dict.Add("scene_id", scene_id);
            dict.Add("shop_id", shopID);
            dict.Add("order_time", CommonUtil.GetNowTime().ToString());

            string payExtra = JsonMapper.ToJson(dict);
            payInfo.order_no = order_no;
            payInfo.price = shopItem.price;
            payInfo.product_id = shopID.ToString();
            payInfo.product_name = shopItem.alias;
            payInfo.extra = payExtra;
            payInfo.notify_url = "";
            string jsonArgs = JsonMapper.ToJson(payInfo);
            Logger.Info($"Pay args : {jsonArgs}");

            SDKMgr.Instance.CallSDKMethod("pay", jsonArgs, "", returnData =>
            {
                var code = returnData.Code;
                Logger.Info($"Pay Callback Args: {code.CallBackData}");
                PayResult((string)code.CallBackData, code.ErrMsg);
            });

            Logger.Info("Iap PayOrder AddOrder");
            RecordOrder(order_no, payExtra);
        }

        // 支付结果回调
        public void PayResult(string payResult,string errorMsg) {
            Logger.Info("Iap  payResult：" + payResult);
            var jsonObj = JsonMapper.ToObject(payResult);
            int state = (int)jsonObj["pay_state"];
            if (state == 1)
            {
                Logger.Info("支付成功");
            } else if (state == 2 || state == 3) {
                // 取消支付/支付失败，删除缓存订单
                Dictionary<string, object> orderDetail = JsonMapper.ToObject<Dictionary<string, object>>((string)jsonObj["cp_extra"]);
                BuyProductFinish(orderDetail["order_no"].ToString());
            } else {
                Logger.Error("PayResult error: " + errorMsg);
            }
        }

        //检测订单缓存
        private void OrderStateQuery() {
            // Logger.Info("Iap 订单查询  orderDict：  " + orderDict.ToString());
            foreach (var order in _orderDict) {
                Dictionary<string, object> orderDetail = JsonMapper.ToObject<Dictionary<string, object>>(order.Value);
                G.HttpModule.OrderStateQuery(orderDetail["order_no"].ToString(), int.Parse(orderDetail["scene_id"].ToString()));
            }
        }

        // 记录订单
        public void RecordOrder(string order_no, string extra) {
            Logger.Info("Iap AddOrder order_no = " + order_no + " extra = " + extra);

            if (_orderDict.ContainsKey(order_no)) {
                _orderDict[order_no] = extra;
            } else {
                _orderDict.Add(order_no, extra);
            }

            _checkOrderCount = 0;

            SavePayOrder();
            CheckOrderCache();
        }

        // http支付完成，移除订单
        public void BuyProductFinish(string order_no) {
            Logger.Info("Iap RemoveOrder order_no = " + order_no + " _orderDict.Count = " + _orderDict.Count);

            if (_orderDict.ContainsKey(order_no)) {
                // TaskManager.Instance.TickTaskChanged();

                var orderData = _orderDict[order_no];
                _orderDict.Remove(order_no);
                Logger.Info("删除账单成功 order_no：" + order_no);

                var dict = JsonMapper.ToObject(orderData);
                int shopID = int.TryParse(dict["shop_id"].ToString() ,out int shopIDValue) ? shopIDValue : 0;
                G.EventModule.DispatchEvent(GameEventDefine.OnShopBuyProductFinish, EventOneParam<int>.Create(shopID));

                G.HttpModule.GetShopData();
            }

            SavePayOrder();

            CheckExpireOrder();
        }

        private void SavePayOrder() {
            var json = JsonMapper.ToJson(_orderDict);
            PlayerPrefsUtil.SetString("PayOrder", json);
        }

        private void InitOrderDict() {
            var json = PlayerPrefsUtil.GetString("PayOrder", "");
			if (json != "") {
				_orderDict = JsonMapper.ToObject<Dictionary<string, string>>(json);
			} else {
                _orderDict = new Dictionary<string, string>();
            }
        }


        // 检测订单缓存
        private void CheckOrderCache() {
            Logger.Info("Iap CheckOrderCache _checkOrderCount:  " + _checkOrderCount);
            _checkOrderCount++;
            if (_checkOrderCount > 40 || _orderDict.Count == 0) return;
            
            OrderStateQuery();

            // 递归2秒查询订单
			G.TimerModule.AddTimer(() => {
				CheckOrderCache();
			},2);
        }


        // 移除过期账单  24小时前的账单移除
        private void CheckExpireOrder() {
            List<string> removeList = new List<string>();
            foreach (var order in _orderDict) {
                // Dictionary<string, object> orderDetail = JsonMapper.ToObject<Dictionary<string, object>>(order.Value);
                // try {
                //     var orderTime = long.Parse(orderDetail["order_time"].ToString());
                    
                //     if (CommonUtil.GetNowTime() - orderTime > 86400) {
                //         removeList.Add(order.Key);
                //     }
                // } catch (Exception e) {
                //     Logger.Error("CheckExpireOrder error: " + e.Message);
                // }
                Logger.Info("CheckExpireOrder order = " + order.Value);
            }

            foreach (var order in removeList) {
                _orderDict.Remove(order);
            }
        }

        public string GetOrderData(string order_no) {
            if (_orderDict.ContainsKey(order_no)) {
                return _orderDict[order_no];
            }
            return null;
        }

        public GradeGiftData GetGradeGiftData(GradeGiftType giftType) {
            var data = _gradeGiftList.Find(item => item.type == giftType);
            if (data == null) {
                data = new GradeGiftData();
                data.type = giftType;
                data.shopID = GetGradeGiftDefultShopID(giftType);
                _gradeGiftList.Add(data);
            } else {
                var recordDate = CommonUtil.UnixToLocalDateTime(data.recordTime);
                var nowDate = CommonUtil.GetNowDateTime();
                if (recordDate.Month != nowDate.Month || recordDate.Day != nowDate.Day) {
                    data.dailyCount = 0;
                    data.recordTime = CommonUtil.GetNowTime();
                    data.shopID = GetGradeGiftDefultShopID(giftType);
                }
            }
            return data;
        }

        public int[] GetGradeGiftDefultShopID(GradeGiftType giftType) {
            var gradeGifts = ConfigMemoryPool.Get<GradeGiftDB>().GetGradeGiftsByType(giftType);
            if (gradeGifts == null || gradeGifts.Count < 2) {
                return null;
            }

            if (_monthlyPayAmount < 50) {
                return new int[] { gradeGifts[0].shopID, gradeGifts[1].shopID };
            } else if (_monthlyPayAmount > 200) {
                return new int[] { gradeGifts[gradeGifts.Count - 2].shopID, gradeGifts[gradeGifts.Count - 1].shopID };
            } else {
                var idx = gradeGifts.Count / 2;
                return new int[] { gradeGifts[idx].shopID, gradeGifts[idx + 1].shopID };
            }
        }

        public void SetMoonthlyAmount(int amount) {
            _monthlyPayAmount = amount;
        }

        public void AddGradeGiftCloseCount(GradeGiftType giftType) {
            var data = GetGradeGiftData(giftType);
            data.dailyCount++;
            data.recordTime = CommonUtil.GetNowTime();

            if (_monthlyPayAmount <= 200 && data.dailyCount >= 3 || _monthlyPayAmount > 200 && data.dailyCount >= 8) {
                data.dailyCount = 0;
                DownGradeGift(giftType);
            }

            WriteGradeGiftData();
        }

        public void BuyGradeGift(GradeGiftType giftType) {
            var data = GetGradeGiftData(giftType);
            data.dailyCount = 0;
            data.recordTime = CommonUtil.GetNowTime();

            UpgradeGift(giftType);
            WriteGradeGiftData();
        }

        private void DownGradeGift(GradeGiftType giftType) {
            var data = GetGradeGiftData(giftType);
            var gradeGifts = ConfigMemoryPool.Get<GradeGiftDB>().GetGradeGiftsByType(giftType);

            if (data.shopID[0] != gradeGifts[0].shopID) {
                for (int i = 0; i < gradeGifts.Count; i++) {
                    if (data.shopID[0] == gradeGifts[i].shopID) {
                        data.shopID[1] = data.shopID[0];
                        data.shopID[0] = gradeGifts[i - 1].shopID;
                        break;
                    }
                }
            }
        }

        private void UpgradeGift(GradeGiftType giftType) {
            var data = GetGradeGiftData(giftType);
            var gradeGifts = ConfigMemoryPool.Get<GradeGiftDB>().GetGradeGiftsByType(giftType);

            if (data.shopID[1] != gradeGifts[gradeGifts.Count - 1].shopID) {
                for (int i = 0; i < gradeGifts.Count; i++) {
                    if (data.shopID[1] == gradeGifts[i].shopID) {
                        data.shopID[0] = data.shopID[1];
                        data.shopID[1] = gradeGifts[i + 1].shopID;
                        break;
                    }
                }
            }

        }

        private void InitGradeGiftList() {
            var dataStr = PlayerPrefsUtil.GetString("GradeGiftData", "");
            if (dataStr != "") {
                _gradeGiftList = JsonMapper.ToObject<List<GradeGiftData>>(dataStr);
            } else {
                _gradeGiftList = new List<GradeGiftData>();
            }
        }

        private void WriteGradeGiftData() {
            var json = JsonMapper.ToJson(_gradeGiftList);
            PlayerPrefsUtil.SetString("GradeGiftData", json);
        }

        public void SetBuyItemData(List<BuyItemData> buyItemDatas) {
            _buyItemDatas = buyItemDatas;
            G.EventModule.DispatchEvent(GameEventDefine.OnShopUpdateProductData);
        }
        
        public BuyItemData GetBuyItemData(int id) {
            if (_buyItemDatas == null) {
                return null;
            }
            return _buyItemDatas.Find(item => item.id == id);
        }

        // 根据订单号显示奖励
        public void ShowShopReward(string order_no) {
            var orderData = G.IAPModule.GetOrderData(order_no);
            if (orderData != null) {
                var dict = JsonMapper.ToObject(orderData);
                int shopID = int.Parse(dict["shop_id"].ToString());


                if (G.TimerGiftModule.CheckIsMostFirstBuyGift(shopID)) {
                    // 首冲礼包需要特殊处理，额外获得奖励
                    G.TimerGiftModule.ShopBuyFirstBuyGiftAllReward();
                } else {
                    var shopItemDB = ConfigMemoryPool.Get<ShopItemsDB>()[shopID];
                    var itemDatas = CommonUtil.GetItemDatasByStr(shopItemDB.reward);
                    foreach (var item in itemDatas) {
                        if (item.Name.Contains("Buff")) {
                            item.Time = (int)CommonUtil.GetNowTime() + 60 * item.Count;
                        }
                    }
                    CommonUtil.ShowRewardWindow(itemDatas, shopItemDB.alias);
                    G.GameItemModule.AddItemCount(itemDatas);
                }
            }
        }
    }
}
