using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Games.Teams
{
    public class TeamManager
    {
        public string Game;
        public List<RoomUser> BlueTeam;
        public List<RoomUser> GreenTeam;
        public List<RoomUser> RedTeam;
        public List<RoomUser> YellowTeam;

        public static TeamManager CreateTeam(string Game)
        {
            var t = new TeamManager();
            t.Game = Game;
            t.BlueTeam = new List<RoomUser>();
            t.RedTeam = new List<RoomUser>();
            t.GreenTeam = new List<RoomUser>();
            t.YellowTeam = new List<RoomUser>();
            return t;
        }

        public bool CanEnterOnTeam(Team t)
        {
            if (t.Equals(Team.Blue))
                return (BlueTeam.Count < 5);
            else if (t.Equals(Team.Red))
                return (RedTeam.Count < 5);
            else if (t.Equals(Team.Yellow))
                return (YellowTeam.Count < 5);
            else if (t.Equals(Team.Green))
                return (GreenTeam.Count < 5);
            return false;
        }

        public void AddUser(RoomUser user)
        {
            if (user.Team.Equals(Team.Blue) && !BlueTeam.Contains(user))
                BlueTeam.Add(user);
            else if (user.Team.Equals(Team.Red) && !RedTeam.Contains(user))
                RedTeam.Add(user);
            else if (user.Team.Equals(Team.Yellow) && !YellowTeam.Contains(user))
                YellowTeam.Add(user);
            else if (user.Team.Equals(Team.Green) && !GreenTeam.Contains(user))
                GreenTeam.Add(user);

            switch (Game.ToLower())
            {
                case "banzai":
                    {
                        Room room = user.GetClient().GetHabbo().CurrentRoom;
                        if (room == null)
                            return;

                        foreach (Item Item in room.GetRoomItemHandler().GetFloor.ToList())
                        {
                            if (Item == null)
                                continue;

                            if (Item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateBlue))
                            {
                                Item.ExtraData = BlueTeam.Count.ToString();
                                Item.UpdateState();
                                if (BlueTeam.Count == 5)
                                {
                                    foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(Item.GetX, Item.GetY)))
                                    {
                                        sser.SqState = 0;
                                    }

                                    room.GetGameMap().GameMap[Item.GetX, Item.GetY] = 0;
                                }
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateRed))
                            {
                                Item.ExtraData = RedTeam.Count.ToString();
                                Item.UpdateState();
                                if (RedTeam.Count == 5)
                                {
                                    foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(Item.GetX, Item.GetY)))
                                    {
                                        sser.SqState = 0;
                                    }

                                    room.GetGameMap().GameMap[Item.GetX, Item.GetY] = 0;
                                }
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateGreen))
                            {
                                Item.ExtraData = GreenTeam.Count.ToString();
                                Item.UpdateState();
                                if (GreenTeam.Count == 5)
                                {
                                    foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(Item.GetX, Item.GetY)))
                                        sser.SqState = 0;

                                    room.GetGameMap().GameMap[Item.GetX, Item.GetY] = 0;
                                }
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateYellow))
                            {
                                Item.ExtraData = YellowTeam.Count.ToString();
                                Item.UpdateState();
                                if (YellowTeam.Count == 5)
                                {
                                    foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(Item.GetX, Item.GetY)))
                                        sser.SqState = 0;

                                    room.GetGameMap().GameMap[Item.GetX, Item.GetY] = 0;
                                }
                            }
                        }
                        break;
                    }

                case "freeze":
                    {
                        Room room = user.GetClient().GetHabbo().CurrentRoom;
                        if (room == null)
                            return;

                        foreach (Item Item in room.GetRoomItemHandler().GetFloor.ToList())
                        {
                            if (Item == null)
                                continue;

                            if (Item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeBlueGate))
                            {
                                Item.ExtraData = BlueTeam.Count.ToString();
                                Item.UpdateState();
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeRedGate))
                            {
                                Item.ExtraData = RedTeam.Count.ToString();
                                Item.UpdateState();
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeGreenGate))
                            {
                                Item.ExtraData = GreenTeam.Count.ToString();
                                Item.UpdateState();
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeYellowGate))
                            {
                                Item.ExtraData = YellowTeam.Count.ToString();
                                Item.UpdateState();
                            }
                        }
                        break;
                    }
            }
        }

        public void OnUserLeave(RoomUser user)
        {
            //Console.WriteLine("remove user from team! (" + Game + ")");
            if (user.Team.Equals(Team.Blue) && BlueTeam.Contains(user))
                BlueTeam.Remove(user);
            else if (user.Team.Equals(Team.Red) && RedTeam.Contains(user))
                RedTeam.Remove(user);
            else if (user.Team.Equals(Team.Yellow) && YellowTeam.Contains(user))
                YellowTeam.Remove(user);
            else if (user.Team.Equals(Team.Green) && GreenTeam.Contains(user))
                GreenTeam.Remove(user);

            switch (Game.ToLower())
            {
                case "banzai":
                    {
                        Room room = user.GetClient().GetHabbo().CurrentRoom;
                        if (room == null)
                            return;

                        foreach (Item Item in room.GetRoomItemHandler().GetFloor.ToList())
                        {
                            if (Item == null)
                                continue;

                            if (Item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateBlue))
                            {
                                Item.ExtraData = BlueTeam.Count.ToString();
                                Item.UpdateState();
                                if (room.GetGameMap().GameMap[Item.GetX, Item.GetY] == 0)
                                {
                                    foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(Item.GetX, Item.GetY)))
                                        sser.SqState = 1;

                                    room.GetGameMap().GameMap[Item.GetX, Item.GetY] = 1;
                                }
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateRed))
                            {
                                Item.ExtraData = RedTeam.Count.ToString();
                                Item.UpdateState();
                                if (room.GetGameMap().GameMap[Item.GetX, Item.GetY] == 0)
                                {
                                    foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(Item.GetX, Item.GetY)))
                                        sser.SqState = 1;


                                    room.GetGameMap().GameMap[Item.GetX, Item.GetY] = 1;
                                }
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateGreen))
                            {
                                Item.ExtraData = GreenTeam.Count.ToString();
                                Item.UpdateState();
                                if (room.GetGameMap().GameMap[Item.GetX, Item.GetY] == 0)
                                {
                                    foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(Item.GetX, Item.GetY)))

                                        sser.SqState = 1;


                                    room.GetGameMap().GameMap[Item.GetX, Item.GetY] = 1;
                                }
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.BanzaiGateYellow))
                            {
                                Item.ExtraData = YellowTeam.Count.ToString();
                                Item.UpdateState();
                                if (room.GetGameMap().GameMap[Item.GetX, Item.GetY] == 0)
                                {
                                    foreach (RoomUser sser in room.GetGameMap().GetRoomUsers(new Point(Item.GetX, Item.GetY)))
                                        sser.SqState = 1;


                                    room.GetGameMap().GameMap[Item.GetX, Item.GetY] = 1;
                                }
                            }
                        }
                        break;
                    }
                case "freeze":
                    {
                        Room room = user.GetClient().GetHabbo().CurrentRoom;
                        if (room == null)
                            return;

                        foreach (Item Item in room.GetRoomItemHandler().GetFloor.ToList())
                        {
                            if (Item == null)
                                continue;

                            if (Item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeBlueGate))
                            {
                                Item.ExtraData = BlueTeam.Count.ToString();
                                Item.UpdateState();
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeRedGate))
                            {
                                Item.ExtraData = RedTeam.Count.ToString();
                                Item.UpdateState();
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeGreenGate))
                            {
                                Item.ExtraData = GreenTeam.Count.ToString();
                                Item.UpdateState();
                            }
                            else if (Item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeYellowGate))
                            {
                                Item.ExtraData = YellowTeam.Count.ToString();
                                Item.UpdateState();
                            }
                        }
                        break;
                    }
            }
        }

        public void Dispose()
        {
            BlueTeam.Clear();
            GreenTeam.Clear();
            RedTeam.Clear();
            YellowTeam.Clear();
        }
    }
}