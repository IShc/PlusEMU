using System;
using Plus.Communication.Packets.Outgoing;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items.Data.Toner;

namespace Plus.HabboHotel.Items
{
    static class ItemBehaviourUtility
    {
        public static void GenerateExtradata(Item item, ServerPacket packet)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                default:
                    packet.WriteInteger(1);
                    packet.WriteInteger(0);
                    packet.WriteString(item.GetBaseItem().InteractionType != InteractionType.FOOTBALL_GATE ? item.ExtraData : string.Empty);
                    break;

                case InteractionType.GNOME_BOX:
                    packet.WriteInteger(0);
                    packet.WriteInteger(0);
                    packet.WriteString("");
                    break;

                case InteractionType.PET_BREEDING_BOX:
                case InteractionType.PURCHASABLE_CLOTHING:
                    packet.WriteInteger(0);
                    packet.WriteInteger(0);
                    packet.WriteString("0");
                    break;

                case InteractionType.STACKTOOL:
                    packet.WriteInteger(0);
                    packet.WriteInteger(0);
                    packet.WriteString("");
                    break;

                case InteractionType.WALLPAPER:
                    packet.WriteInteger(2);
                    packet.WriteInteger(0);
                    packet.WriteString(item.ExtraData);

                    break;
                case InteractionType.FLOOR:
                    packet.WriteInteger(3);
                    packet.WriteInteger(0);
                    packet.WriteString(item.ExtraData);
                    break;

                case InteractionType.LANDSCAPE:
                    packet.WriteInteger(4);
                    packet.WriteInteger(0);
                    packet.WriteString(item.ExtraData);
                    break;

                case InteractionType.GUILD_ITEM:
                case InteractionType.GUILD_GATE:
                case InteractionType.GUILD_FORUM:
                    Group group = null;
                    if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(item.GroupId, out group))
                    {
                        packet.WriteInteger(1);
                        packet.WriteInteger(0);
                        packet.WriteString(item.ExtraData);
                    }
                    else
                    {
                        packet.WriteInteger(0);
                        packet.WriteInteger(2);
                        packet.WriteInteger(5);
                        packet.WriteString(item.ExtraData);
                        packet.WriteString(group.Id.ToString());
                        packet.WriteString(group.Badge);
                        packet.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour1, true));
                        packet.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour2, false));
                    }
                    break;

                case InteractionType.BACKGROUND:
                    packet.WriteInteger(0);
                    packet.WriteInteger(1);
                    if (!String.IsNullOrEmpty(item.ExtraData))
                    {
                        packet.WriteInteger(item.ExtraData.Split(Convert.ToChar(9)).Length / 2);

                        for (int i = 0; i <= item.ExtraData.Split(Convert.ToChar(9)).Length - 1; i++)
                        {
                            packet.WriteString(item.ExtraData.Split(Convert.ToChar(9))[i]);
                        }
                    }
                    else
                    {
                        packet.WriteInteger(0);
                    }
                    break;

                case InteractionType.GIFT:
                    {
                        string[] extraData = item.ExtraData.Split(Convert.ToChar(5));
                        if (extraData.Length != 7)
                        {
                            packet.WriteInteger(0);
                            packet.WriteInteger(0);
                            packet.WriteString(item.ExtraData);
                        }
                        else
                        {
                            int style = int.Parse(extraData[6]) * 1000 + int.Parse(extraData[6]);

                            UserCache purchaser = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Convert.ToInt32(extraData[2]));
                            if (purchaser == null)
                            {
                                packet.WriteInteger(0);
                                packet.WriteInteger(0);
                                packet.WriteString(item.ExtraData);
                            }
                            else
                            {
                                packet.WriteInteger(style);
                                packet.WriteInteger(1);
                                packet.WriteInteger(6);
                                packet.WriteString("EXTRA_PARAM");
                                packet.WriteString("");
                                packet.WriteString("MESSAGE");
                                packet.WriteString(extraData[1]);
                                packet.WriteString("PURCHASER_NAME");
                                packet.WriteString(purchaser.Username);
                                packet.WriteString("PURCHASER_FIGURE");
                                packet.WriteString(purchaser.Look);
                                packet.WriteString("PRODUCT_CODE");
                                packet.WriteString("A1 KUMIANKKA");
                                packet.WriteString("state");
                                packet.WriteString(item.MagicRemove == true ? "1" : "0");
                            }
                        }
                    }
                    break;

                case InteractionType.MANNEQUIN:
                    packet.WriteInteger(0);
                    packet.WriteInteger(1);
                    packet.WriteInteger(3);
                    if (item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                    {
                        string[] stuff = item.ExtraData.Split(Convert.ToChar(5));
                        packet.WriteString("GENDER");
                        packet.WriteString(stuff[0]);
                        packet.WriteString("FIGURE");
                        packet.WriteString(stuff[1]);
                        packet.WriteString("OUTFIT_NAME");
                        packet.WriteString(stuff[2]);
                    }
                    else
                    {
                        packet.WriteString("GENDER");
                        packet.WriteString("");
                        packet.WriteString("FIGURE");
                        packet.WriteString("");
                        packet.WriteString("OUTFIT_NAME");
                        packet.WriteString("");
                    }
                    break;

                case InteractionType.TONER:
                    if (item.RoomId != 0)
                    {
                        if (item.GetRoom().TonerData == null)
                            item.GetRoom().TonerData = new TonerData(item.Id);

                        packet.WriteInteger(0);
                        packet.WriteInteger(5);
                        packet.WriteInteger(4);
                        packet.WriteInteger(item.GetRoom().TonerData.Enabled);
                        packet.WriteInteger(item.GetRoom().TonerData.Hue);
                        packet.WriteInteger(item.GetRoom().TonerData.Saturation);
                        packet.WriteInteger(item.GetRoom().TonerData.Lightness);
                    }
                    else
                    {
                        packet.WriteInteger(0);
                        packet.WriteInteger(0);
                        packet.WriteString(string.Empty);
                    }
                    break;

                case InteractionType.BADGE_DISPLAY:
                    packet.WriteInteger(0);
                    packet.WriteInteger(2);
                    packet.WriteInteger(4);

                    string[] badgeData = item.ExtraData.Split(Convert.ToChar(9));
                    if (item.ExtraData.Contains(Convert.ToChar(9).ToString()))
                    {
                        packet.WriteString("0");//No idea
                        packet.WriteString(badgeData[0]);//Badge name
                        packet.WriteString(badgeData[1]);//Owner
                        packet.WriteString(badgeData[2]);//Date
                    }
                    else
                    {
                        packet.WriteString("0");//No idea
                        packet.WriteString("DEV");//Badge name
                        packet.WriteString("Sledmore");//Owner
                        packet.WriteString("13-13-1337");//Date
                    }
                    break;

                case InteractionType.TELEVISION:
                    packet.WriteInteger(0);
                    packet.WriteInteger(1);
                    packet.WriteInteger(1);

                    packet.WriteString("THUMBNAIL_URL");
                    //Message.WriteString("http://img.youtube.com/vi/" + PlusEnvironment.GetGame().GetTelevisionManager().TelevisionList.OrderBy(x => Guid.NewGuid()).FirstOrDefault().YouTubeId + "/3.jpg");
                    packet.WriteString("");
                    break;

                case InteractionType.LOVELOCK:
                    if (item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                    {
                        var eData = item.ExtraData.Split((char)5);
                        int I = 0;
                        packet.WriteInteger(0);
                        packet.WriteInteger(2);
                        packet.WriteInteger(eData.Length);
                        while (I < eData.Length)
                        {
                            packet.WriteString(eData[I]);
                            I++;
                        }
                    }
                    else
                    {
                        packet.WriteInteger(0);
                        packet.WriteInteger(0);
                        packet.WriteString("0");
                    }
                    break;

                case InteractionType.MONSTERPLANT_SEED:
                    packet.WriteInteger(0);
                    packet.WriteInteger(1);
                    packet.WriteInteger(1);

                    packet.WriteString("rarity");
                    packet.WriteString("1");//Leve should be dynamic.
                    break;
            }
        }

        public static void GenerateWallExtradata(Item item, ServerPacket message)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                default:
                    message.WriteString(item.ExtraData);
                    break;

                case InteractionType.POSTIT:
                    message.WriteString(item.ExtraData.Split(' ')[0]);
                    break;
            }
        }
    }
}