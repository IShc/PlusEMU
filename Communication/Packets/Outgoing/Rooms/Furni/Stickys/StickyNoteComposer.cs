namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Stickys
{
    internal class StickyNoteComposer : MessageComposer
    {
        public string ItemId { get; }
        public string ExtraData { get; }

        public StickyNoteComposer(string itemId, string extradata)
            : base(ServerPacketHeader.StickyNoteMessageComposer)
        {
            ItemId = itemId;
            ExtraData = extradata;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteString(ItemId);
            packet.WriteString(ExtraData);
        }
    }
}
