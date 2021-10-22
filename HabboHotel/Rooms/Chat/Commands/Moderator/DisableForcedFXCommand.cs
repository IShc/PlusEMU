using Plus.Database.Interfaces;


namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class DisableForcedFxCommand : IChatCommand
    {
        public string PermissionRequired => "command_forced_effects";

        public string Parameters => "";

        public string Description => "Gives you the ability to ignore or allow forced effects.";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            session.GetHabbo().DisableForcedEffects = !session.GetHabbo().DisableForcedEffects;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `disable_forced_effects` = @DisableForcedEffects WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("DisableForcedEffects", (session.GetHabbo().DisableForcedEffects == true ? 1 : 0).ToString());
                dbClient.RunQuery();
            }

            session.SendWhisper("Forced FX mode is now " + (session.GetHabbo().DisableForcedEffects == true ? "disabled!" : "enabled!"));
        }
    }
}
