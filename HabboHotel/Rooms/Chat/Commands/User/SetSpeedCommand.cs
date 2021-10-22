namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class SetSpeedCommand : IChatCommand
    {
        public string PermissionRequired => "command_setspeed";

        public string Parameters => "%value%";

        public string Description => "Set the speed of the rollers in the current room.";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            if (!room.CheckRights(session, true))
                return;

            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter a value for the roller speed.");
                return;
            }

            int speed;
            if (int.TryParse(@params[1], out speed))
            {
                session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetSpeed(speed);
            }
            else
                session.SendWhisper("Invalid amount, please enter a valid number.");
        }
    }
}