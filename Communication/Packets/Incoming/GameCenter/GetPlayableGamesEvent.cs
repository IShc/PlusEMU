using Plus.Communication.Packets.Outgoing.GameCenter;

namespace Plus.Communication.Packets.Incoming.GameCenter
{
    class GetPlayableGamesEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient session, ClientPacket packet)
        {
            int gameId = packet.PopInt();

            session.SendPacket(new GameAccountStatusComposer(gameId));
            session.SendPacket(new PlayableGamesComposer(gameId));
            session.SendPacket(new GameAchievementListComposer(session, PlusEnvironment.GetGame().GetAchievementManager().GetGameAchievements(gameId), gameId));
        }
    }
}