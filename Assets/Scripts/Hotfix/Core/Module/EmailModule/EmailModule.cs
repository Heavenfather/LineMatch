using System.Collections.Generic;
using GameCore.Localization;
using Hotfix.Define;
using Hotfix.Utils;
using HotfixLogic;

namespace HotfixCore.Module
{
    public class EmailModule : IModuleAwake, IModuleDestroy
    {
        private List<ServerEmailItem> _emailList = new List<ServerEmailItem>();
        public List<ServerEmailItem> EmailList => _emailList;


        public List<ServerEmailItem> _rewardEmailList = new List<ServerEmailItem>();
        public List<ServerEmailItem> RewardEmailList => _rewardEmailList;

        public List<ServerEmailItem> _readEmailList = new List<ServerEmailItem>();
        public List<ServerEmailItem> ReadEmailList => _readEmailList;

        public void Awake(object parameter)
        {
        }

        public void Destroy()
        {
        }

        public void SetEmailList(List<ServerEmailItem> emailList) {
            if (emailList == null) {
                return;
            }
            _emailList = emailList;

            _rewardEmailList.Clear();
            _readEmailList.Clear();
            for (int i = 0; i < _emailList.Count; i++) {
                if (_emailList[i].is_taken == 1) {
                    _rewardEmailList.Add(_emailList[i]);
                }

                if (_emailList[i].is_read == 0) {
                    _readEmailList.Add(_emailList[i]);
                }
            }

            CheckEmailRedDot();

            G.EventModule.DispatchEvent(GameEventDefine.OnEamilUpdateData);
        }

        public bool IsEmptyEmail() {
            return _emailList == null || _emailList.Count == 0;
        }

        public int GetEmailCount() {
            if (_emailList == null) return 0;
            return _emailList.Count;
        }

        public ServerEmailItem GetEmailByEmailId(int emailId) {
            if (_emailList == null) return null;

            var email = _emailList.Find(item => item.email_id == emailId);
            return email;
        }

        // 更新邮件奖励状态
        public void UpdateEmailRewardState(int emailId) {
            if (_emailList == null) return;

            var email = _emailList.Find(item => item.email_id == emailId);
            if (email == null) return;

            email.is_taken = 2;

            RemoveRewardEmail(emailId);
        }

        // 更新已读状态
        public void UpdateEmailIsRead(int emailId) {
            if (_emailList == null) return;

            var email = _emailList.Find(item => item.email_id == emailId);
            if (email == null) return;

            email.is_read = 1;
            RemoveReadEmail(emailId);
        }

        private void RemoveReadEmail(int emailId) {
            if (_readEmailList == null) return;
            _readEmailList.Remove(_readEmailList.Find(item => item.email_id == emailId));

            CheckEmailRedDot();
        }

        private void RemoveRewardEmail(int emailId) {
            if (_rewardEmailList == null) return;
            _rewardEmailList.Remove(_rewardEmailList.Find(item => item.email_id == emailId));
            RemoveReadEmail(emailId);

            CheckEmailRedDot();
        }

        private void RemoveEmail(int emailId) {
            if (_emailList == null) return;
            _emailList.Remove(_emailList.Find(item => item.email_id == emailId));

            RemoveReadEmail(emailId);
            RemoveRewardEmail(emailId);
        }

        private void CheckEmailRedDot() {
            var redotCount = (_rewardEmailList.Count > 0 || _readEmailList.Count > 0) ? 1 : 0;
            G.RedDotModule.SetRedDotCount(RedDotDefine.Email, redotCount );
        }



        // 删除所有邮件
        public void DeleteAllEmail() {
            if (_emailList == null) return;

            var delEmailList = new List<int>();

            var delEmailStr = "";
            for (int i = 0; i < _emailList.Count; i++) {
                var emailData = _emailList[i];

                if (emailData.is_taken == 2 || emailData.is_taken == 0 && emailData.is_read == 1) {
                    if (delEmailStr.Length > 0) {
                        delEmailStr += "|";
                    }
                    delEmailStr += _emailList[i].email_id;

                    delEmailList.Add(emailData.email_id);
                }
            }

            if (delEmailList.Count == 0) {
                CommonUtil.ShowCommonTips(LocalizationPool.Get("Email/DelEmailFail"));
                return;
            }

            G.HttpModule.DeleteEmail(delEmailStr, succ => {
                if (succ) {
                    
                    CommonUtil.ShowCommonTips(LocalizationPool.Get("Email/DelFinish"));
                    for (int i = 0; i < delEmailList.Count; i++) {
                        RemoveEmail(delEmailList[i]);
                    }

                    G.EventModule.DispatchEvent(GameEventDefine.OnEamilUpdateData);
                }
            });
        }

        // 一键领奖
        public void GetAllReward() {
            if (_emailList == null || _rewardEmailList.Count <= 0) {
                CommonUtil.ShowCommonTips(LocalizationPool.Get("Email/NorRewardEmail"));
                return;
            }

            var rewardEmailList = new List<int>();

            var rewardEmailStr = "";
            for (int i = 0; i < _rewardEmailList.Count; i++) {
                var emailData = _rewardEmailList[i];

                if (rewardEmailStr.Length > 0) {
                    rewardEmailStr += "|";
                }
                rewardEmailStr += emailData.email_id;
            }

            G.HttpModule.GetEmailReward(rewardEmailStr, succ => {
                if (succ) {
                    G.EventModule.DispatchEvent(GameEventDefine.OnEamilUpdateData);
                }
            });
        }


    }
}
