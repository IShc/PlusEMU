using Plus.Database.Interfaces;



namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class DisableMimicCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_mimic";

        public string Parameters => "";

        public string Description => "Allows you to disable the ability to be mimiced or to enable the ability to be mimiced.";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            session.GetHabbo().AllowMimic = !session.GetHabbo().AllowMimic;
            session.SendWhisper("You're " + (session.GetHabbo().AllowMimic == true ? "now" : "no longer") + " able to be mimiced.");

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `allow_mimic` = @AllowMimic WHERE `id` = '" + session.GetHabbo().Id + "'");
                dbClient.AddParameter("AllowMimic", PlusEnvironment.BoolToEnum(session.GetHabbo().AllowMimic));
                dbClient.RunQuery();
            }
        }
    }
}