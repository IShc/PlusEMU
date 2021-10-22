namespace Plus.Communication.Packets.Outgoing.Rooms.Permissions
{
    class YouAreControllerComposer : MessageComposer
    {
        public int Setting { get; }
        public YouAreControllerComposer(int setting)
            : base(ServerPacketHeader.YouAreControllerMessageComposer)
        {
            Setting = setting;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Setting);
        }
    }
}
