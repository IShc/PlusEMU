﻿using System.Linq;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms.PathFinding;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    class AllEyesOnMeCommand : IChatCommand
    {
        public string PermissionRequired => "command_alleyesonme";

        public string Parameters => "";

        public string Description => "Want some attention? Make everyone face you!";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            RoomUser thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
                return;

            List<RoomUser> users = room.GetRoomUserManager().GetRoomUsers();
            foreach (RoomUser u in users.ToList())
            {
                if (u == null || session.GetHabbo().Id == u.UserId)
                    continue;

                u.SetRot(Rotation.Calculate(u.X, u.Y, thisUser.X, thisUser.Y), false);
            }
        }
    }
}
