namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.LoveLocks
{
    class LoveLockDialogueCloseMessageComposer : MessageComposer
    {
        public int ItemId { get; }

        public LoveLockDialogueCloseMessageComposer(int itemId)
            : base(ServerPacketHeader.LoveLockDialogueCloseMessageComposer)
        {
            ItemId = itemId;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(ItemId);
        }
    }
}
