using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    class ModeratorUserRoomVisitsComposer : MessageComposer
    {
        public Habbo Habbo { get; }
        public Dictionary<double, RoomData> Visits { get; }

        public ModeratorUserRoomVisitsComposer(Habbo data, Dictionary<double, RoomData> visits)
            : base(ServerPacketHeader.ModeratorUserRoomVisitsMessageComposer)
        {
            Habbo = data;
            Visits = visits;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Habbo.Id);
            packet.WriteString(Habbo.Username);
            packet.WriteInteger(Visits.Count);

            foreach (KeyValuePair<double, RoomData> visit in Visits)
            {
                packet.WriteInteger(visit.Value.Id);
                packet.WriteString(visit.Value.Name);
                packet.WriteInteger(UnixTimestamp.FromUnixTimestamp(visit.Key).Hour);
                packet.WriteInteger(UnixTimestamp.FromUnixTimestamp(visit.Key).Minute);
            }
        }
    }
}
