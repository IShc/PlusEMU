namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class GotoCommand : IChatCommand
    {
        public string PermissionRequired => "command_goto";

        public string Parameters => "%room_id%";

        public string Description => "";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("You must specify a room id!");
                return;
            }


            if (!int.TryParse(@params[1], out int roomId))
                session.SendWhisper("You must enter a valid room ID");
            else
            {
                RoomData data = null;
                if (!RoomFactory.TryGetData(roomId, out data))
                {
                    session.SendWhisper("This room does not exist!");
                    return;
                }
                else
                {
                    session.GetHabbo().PrepareRoom(roomId, "");
                }
            }
        }
    }
}