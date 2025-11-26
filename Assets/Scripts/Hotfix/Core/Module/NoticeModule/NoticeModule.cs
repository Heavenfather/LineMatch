using System.Collections.Generic;
using GameCore.LitJson;
using GameCore.SDK;
using Hotfix.Define;
using Hotfix.Utils;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public class NoticeData {
        public int id;
        public int type;
        public string title;
        public string content;
        public int valid_from;
        public int valid_until;
        public int visible;
        public int closeable;
        public int sort;
    }

    public class NoticeModule : IModuleAwake, IModuleDestroy
    {
        private List<NoticeData> _loginNoticeList = new List<NoticeData>();
        public List<NoticeData> LoginNoticeList => _loginNoticeList;

        private List<NoticeData> _gameNoticeList = new List<NoticeData>();
        public List<NoticeData> GameNoticeList => _gameNoticeList;



        public void Awake(object parameter)
        {

        }


        public void Destroy()
        {
        }

        public void UpdateNoticeData() {
            if (CommonUtil.IsWechatMiniGame()) {

                Logger.Debug("UpdateNoticeData getNotice");
                SDKMgr.Instance.CallSDKMethod("getNotice","", "", returnData =>
                {
                    var code = returnData.Code;
                    if (code.CallbackCode > 0)
                    {
                        Logger.Debug("UpdateNoticeData returnData.Param = " + returnData.Param);
                        var data = JsonMapper.ToObject<List<Dictionary<string, object>>>(returnData.Param);
                        var noticeList = new List<NoticeData>();
                        for (int i = 0; i < data.Count; i++) {
                            var item = data[i];
                            var id = int.Parse(item["id"].ToString());
                            var type = int.Parse(item["type"].ToString());
                            var title = item["title"].ToString();
                            var content = item["content"].ToString();
                            var valid_from = int.Parse(item["valid_from"].ToString());
                            var valid_until = int.Parse(item["valid_until"].ToString());
                            var visible = int.Parse(item["visible"].ToString());
                            var closeable = int.Parse(item["closeable"].ToString());
                            var sort = int.Parse(item["sort"].ToString());
                            var notice = new NoticeData {
                                id = id,
                                type = type,
                                title = title,
                                content = content,
                                valid_from = valid_from,
                                valid_until = valid_until,
                                visible = visible,
                                closeable = closeable,
                                sort = sort
                            };
                            noticeList.Add(notice);
                        }
                        SetNoticeData(noticeList);
                    } else {
                        Logger.Error("获取公告失败：" + returnData.Code.ToString());
                    }
                });
            } else {
                // windows模拟数据
                var jsonStr = "[{\"id\":18,\"type\":1,\"title\":\"公告1\",\"content\":\"公告内容1111111111\",\"valid_from\":1756656000,\"valid_until\":1759248000,\"visible\":1,\"closeable\":1,\"sort\":0},{\"id\":19,\"type\":1,\"title\":\"公告2\",\"content\":\"公告内容22222222222\",\"valid_from\":1756656000,\"valid_until\":1759248000,\"visible\":1,\"closeable\":1,\"sort\":0},{\"id\":20,\"type\":2,\"title\":\"活动1\",\"content\":\"活动内容111111111111111111111111\",\"valid_from\":1756656000,\"valid_until\":1764518400,\"visible\":1,\"closeable\":1,\"sort\":0},{\"id\":21,\"type\":2,\"title\":\"活动2\",\"content\":\"活动内容2222222222222222222\",\"valid_from\":1756656000,\"valid_until\":1764518400,\"visible\":1,\"closeable\":1,\"sort\":0}]";
                SetNoticeData(JsonMapper.ToObject<List<NoticeData>>(jsonStr));
            }
        }

        public void SetNoticeData(List<NoticeData> noticeList) {
            _loginNoticeList.Clear();
            _gameNoticeList.Clear();

            foreach (var notice in noticeList) {
                if (notice.type == 1) {
                    _loginNoticeList.Add(notice);
                } else if (notice.type == 2) {
                    _gameNoticeList.Add(notice);
                }
            }

            G.EventModule.DispatchEvent(GameEventDefine.OnNoticeUpdateData);
        }

        public bool CheckNoticePop() {
            foreach (var notice in _loginNoticeList) {
                if (notice.visible == 1) return true;
            }

            foreach (var notice in _gameNoticeList) {
                if (notice.visible == 1) return true;
            }

            return false;
        }

        public NoticeData GetNoticeById(int id) {
            var data = _loginNoticeList.Find(x => x.id == id);
            if (data!= null) return data;

            data = _gameNoticeList.Find(x => x.id == id);
            if (data!= null) return data;

            return null;
        }

        public NoticeData GetLoginNoticeById(int id) {
            var data = _loginNoticeList.Find(x => x.id == id);
            if (data!= null) return data;

            return null;
        }

        public NoticeData GetGameNoticeById(int id) {
            var data = _gameNoticeList.Find(x => x.id == id);
            if (data!= null) return data;

            return null;
        }


    }
}
