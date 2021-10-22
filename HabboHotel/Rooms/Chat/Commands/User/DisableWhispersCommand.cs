namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class DisableWhispersCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_whispers";

        public string Parameters => "";

        public string Description => "Allows you to enable or disable the ability to receive whispers.";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            session.GetHabbo().ReceiveWhispers = !session.GetHabbo().ReceiveWhispers;
            session.SendWhisper("You're " + (session.GetHabbo().ReceiveWhispers ? "now" : "no longer") + " receiving whispers!");
        }
    }
}
