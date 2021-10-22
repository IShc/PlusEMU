using System.Linq;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class RoomKickCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_kick";

        public string Parameters => "%message%";

        public string Description => "Kick the room and provide a message to the users.";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please provide a reason to the users for this room kick.");
                return;
            }

            string message = CommandManager.MergeParams(@params, 1);
            foreach (RoomUser roomUser in room.GetRoomUserManager().GetUserList().ToList())
            {
                if (roomUser == null || roomUser.IsBot || roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null || roomUser.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool") || roomUser.GetClient().GetHabbo().Id == session.GetHabbo().Id)
                    continue;

                roomUser.GetClient().SendNotification("You have been kicked by a moderator: " + message);

                room.GetRoomUserManager().RemoveUserFromRoom(roomUser.GetClient(), true, false);
            }

            session.SendWhisper("Successfully kicked all users from the room.");
        }
    }
}
