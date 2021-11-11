using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Handshake
{
    public class UniqueIDEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            packet.PopString();
            string machineId = packet.PopString();

            session.MachineId = machineId;

            session.SendPacket(new SetUniqueIdComposer(machineId));
        }
    }
}