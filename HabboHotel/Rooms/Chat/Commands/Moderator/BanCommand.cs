using System;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;

using Plus.HabboHotel.Moderation;

using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class BanCommand : IChatCommand
    {

        public string PermissionRequired => "command_ban";

        public string Parameters => "%username% %length% %reason% ";

        public string Description => "Remove a toxic player from the hotel for a fixed amount of time.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you'd like to IP ban & account ban.");
                return;
            }

            Habbo habbo = PlusEnvironment.GetHabboByUsername(@params[1]);
            if (habbo == null)
            {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_soft_ban") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                session.SendWhisper("Oops, you cannot ban that user.");
                return;
            }

            double expire = 0;
            string hours = @params[2];
            if (String.IsNullOrEmpty(hours) || hours == "perm")
                expire = PlusEnvironment.GetUnixTimestamp() + 78892200;
            else
                expire = (PlusEnvironment.GetUnixTimestamp() + (Convert.ToDouble(hours) * 3600));

            string reason = null;
            if (@params.Length >= 4)
                reason = CommandManager.MergeParams(@params, 3);
            else
                reason = "No reason specified.";

            string username = habbo.Username;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + habbo.Id + "' LIMIT 1");
            }

            PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, reason, expire);

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (targetClient != null)
                targetClient.Disconnect();

            session.SendWhisper("Success, you have account banned the user '" + username + "' for " + hours + " hour(s) with the reason '" + reason + "'!");
        }
    }
}