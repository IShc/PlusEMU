namespace Plus.Communication.Packets.Outgoing.Rooms.Session
{
    class CantConnectComposer : MessageComposer
    {
        public int Error { get; }
        public CantConnectComposer(int error)
            : base(ServerPacketHeader.CantConnectMessageComposer)
        {
            Error = error;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Error);
        }
    }
}
