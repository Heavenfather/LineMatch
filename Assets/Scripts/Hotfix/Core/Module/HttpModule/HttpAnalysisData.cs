using System;
using System.Collections;
using System.Collections.Generic;
using GameCore.LitJson;
using UnityEngine;

#region 登录回调数据
namespace HotfixCore.Module
{
    public class RoleServerInfo
    {
        public string server_id = "";
        public string server_name = "";
        public string role_id = "";
        public string role_name = "";
        public int role_level = 0;
        public int vip_level = 0;
    }
    
    public class LoginResponseData
    {
        // 用户数据
        public LoginUserData user;
        // // 道具数据
        // public List<ServerItem> items;
        // // 关卡数据
        // public StageData stage;
        public string access_token;
        // public SwitchData func_switch ;
        // public Dictionary<string, object> ab_config ;
        // public PuzzleMapInfo puzzle;
        // public List<int> card_collect;
        public int daily_sign_state = 0;
    }

    public class LoginUserData 
    {
        public int user_id;
        public string open_id;
        public string pf_openid;
        public string avatar;
        public string avatar_frame;
        public string nickname;
        public string nickname_color;
        public int create_time;
        public int last_login_time;
        public int login_times;
        public int continue_login_days;
        public int total_login_days;
        public int last_pay_time;
        public int total_pay_money;
        public int max_win_streak;
        public int max_ranking;
        public int once_pass_times;
        public string medal;
        public string invite_code;
    }
    #endregion



    #region 各种功能进度数据 
    public class GameProgressData
    {
        public int active_id;
    }
    #endregion



    #region 道具数据
    public class ItemListData
    {
        public List<ServerItem> items;
    }

    public class ServerItem
    {
        public int item_id;
        public int item_num;
        public int reward_num;
        public int change_num;
        public int item_expend_time;
        public int item_expire_time;
    }
    #endregion



    #region 订单数据
    public class OrderData
    {
        public string order_no;
        public int pay_money;
    }

    // 订单状态数据
    public class OrderStateData {
        public string order_no;
        public int order_state;
    }
    #endregion


    #region 奖励发放数据
    public class RewardData
    {
        public int reward_id;
        public List<ServerItem> rewards;
        public int get_reward_time;
        public int get_reward_num;
    }
    #endregion


    public class ServerShopData {
        public int register_time;
        public int pay_money;
        public List<BuyItemData> product_list;
    }

    public class BuyItemData {
        public int id;
        public int limit_type;
        public int limit_num;
        public int buy_count;
    }


    #region 商城兑换数据
    public class ShopExchangeData
    {
        public int exchange_id;
        public List<ServerItem> rewards;
        public CoinData coins;
        public int get_reward_time;
        public int get_reward_num;
    }

    public class CoinData {
        public int coin_num;
        public int consume_coin;
    }
    #endregion


    #region GM道具修改
    public class GMItem {
        public int item_id;
        public int item_num;
        public int change_num;
    }
    #endregion



    #region 关卡数据

    public class ServerStageData {
        public StageData stage;
    }

    public class StageData {
        public int stage_id;
        public int win_streak;
        public StageSetting setting;
        public TargetTask objective;
        public string stage_group;
        public int win_streak_rank_score;
        public bool is_coin_stage;
    }

    public class StageSetting {
        public int stage_id;
        public int stage_val;
        public int stage_play_cnt;
        public StageDetail stage_val_detail;

    }
    public class StageDetail {
        public int behavior_val;
        public int group_val;
    }

    public class StageEndData {
        public int stage_id;
        public List<ServerItem> items;
        public StageSetting setting;
        public TargetTask objective;
        public int is_once_pass;
        public bool is_coin_stage;
    }
    #endregion

    #region 排行榜数据
    public class WorldRankingData {
        public int self_rank;
        public List<WorldRankData> ranks;
    }

    public class WinStreakRankingData {
        public int self_rank;
        public int start_tim;
        public int end_tim;
        public int last_week_rank;
        public int reward_state;
        public List<WinStreakRankData> ranks;
    }

    public class WorldRankData {
        public int rank;
        public int user_id;
        public string avatar;
        public string nickname;
        public string avatar_frame;
        public int stage_id;
        public string medal;
        public string nickname_color;
    }

    public class WinStreakRankData {
        public int rank;
        public int user_id;
        public string avatar;
        public string nickname;
        public string avatar_frame;
        public int win_streak;
        public string medal;
        public string nickname_color;
    }
    #endregion


    #region Team数据
    public class CreateTeamData {
        public TeamInfo team;
        public List<ServerItem> items;
    }

    public class TeamInfo {
        public int id;
        public string name;
        public string avatar;
        public string description;
        public int limit_level;
        public int leader;
        public int state;
        public int score;
        public int member_limit = 0;
        public int member_count = 0;
        public List<TeamMemberData> members;
    }

    public class TeamMemberData {
        public int user_id;
        public string nickname;
        public string avatar;
        public int stage_id;
    }

    #endregion

    public class SwitchData {
        public Dictionary<string, object> ios;
        public Dictionary<string, object> android;
    }

    public class ServerSwitchData {
        public SwitchData func_switch;
    }


    public class PuzzleMapState {
        public int map_id;
        public int level_id;
        public int status;
        public int collect;
    }

    public class ServerPuzzleData {
        public PuzzleMapInfo stage;
    }


    public class PuzzleMapInfo {
        public int map_id;
        public List<PuzzleMapState> list;
    }

    public class PuzzleRewardConfig {
        public List<PuzzleRewardData> datas;
    }

    public class PuzzleRewardData {
        public int id;
        public int mapId;
        public string levelId;
        public string openCoin;
        public string openStar;
        public string reward;
    }

    public class PuzzleUploadReward {
        public string map_id;
        public Dictionary<string, int> reward;
    }

    public class TargetTaskQueryData {
        public TargetTask objective;
    }

    public class TargetTask {
      public int total_num;
      public int start_time;
      public int reward_id;
      public int reward_start_num;
      public int reward_end_num;
      public long end_time;
      public List<int> wait_reward_ids;
      public int objective_type;
      public int reward_type;
    }

    public class ActivityReward {
        // public List<int> reward_id;
        public string type;
        public List<ServerItem> rewards;
    }

    public class AdvData {
        public int reward_id;
        public int daily_limit;
        public int get_reward_time;
        public int get_reward_num;
    }

    #region 集卡数据
    public class CardResponseData {
        public List<CardData> cards;
        public StarRewardRecord star;
        public List<RewardRecord> pack;
        public int theme_id;
        public int collect_card_total;
        public int reward_id;
        public int start_time;
        public int end_time;
        public int share_limit;
    }

    public class CollectCardData {
        public List<int> card_collect;
    }

    public class UsePackData {
        public List<CardData> rewards;
    }
    public class CardRewardData {
        public string type;
        public List<ServerItem> rewards;
    }

    public class CardData {
        public int card_id;
        public int card_num;
        public int card_star;
    }

    public class RewardRecord {
        public int reward_id;
        public int reward_state;
    }

    public class StarRewardRecord {
        public int star_num;
        public List<RewardRecord> reward_list;
    }

    public class ShareCardData {
        public string share_code;
    }

    public class ReceiveCardData {
        public CardData reward;
    }
    
    #endregion

    #region 签到

    public class SignGetRewardData
    {
        public List<string> reward_id;
        public string type;
        public List<ServerItem> rewards;
    }

    #endregion


    public class ServerLiveData {
        public List<LevelItem> list;
        public int stage_id;
        public int star_stage;
        public int end_stage;
    }

    public class LevelItem {
        public int stage_id;
        public int star_num;
    }


    public class ServerMonthCardData {
        public int reward_id;
        public int state;
        public int last_get_time;
        public int expire_time;
    }
    public class ServerMonthCardReward {
        public List<string> reward_id;
        public string type;
        public List<ServerItem> rewards;
    }

    public class ServerTrainMaster {
        public ServerTrainMasterData master_trial;
    }

    public class ServerTrainMasterReward {
        public List<ServerItem> rewards;
    }

    public class ServerTrainMasterData {
        public int total_coin;
        public int start_time;
        public int end_time;
        public int coin_reward;
        public int win_streak_num;
        public int state;
        public int start_stage_id;
        public int win_streak;
        
    }

    public class ServerEmailData {
        public List<ServerEmailItem> list;
    }

    public class ServerEmailReward {
        public Dictionary<string, ServerItem> rewards;
    }

    public class ServerEmailItem {
        public int email_id;
        public string title;
        public string content;
        public DateTime send_time;
        public DateTime deadline;
        public int state;
        public int is_read;
        public int is_taken;
        public List<ServerItem> item;
    }

    public class ServerInviteData {
        public int invite_cnt;
        public List<ServerInviteReward> reward_list;
        public List<ServerInvitePlayer> invite_fail_list;
    }

    public class ServerInviteReward {
        public int id;
        public int invite_num;
        public int state;
    }

    public class ServerInvitePlayer {
        public int user_id;
        public string nickname;
        public string nickname_color;
        public string avatar;
        public string avatar_frame;
        public string wx_nickname;
        public string wx_avatar;
        public string medal;
        public int show_cnt;
    }

    #region 任务

    public class ServerTaskData
    {
        public int create_time;
        public int task_type;
        public List<ServerTaskItem> daily_task;
        public Dictionary<string,List<ServerTaskItem>> seven_day_task;
    }

    public class ServerTaskItem
    {
        public int id;
        public int tag;
        public int num;
        public string reward;
        public int tag_num;
        public int state;
    }

    public class ServerActiveTaskData
    {
        public int create_time;
        public int task_type;
        public int engagement_num;
        public List<ServerActiveTaskItem> reward_list;
    }
    
    public class ServerActiveTaskItem
    {
        public int id;
        public int num;
        public int state;
        public int taskType;
        public string reward;
    }

    public class ServerTreasureData
    {
        public int start_time;
        public int end_time;
        public List<int> group_type;
        public int reward_id;
    }

    #endregion


    public class ServerItemDatas {
        public List<ServerItem> items;
    }

    public class ServerQuestData {
        public int theme_id;
        public int state;
        public int start_time;
        public int end_time;
        public List<ServerQuestItem> question;
        public List<ServerItem> rewards;
    }

    public class ServerQuestItem {
        public int id;
    }

    public class ServerABConfigData {
        public Dictionary<string, object> ab_config ;
    }

}