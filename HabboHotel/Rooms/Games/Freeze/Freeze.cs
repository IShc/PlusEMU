using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Freeze;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.Games.Teams;

namespace Plus.HabboHotel.Rooms.Games.Freeze
{
    public class Freeze
    {
        private Room _room;
        private Random _random;
        private readonly ConcurrentDictionary<int, Item> _freezeBlocks;
        private readonly ConcurrentDictionary<int, Item> _freezeTiles;

        public Freeze(Room room)
        {
            _room = room;
            GameIsStarted = false;
            ExitTeleports = new ConcurrentDictionary<int, Item>();
            _random = new Random();
            _freezeTiles = new ConcurrentDictionary<int, Item>();
            _freezeBlocks = new ConcurrentDictionary<int, Item>();
        }

        public bool GameIsStarted { get; private set; }

        public ConcurrentDictionary<int, Item> ExitTeleports { get; }

        public void AddExitTile(Item Item)
        {
            if (!ExitTeleports.ContainsKey(Item.Id))
                ExitTeleports.TryAdd(Item.Id, Item);
        }

        public void RemoveExitTile(int Id)
        {
            Item Temp;
            if (ExitTeleports.ContainsKey(Id))
                ExitTeleports.TryRemove(Id, out Temp);
        }

        public Item GetRandomExitTile()
        {
            return ExitTeleports.Values.ToList()[PlusEnvironment.GetRandomNumber(0, ExitTeleports.Count - 1)];
        }

        public void StartGame()
        {
            GameIsStarted = true;
            CountTeamPoints();
            ResetGame();

            if (ExitTeleports.Count > 0)
            {
                foreach (Item ExitTile in ExitTeleports.Values.ToList())
                {
                    if (ExitTile.ExtraData == "0" || String.IsNullOrEmpty(ExitTile.ExtraData))
                        ExitTile.ExtraData = "1";

                    ExitTile.UpdateState();
                }
            }

            _room.GetGameManager().LockGates();
        }

        public void StopGame(bool userTriggered = false)
        {
            GameIsStarted = false;
            _room.GetGameManager().UnlockGates();
            _room.GetGameManager().StopGame();

            ResetGame();

            if (ExitTeleports.Count > 0)
            {
                foreach (Item ExitTile in ExitTeleports.Values.ToList())
                {
                    if (ExitTile.ExtraData == "1" || String.IsNullOrEmpty(ExitTile.ExtraData))
                        ExitTile.ExtraData = "0";

                    ExitTile.UpdateState();
                }
            }

            Team Winners = _room.GetGameManager().GetWinningTeam();
            foreach (RoomUser User in _room.GetRoomUserManager().GetUserList().ToList())
            {
                User.FreezeLives = 0;
                if (User.Team == Winners)
                {
                    User.UnIdle();
                    User.DanceId = 0;
                    _room.SendPacket(new ActionComposer(User.VirtualId, 1));
                }

                if (ExitTeleports.Count > 0)
                {
                    Item tile = _freezeTiles.Values.Where(x => x.GetX == User.X && x.GetY == User.Y).FirstOrDefault();
                    if (tile != null)
                    {
                        Item ExitTle = GetRandomExitTile();

                        if (ExitTle != null)
                        {
                            _room.GetGameMap().UpdateUserMovement(User.Coordinate, ExitTle.Coordinate, User);
                            User.SetPos(ExitTle.GetX, ExitTle.GetY, ExitTle.GetZ);
                            User.UpdateNeeded = true;

                            if (User.IsAsleep)
                                User.UnIdle();
                        }
                    }
                }
            }

            if (!userTriggered)
                _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameEnds, null);
        }

        public void CycleUser(RoomUser User)
        {
            if (User.Freezed)
            {
                User.FreezeCounter++;
                if (User.FreezeCounter > 10)
                {
                    User.Freezed = false;
                    User.FreezeCounter = 0;
                    ActivateShield(User);
                }
            }

            if (User.ShieldActive)
            {
                User.ShieldCounter++;
                if (User.ShieldCounter > 10)
                {
                    User.ShieldActive = false;
                    User.ShieldCounter = 10;
                    User.ApplyEffect(Convert.ToInt32(User.Team) + 39);
                }
            }
        }

        public void ResetGame()
        {
            foreach (Item Item in _freezeTiles.Values.ToList())
            {
                if (!string.IsNullOrEmpty(Item.ExtraData))
                {
                    Item.InteractionCountHelper = 0;
                    Item.ExtraData = "";
                    Item.UpdateState(false, true);
                    _room.GetGameMap().AddItemToMap(Item, false);
                }
            }

            foreach (Item Item in _freezeBlocks.Values)
            {
                if (!string.IsNullOrEmpty(Item.ExtraData))
                {
                    Item.ExtraData = "";
                    Item.UpdateState(false, true);
                    _room.GetGameMap().AddItemToMap(Item, false);
                }
            }
        }

        public void OnUserWalk(RoomUser User)
        {
            if (!GameIsStarted || User.Team == Team.None)
                return;

            foreach (Item Item in _freezeTiles.Values.ToList())
            {
                if (User.GoalX == Item.GetX && User.GoalY == Item.GetY && User.FreezeInteracting)
                {
                    if (Item.InteractionCountHelper == 0)
                    {
                        Item.InteractionCountHelper = 1;
                        Item.ExtraData = "1000";
                        Item.UpdateState();
                        Item.InteractingUser = User.UserId;
                        Item.FreezePowerUp = User.BanzaiPowerUp;
                        Item.RequestUpdate(4, true);

                        switch (User.BanzaiPowerUp)
                        {
                            case FreezePowerUp.GreenArrow:
                            case FreezePowerUp.OrangeSnowball:
                                {
                                    User.BanzaiPowerUp = FreezePowerUp.None;
                                    break;
                                }
                        }
                        break;
                    }
                }
            }

            foreach (Item Item in _freezeBlocks.Values.ToList())
            {
                if (User.GoalX == Item.GetX && User.GoalY == Item.GetY)
                {
                    if (Item.FreezePowerUp != FreezePowerUp.None)
                    {
                        PickUpPowerUp(Item, User);
                    }
                }
            }
        }

        private void CountTeamPoints()
        {
            _room.GetGameManager().Reset();

            foreach (RoomUser User in _room.GetRoomUserManager().GetUserList().ToList())
            {
                if (User.IsBot || User.Team == Team.None || User.GetClient() == null)
                    continue;

                User.BanzaiPowerUp = FreezePowerUp.None;
                User.FreezeLives = 3;
                User.ShieldActive = false;
                User.ShieldCounter = 11;

                _room.GetGameManager().AddPointToTeam(User.Team, 30);
                User.GetClient().SendPacket(new UpdateFreezeLivesComposer(User.InternalRoomId, User.FreezeLives));
            }
        }

        public void onFreezeTiles(Item item, FreezePowerUp powerUp)
        {
            List<Item> items;

            switch (powerUp)
            {
                case FreezePowerUp.BlueArrow:
                    {
                        items = GetVerticalItems(item.GetX, item.GetY, 5);
                        break;
                    }

                case FreezePowerUp.GreenArrow:
                    {
                        items = GetDiagonalItems(item.GetX, item.GetY, 5);
                        break;
                    }

                case FreezePowerUp.OrangeSnowball:
                    {
                        items = GetVerticalItems(item.GetX, item.GetY, 5);
                        items.AddRange(GetDiagonalItems(item.GetX, item.GetY, 5));
                        break;
                    }

                default:
                    {
                        items = GetVerticalItems(item.GetX, item.GetY, 3);
                        break;
                    }
            }
            HandleBanzaiFreezeItems(items);
        }

        private static void ActivateShield(RoomUser User)
        {
            User.ApplyEffect(Convert.ToInt32(User.Team + 48));
            User.ShieldActive = true;
            User.ShieldCounter = 0;
        }

        private void HandleBanzaiFreezeItems(List<Item> items)
        {
            foreach (Item item in items.ToList())
            {
                switch (item.GetBaseItem().InteractionType)
                {
                    case InteractionType.FREEZE_TILE:
                        {
                            item.ExtraData = "11000";
                            item.UpdateState(false, true);
                            continue;
                        }

                    case InteractionType.FREEZE_TILE_BLOCK:
                        {
                            SetRandomPowerUp(item);
                            item.UpdateState(false, true);
                            continue;
                        }
                    default:
                        {
                            continue;
                        }
                }
            }
        }

        private void SetRandomPowerUp(Item item)
        {
            if (!string.IsNullOrEmpty(item.ExtraData))
                return;

            int next = _random.Next(1, 14);

            switch (next)
            {
                case 2:
                    {
                        item.ExtraData = "2000";
                        item.FreezePowerUp = FreezePowerUp.BlueArrow;
                        break;
                    }
                case 3:
                    {
                        item.ExtraData = "3000";
                        item.FreezePowerUp = FreezePowerUp.Snowballs;
                        break;
                    }
                case 4:
                    {
                        item.ExtraData = "4000";
                        item.FreezePowerUp = FreezePowerUp.GreenArrow;
                        break;
                    }
                case 5:
                    {
                        item.ExtraData = "5000";
                        item.FreezePowerUp = FreezePowerUp.OrangeSnowball;
                        break;
                    }
                case 6:
                    {
                        item.ExtraData = "6000";
                        item.FreezePowerUp = FreezePowerUp.Heart;
                        break;
                    }
                case 7:
                    {
                        item.ExtraData = "7000";
                        item.FreezePowerUp = FreezePowerUp.Shield;
                        break;
                    }
                default:
                    {
                        item.ExtraData = "1000";
                        item.FreezePowerUp = FreezePowerUp.None;
                        break;
                    }
            }

            _room.GetGameMap().RemoveFromMap(item, false);
            item.UpdateState(false, true);
        }

        private void PickUpPowerUp(Item item, RoomUser User)
        {
            switch (item.FreezePowerUp)
            {
                case FreezePowerUp.Heart:
                    {
                        if (User.FreezeLives < 5)
                        {
                            User.FreezeLives++;
                            _room.GetGameManager().AddPointToTeam(User.Team, 10);
                        }

                        User.GetClient().SendPacket(new UpdateFreezeLivesComposer(User.InternalRoomId, User.FreezeLives));
                        break;
                    }
                case FreezePowerUp.Shield:
                    {
                        ActivateShield(User);
                        break;
                    }
                case FreezePowerUp.BlueArrow:
                case FreezePowerUp.GreenArrow:
                case FreezePowerUp.OrangeSnowball:
                    {
                        User.BanzaiPowerUp = item.FreezePowerUp;
                        break;
                    }
            }

            item.FreezePowerUp = FreezePowerUp.None;
            item.ExtraData = "1" + item.ExtraData;
            item.UpdateState(false, true);
        }

        public void AddFreezeTile(Item Item)
        {
            if (!_freezeTiles.ContainsKey(Item.Id))
                _freezeTiles.TryAdd(Item.Id, Item);
        }

        public void RemoveFreezeTile(int itemID)
        {
            Item Item = null;
            if (_freezeTiles.ContainsKey(itemID))
                _freezeTiles.TryRemove(itemID, out Item);
        }

        public void AddFreezeBlock(Item Item)
        {
            if (!_freezeBlocks.ContainsKey(Item.Id))
                _freezeBlocks.TryAdd(Item.Id, Item);
        }

        public void RemoveFreezeBlock(int ItemID)
        {
            Item Item = null;
            _freezeBlocks.TryRemove(ItemID, out Item);
        }

        private void HandleUserFreeze(Point point)
        {
            if (_room == null)
                return;

            RoomUser user = _room.GetGameMap().GetRoomUsers(point).FirstOrDefault();
            if (user != null)
            {
                if (user.IsWalking && user.SetX != point.X && user.SetY != point.Y)
                    return;

                FreezeUser(user);
            }
        }

        private void FreezeUser(RoomUser User)
        {
            if (User.IsBot || User.ShieldActive || User.Team == Team.None || User.Freezed)
                return;

            User.Freezed = true;
            User.FreezeCounter = 0;

            User.FreezeLives--;
            if (User.FreezeLives <= 0)
            {
                User.GetClient().SendPacket(new UpdateFreezeLivesComposer(User.InternalRoomId, User.FreezeLives));

                User.ApplyEffect(-1);
                _room.GetGameManager().AddPointToTeam(User.Team, -10);
                TeamManager t = _room.GetTeamManagerForFreeze();
                t.OnUserLeave(User);
                User.Team = Team.None;
                if (ExitTeleports.Count > 0)
                    _room.GetGameMap().TeleportToItem(User, GetRandomExitTile());

                User.Freezed = false;
                User.SetStep = false;
                User.IsWalking = false;
                User.UpdateNeeded = true;

                if (t.BlueTeam.Count <= 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count <= 0 && t.YellowTeam.Count > 0)
                    StopGame(); // yellow team win
                else if (t.BlueTeam.Count > 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count <= 0 &&
                         t.YellowTeam.Count <= 0)
                    StopGame(); // blue team win
                else if (t.BlueTeam.Count <= 0 && t.RedTeam.Count > 0 && t.GreenTeam.Count <= 0 &&
                         t.YellowTeam.Count <= 0)
                    StopGame(); // red team win
                else if (t.BlueTeam.Count <= 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count > 0 &&
                         t.YellowTeam.Count <= 0)
                    StopGame(); // green team win
                return;
            }

            _room.GetGameManager().AddPointToTeam(User.Team, -10);
            User.ApplyEffect(12);

            User.GetClient().SendPacket(new UpdateFreezeLivesComposer(User.InternalRoomId, User.FreezeLives));
        }

        private List<Item> GetVerticalItems(int x, int y, int length)
        {
            var totalItems = new List<Item>();

            for (int i = 0; i < length; i++)
            {
                var point = new Point(x + i, y);

                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 1; i < length; i++)
            {
                var point = new Point(x, y + i);

                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 1; i < length; i++)
            {
                var point = new Point(x - i, y);
                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 1; i < length; i++)
            {
                var point = new Point(x, y - i);
                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            return totalItems;
        }

        private List<Item> GetDiagonalItems(int x, int y, int length)
        {
            var totalItems = new List<Item>();

            for (int i = 0; i < length; i++)
            {
                var point = new Point(x + i, y + i);

                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 0; i < length; i++)
            {
                var point = new Point(x - i, y - i);
                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 0; i < length; i++)
            {
                var point = new Point(x - i, y + i);
                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 0; i < length; i++)
            {
                var point = new Point(x + i, y - i);
                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            return totalItems;
        }

        private List<Item> GetItemsForSquare(Point point)
        {
            return _room.GetGameMap().GetCoordinatedItems(point);
        }

        private static bool SquareGotFreezeTile(List<Item> items)
        {
            foreach (Item item in items)
            {
                if (item.GetBaseItem().InteractionType == InteractionType.FREEZE_TILE)
                    return true;
            }

            return false;
        }

        private static bool SquareGotFreezeBlock(List<Item> items)
        {
            foreach (Item item in items)
            {
                if (item.GetBaseItem().InteractionType == InteractionType.FREEZE_TILE_BLOCK)
                    return true;
            }

            return false;
        }

        public void Dispose()
        {
            _room = null;
            _random = null;
            ExitTeleports.Clear();
            _freezeTiles.Clear();
            _freezeBlocks.Clear();
        }
    }
}