using System;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    class CarryCommand : IChatCommand
    {
        public string PermissionRequired => "command_carry";

        public string Parameters => "%ItemId%";

        public string Description => "Allows you to carry a hand item";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            int itemId = 0;
            if (!int.TryParse(Convert.ToString(@params[1]), out itemId))
            {
                session.SendWhisper("Please enter a valid integer.");
                return;
            }

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            user.CarryItem(itemId);
        }
    }
}
