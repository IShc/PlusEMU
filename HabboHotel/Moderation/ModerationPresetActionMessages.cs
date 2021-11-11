namespace Plus.HabboHotel.Moderation
{
    public class ModerationPresetActionMessages
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Caption { get; set; }
        public string MessageText { get; set; }
        public int MuteTime { get; set; }
        public int BanTime { get; set; }
        public int IpBanTime { get; set; }
        public int TradeLockTime { get; set; }
        public string Notice { get; set; }

        public ModerationPresetActionMessages(int id, int parentId, string caption, string messageText, int muteTime, int banTime, int ipBanTime, int tradeLockTime, string notice)
        {
            this.Id = id;
            this.ParentId = parentId;
            this.Caption = caption;
            this.MessageText = messageText;
            this.MuteTime = muteTime;
            this.BanTime = banTime;
            this.IpBanTime = ipBanTime;
            this.TradeLockTime = tradeLockTime;
            this.Notice = notice;
        }
    }
}
