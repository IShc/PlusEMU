﻿using System;
using Plus.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    class ObjectsComposer : MessageComposer
    {
        public Item[] Objects { get; }
        public int OwnerId { get; }
        public string OwnerName { get; }

        public ObjectsComposer(Item[] objects, Room room)
            : base(ServerPacketHeader.ObjectsMessageComposer)
        {
            Objects = objects;
            OwnerId = room.OwnerId;
            OwnerName = room.OwnerName;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(1);

            packet.WriteInteger(OwnerId);
            packet.WriteString(OwnerName);

            packet.WriteInteger(Objects.Length);
            foreach (Item item in Objects)
            {
                WriteFloorItem(item, Convert.ToInt32(item.UserID), packet);
            }
        }

        private void WriteFloorItem(Item item, int userId, ServerPacket packet)
        {
            packet.WriteInteger(item.Id);
            packet.WriteInteger(item.GetBaseItem().SpriteId);
            packet.WriteInteger(item.GetX);
            packet.WriteInteger(item.GetY);
            packet.WriteInteger(item.Rotation);
            packet.WriteString(String.Format("{0:0.00}", TextHandling.GetString(item.GetZ)));
            packet.WriteString(String.Empty);

            if (item.LimitedNo > 0)
            {
                packet.WriteInteger(1);
                packet.WriteInteger(256);
                packet.WriteString(item.ExtraData);
                packet.WriteInteger(item.LimitedNo);
                packet.WriteInteger(item.LimitedTot);
            }
            else
            {
                ItemBehaviourUtility.GenerateExtradata(item, packet);
            }

            packet.WriteInteger(-1); // to-do: check
            packet.WriteInteger((item.GetBaseItem().Modes > 1) ? 1 : 0);
            packet.WriteInteger(userId);
        }
    }
}