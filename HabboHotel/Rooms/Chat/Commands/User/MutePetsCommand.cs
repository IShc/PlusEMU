﻿using Plus.Database.Interfaces;


namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class MutePetsCommand : IChatCommand
    {
        public string PermissionRequired => "command_mute_pets";

        public string Parameters => "";

        public string Description => "Ignore bot chat or enable it again.";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            session.GetHabbo().AllowPetSpeech = !session.GetHabbo().AllowPetSpeech;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `pets_muted` = '" + ((session.GetHabbo().AllowPetSpeech) ? 1 : 0) + "' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
            }

            if (session.GetHabbo().AllowPetSpeech)
                session.SendWhisper("Change successful, you can no longer see speech from pets.");
            else
                session.SendWhisper("Change successful, you can now see speech from pets.");
        }
    }
}
