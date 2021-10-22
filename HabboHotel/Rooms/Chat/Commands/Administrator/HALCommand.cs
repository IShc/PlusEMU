using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    class HalCommand : IChatCommand
    {
        public string PermissionRequired => "command_hal";

        public string Parameters => "%message%";

        public string Description => "Send a message to the entire hotel, with a link.";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 2)
            {
                session.SendWhisper("Please enter a message and a URL to send..");
                return;
            }

            string url = @params[1];

            string message = CommandManager.MergeParams(@params, 2);
            PlusEnvironment.GetGame().GetClientManager().SendPacket(new RoomNotificationComposer("Habboon Hotel Alert!", message + "\r\n" + "- " + session.GetHabbo().Username, "", url, url));
            return;
        }
    }
}
