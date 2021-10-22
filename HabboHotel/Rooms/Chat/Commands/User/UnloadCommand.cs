namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class UnloadCommand : IChatCommand
    {
        public string PermissionRequired => "command_unload";

        public string Parameters => "%id%";

        public string Description => "Unload the current room.";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            if (room.CheckRights(session, true) || session.GetHabbo().GetPermissions().HasRight("room_unload_any"))
            {
                PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);
            }
        }
    }
}
