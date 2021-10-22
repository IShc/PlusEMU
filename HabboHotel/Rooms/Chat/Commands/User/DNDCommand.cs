namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class DndCommand : IChatCommand
    {
        public string PermissionRequired => "command_dnd";

        public string Parameters => "";

        public string Description => "Allows you to chose the option to enable or disable console messages.";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            session.GetHabbo().AllowConsoleMessages = !session.GetHabbo().AllowConsoleMessages;
            session.SendWhisper("You're " + (session.GetHabbo().AllowConsoleMessages == true ? "now" : "no longer") + " accepting console messages.");
        }
    }
}