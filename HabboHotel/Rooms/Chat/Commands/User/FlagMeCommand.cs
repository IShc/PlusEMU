using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Handshake;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class FlagMeCommand : IChatCommand
    {
        public string PermissionRequired => "command_flagme";

        public string Parameters => "";

        public string Description => "Gives you the option to change your username.";

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            if (!CanChangeName(session.GetHabbo()))
            {
                session.SendWhisper("Sorry, it seems you currently do not have the option to change your username!");
                return;
            }

            session.GetHabbo().ChangingName = true;
            session.SendNotification("Please be aware that if your username is deemed as inappropriate, you will be banned without question.\r\rAlso note that Staff will NOT change your username again should you have an issue with what you have chosen.\r\rClose this window and click yourself to begin choosing a new username!");
            session.SendPacket(new UserObjectComposer(session.GetHabbo()));
        }

        private bool CanChangeName(Habbo habbo)
        {
            if (habbo.Rank == 1 && habbo.VIPRank == 0 && habbo.LastNameChange == 0)
                return true;
            else if (habbo.Rank == 1 && habbo.VIPRank == 1 && (habbo.LastNameChange == 0 || (PlusEnvironment.GetUnixTimestamp() + 604800) > habbo.LastNameChange))
                return true;
            else if (habbo.Rank == 1 && habbo.VIPRank == 2 && (habbo.LastNameChange == 0 || (PlusEnvironment.GetUnixTimestamp() + 86400) > habbo.LastNameChange))
                return true;
            else if (habbo.Rank == 1 && habbo.VIPRank == 3)
                return true;
            else if (habbo.GetPermissions().HasRight("mod_tool"))
                return true;

            return false;
        }
    }
}
