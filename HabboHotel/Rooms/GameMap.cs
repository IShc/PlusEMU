using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Core;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Rooms.PathFinding;

namespace Plus.HabboHotel.Rooms
{
    public class Gamemap
    {
        private Room _room;

        public bool DiagonalEnabled { get; set; }
        private double[,] _itemHeightMap;
        private ConcurrentDictionary<Point, List<int>> _coordinatedItems;
        private ConcurrentDictionary<Point, List<RoomUser>> _userMap;

        public Gamemap(Room room, RoomModel model)
        {
            _room = room;
            StaticModel = model;
            DiagonalEnabled = true;

            Model = new DynamicRoomModel(StaticModel);
            _coordinatedItems = new ConcurrentDictionary<Point, List<int>>();
            _itemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
            _userMap = new ConcurrentDictionary<Point, List<RoomUser>>();
        }

        public void AddUserToMap(RoomUser user, Point coord)
        {
            if (_userMap.ContainsKey(coord))
            {
                _userMap[coord].Add(user);
            }
            else
            {
                List<RoomUser> users = new List<RoomUser>
                {
                    user
                };
                _userMap.TryAdd(coord, users);
            }
        }

        public void TeleportToItem(RoomUser user, Item item)
        {
            if (item == null || user == null)
                return;

            GameMap[user.X, user.Y] = user.SqState;
            UpdateUserMovement(new Point(user.Coordinate.X, user.Coordinate.Y), new Point(item.Coordinate.X, item.Coordinate.Y), user);
            user.X = item.GetX;
            user.Y = item.GetY;
            user.Z = item.GetZ;

            user.SqState = GameMap[item.GetX, item.GetY];
            GameMap[user.X, user.Y] = 1;
            user.RotBody = item.Rotation;
            user.RotHead = item.Rotation;

            user.GoalX = user.X;
            user.GoalY = user.Y;
            user.SetStep = false;
            user.IsWalking = false;
            user.UpdateNeeded = true;
        }

        public void UpdateUserMovement(Point oldCoord, Point newCoord, RoomUser user)
        {
            RemoveUserFromMap(user, oldCoord);
            AddUserToMap(user, newCoord);
        }

        public void RemoveUserFromMap(RoomUser user, Point coord)
        {
            if (_userMap.ContainsKey(coord))
                ((List<RoomUser>)_userMap[coord]).RemoveAll(x => x != null && x.VirtualId == user.VirtualId);
        }

        public bool MapGotUser(Point coord)
        {
            return (GetRoomUsers(coord).Count > 0);
        }

        public List<RoomUser> GetRoomUsers(Point coord)
        {
            if (_userMap.ContainsKey(coord))
                return (List<RoomUser>)_userMap[coord];
            else
                return new List<RoomUser>();
        }

        public Point GetRandomWalkableSquare()
        {
            var walkableSquares = new List<Point>();
            for (int y = 0; y < GameMap.GetUpperBound(1); y++)
            {
                for (int x = 0; x < GameMap.GetUpperBound(0); x++)
                {
                    if (StaticModel.DoorX != x && StaticModel.DoorY != y && GameMap[x, y] == 1)
                        walkableSquares.Add(new Point(x, y));
                }
            }

            int randomNumber = PlusEnvironment.GetRandomNumber(0, walkableSquares.Count);
            int i = 0;

            foreach (Point coord in walkableSquares.ToList())
            {
                if (i == randomNumber)
                    return coord;
                i++;
            }

            return new Point(0, 0);
        }


        public bool IsInMap(int X, int Y)
        {
            var walkableSquares = new List<Point>();
            for (int y = 0; y < GameMap.GetUpperBound(1); y++)
            {
                for (int x = 0; x < GameMap.GetUpperBound(0); x++)
                {
                    if (StaticModel.DoorX != x && StaticModel.DoorY != y && GameMap[x, y] == 1)
                        walkableSquares.Add(new Point(x, y));
                }
            }

            if (walkableSquares.Contains(new Point(X, Y)))
                return true;
            return false;
        }

        public void AddToMap(Item item)
        {
            AddItemToMap(item);
        }

        private void SetDefaultValue(int x, int y)
        {
            GameMap[x, y] = 0;
            EffectMap[x, y] = 0;
            _itemHeightMap[x, y] = 0.0;

            if (x == Model.DoorX && y == Model.DoorY)
            {
                GameMap[x, y] = 3;
            }
            else if (Model.SqState[x, y] == SquareState.Open)
            {
                GameMap[x, y] = 1;
            }
            else if (Model.SqState[x, y] == SquareState.Seat)
            {
                GameMap[x, y] = 2;
            }
        }

        public void UpdateMapForItem(Item item)
        {
            RemoveFromMap(item);
            AddToMap(item);
        }

        public void GenerateMaps(bool checkLines = true)
        {
            int maxX = 0;
            int maxY = 0;
            _coordinatedItems = new ConcurrentDictionary<Point, List<int>>();

            if (checkLines)
            {
                Item[] items = _room.GetRoomItemHandler().GetFloor.ToArray();
                foreach (Item item in items.ToList())
                {
                    if (item == null)
                        continue;

                    if (item.GetX > Model.MapSizeX && item.GetX > maxX)
                        maxX = item.GetX;
                    if (item.GetY > Model.MapSizeY && item.GetY > maxY)
                        maxY = item.GetY;
                }

                Array.Clear(items, 0, items.Length);
                items = null;
            }


            if (maxY > (Model.MapSizeY - 1) || maxX > (Model.MapSizeX - 1))
            {
                if (maxX < Model.MapSizeX)
                    maxX = Model.MapSizeX;
                if (maxY < Model.MapSizeY)
                    maxY = Model.MapSizeY;

                Model.SetMapsize(maxX + 7, maxY + 7);
                GenerateMaps(false);
                return;
            }

            if (maxX != StaticModel.MapSizeX || maxY != StaticModel.MapSizeY)
            {
                EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                GameMap = new byte[Model.MapSizeX, Model.MapSizeY];


                _itemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
                //if (modelRemap)
                //    Model.Generate(); //Clears model

                for (int line = 0; line < Model.MapSizeY; line++)
                {
                    for (int chr = 0; chr < Model.MapSizeX; chr++)
                    {
                        GameMap[chr, line] = 0;
                        EffectMap[chr, line] = 0;

                        if (chr == Model.DoorX && line == Model.DoorY)
                        {
                            GameMap[chr, line] = 3;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Open)
                        {
                            GameMap[chr, line] = 1;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Seat)
                        {
                            GameMap[chr, line] = 2;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Pool)
                        {
                            EffectMap[chr, line] = 6;
                        }
                    }
                }
            }
            else
            {
                EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                GameMap = new byte[Model.MapSizeX, Model.MapSizeY];


                _itemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];

                for (int line = 0; line < Model.MapSizeY; line++)
                {
                    for (int chr = 0; chr < Model.MapSizeX; chr++)
                    {
                        GameMap[chr, line] = 0;
                        EffectMap[chr, line] = 0;

                        if (chr == Model.DoorX && line == Model.DoorY)
                        {
                            GameMap[chr, line] = 3;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Open)
                        {
                            GameMap[chr, line] = 1;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Seat)
                        {
                            GameMap[chr, line] = 2;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Pool)
                        {
                            EffectMap[chr, line] = 6;
                        }
                    }
                }
            }

            Item[] tmpItems = _room.GetRoomItemHandler().GetFloor.ToArray();
            foreach (Item item in tmpItems.ToList())
            {
                if (item == null)
                    continue;

                if (!AddItemToMap(item))
                    continue;
            }
            Array.Clear(tmpItems, 0, tmpItems.Length);
            tmpItems = null;

            if (_room.RoomBlockingEnabled == 0)
            {
                foreach (RoomUser user in _room.GetRoomUserManager().GetUserList().ToList())
                {
                    if (user == null)
                        continue;

                    user.SqState = GameMap[user.X, user.Y];
                    GameMap[user.X, user.Y] = 0;
                }
            }

            try
            {
                GameMap[Model.DoorX, Model.DoorY] = 3;
            }
            catch { }
        }

        private bool ConstructMapForItem(Item item, Point coord)
        {
            try
            {
                if (coord.X > (Model.MapSizeX - 1))
                {
                    Model.AddX();
                    GenerateMaps();
                    return false;
                }

                if (coord.Y > (Model.MapSizeY - 1))
                {
                    Model.AddY();
                    GenerateMaps();
                    return false;
                }

                if (Model.SqState[coord.X, coord.Y] == SquareState.Blocked)
                {
                    Model.OpenSquare(coord.X, coord.Y, item.GetZ);
                }
                if (_itemHeightMap[coord.X, coord.Y] <= item.TotalHeight)
                {
                    _itemHeightMap[coord.X, coord.Y] = item.TotalHeight - Model.SqFloorHeight[item.GetX, item.GetY];
                    EffectMap[coord.X, coord.Y] = 0;


                    switch (item.GetBaseItem().InteractionType)
                    {
                        case InteractionType.POOL:
                            EffectMap[coord.X, coord.Y] = 1;
                            break;
                        case InteractionType.NORMAL_SKATES:
                            EffectMap[coord.X, coord.Y] = 2;
                            break;
                        case InteractionType.ICE_SKATES:
                            EffectMap[coord.X, coord.Y] = 3;
                            break;
                        case InteractionType.lowpool:
                            EffectMap[coord.X, coord.Y] = 4;
                            break;
                        case InteractionType.haloweenpool:
                            EffectMap[coord.X, coord.Y] = 5;
                            break;
                    }


                    //SwimHalloween
                    if (item.GetBaseItem().Walkable)    // If this item is walkable and on the floor, allow users to walk here.
                    {
                        if (GameMap[coord.X, coord.Y] != 3)
                            GameMap[coord.X, coord.Y] = 1;
                    }
                    else if (item.GetZ <= (Model.SqFloorHeight[item.GetX, item.GetY] + 0.1) && item.GetBaseItem().InteractionType == InteractionType.GATE && item.ExtraData == "1")// If this item is a gate, open, and on the floor, allow users to walk here.
                    {
                        if (GameMap[coord.X, coord.Y] != 3)
                            GameMap[coord.X, coord.Y] = 1;
                    }
                    else if (item.GetBaseItem().IsSeat || item.GetBaseItem().InteractionType == InteractionType.BED || item.GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
                    {
                        GameMap[coord.X, coord.Y] = 3;
                    }
                    else // Finally, if it's none of those, block the square.
                    {
                        if (GameMap[coord.X, coord.Y] != 3)
                            GameMap[coord.X, coord.Y] = 0;
                    }
                }

                // Set bad maps
                if (item.GetBaseItem().InteractionType == InteractionType.BED || item.GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
                    GameMap[coord.X, coord.Y] = 3;
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
            return true;
        }

        public void AddCoordinatedItem(Item item, Point coord)
        {
            List<int> items = new List<int>(); //mCoordinatedItems[CoordForItem];

            if (!_coordinatedItems.TryGetValue(coord, out items))
            {
                items = new List<int>();

                if (!items.Contains(item.Id))
                    items.Add(item.Id);

                if (!_coordinatedItems.ContainsKey(coord))
                    _coordinatedItems.TryAdd(coord, items);
            }
            else
            {
                if (!items.Contains(item.Id))
                {
                    items.Add(item.Id);
                    _coordinatedItems[coord] = items;
                }
            }
        }

        public List<Item> GetCoordinatedItems(Point coord)
        {
            var point = new Point(coord.X, coord.Y);
            List<Item> items = new List<Item>();

            if (_coordinatedItems.ContainsKey(point))
            {
                List<int> ids = _coordinatedItems[point];
                items = GetItemsFromIds(ids);
                return items;
            }

            return new List<Item>();
        }

        public bool RemoveCoordinatedItem(Item item, Point coord)
        {
            Point point = new Point(coord.X, coord.Y);
            if (_coordinatedItems != null && _coordinatedItems.ContainsKey(point))
            {
                ((List<int>)_coordinatedItems[point]).RemoveAll(x => x == item.Id);
                return true;
            }
            return false;
        }

        private void AddSpecialItems(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FOOTBALL_GATE:
                    //IsTrans = true;
                    _room.GetSoccer().RegisterGate(item);


                    string[] splittedExtraData = item.ExtraData.Split(':');

                    if (string.IsNullOrEmpty(item.ExtraData) || splittedExtraData.Length <= 1)
                    {
                        item.Gender = "M";
                        switch (item.Team)
                        {
                            case Team.Yellow:
                                item.Figure = "lg-275-93.hr-115-61.hd-207-14.ch-265-93.sh-305-62";
                                break;
                            case Team.Red:
                                item.Figure = "lg-275-96.hr-115-61.hd-180-3.ch-265-96.sh-305-62";
                                break;
                            case Team.Green:
                                item.Figure = "lg-275-102.hr-115-61.hd-180-3.ch-265-102.sh-305-62";
                                break;
                            case Team.Blue:
                                item.Figure = "lg-275-108.hr-115-61.hd-180-3.ch-265-108.sh-305-62";
                                break;
                        }
                    }
                    else
                    {
                        item.Gender = splittedExtraData[0];
                        item.Figure = splittedExtraData[1];
                    }
                    break;

                case InteractionType.banzaifloor:
                    {
                        _room.GetBanzai().AddTile(item, item.Id);
                        break;
                    }

                case InteractionType.banzaipyramid:
                    {
                        _room.GetGameItemHandler().AddPyramid(item, item.Id);
                        break;
                    }

                case InteractionType.banzaitele:
                    {
                        _room.GetGameItemHandler().AddTeleport(item, item.Id);
                        item.ExtraData = "";
                        break;
                    }
                case InteractionType.banzaipuck:
                    {
                        _room.GetBanzai().AddPuck(item);
                        break;
                    }

                case InteractionType.FOOTBALL:
                    {
                        _room.GetSoccer().AddBall(item);
                        break;
                    }
                case InteractionType.FREEZE_TILE_BLOCK:
                    {
                        _room.GetFreeze().AddFreezeBlock(item);
                        break;
                    }
                case InteractionType.FREEZE_TILE:
                    {
                        _room.GetFreeze().AddFreezeTile(item);
                        break;
                    }
                case InteractionType.freezeexit:
                    {
                        _room.GetFreeze().AddExitTile(item);
                        break;
                    }
            }
        }

        private void RemoveSpecialItem(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FOOTBALL_GATE:
                    _room.GetSoccer().UnRegisterGate(item);
                    break;
                case InteractionType.banzaifloor:
                    _room.GetBanzai().RemoveTile(item.Id);
                    break;
                case InteractionType.banzaipuck:
                    _room.GetBanzai().RemovePuck(item.Id);
                    break;
                case InteractionType.banzaipyramid:
                    _room.GetGameItemHandler().RemovePyramid(item.Id);
                    break;
                case InteractionType.banzaitele:
                    _room.GetGameItemHandler().RemoveTeleport(item.Id);
                    break;
                case InteractionType.FOOTBALL:
                    _room.GetSoccer().RemoveBall(item.Id);
                    break;
                case InteractionType.FREEZE_TILE:
                    _room.GetFreeze().RemoveFreezeTile(item.Id);
                    break;
                case InteractionType.FREEZE_TILE_BLOCK:
                    _room.GetFreeze().RemoveFreezeBlock(item.Id);
                    break;
                case InteractionType.freezeexit:
                    _room.GetFreeze().RemoveExitTile(item.Id);
                    break;
            }
        }

        public bool RemoveFromMap(Item item, bool handleGameItem)
        {
            if (handleGameItem)
                RemoveSpecialItem(item);

            if (_room.GotSoccer())
                _room.GetSoccer().OnGateRemove(item);

            bool isRemoved = false;
            foreach (Point coord in item.GetCoords.ToList())
            {
                if (RemoveCoordinatedItem(item, coord))
                    isRemoved = true;
            }

            ConcurrentDictionary<Point, List<Item>> items = new ConcurrentDictionary<Point, List<Item>>();
            foreach (Point Tile in item.GetCoords.ToList())
            {
                Point point = new Point(Tile.X, Tile.Y);
                if (_coordinatedItems.ContainsKey(point))
                {
                    List<int> Ids = (List<int>)_coordinatedItems[point];
                    List<Item> __items = GetItemsFromIds(Ids);

                    if (!items.ContainsKey(Tile))
                        items.TryAdd(Tile, __items);
                }

                SetDefaultValue(Tile.X, Tile.Y);
            }

            foreach (Point Coord in items.Keys.ToList())
            {
                if (!items.ContainsKey(Coord))
                    continue;

                List<Item> SubItems = (List<Item>)items[Coord];
                foreach (Item Item in SubItems.ToList())
                {
                    ConstructMapForItem(Item, Coord);
                }
            }


            items.Clear();
            items = null;


            return isRemoved;
        }

        public bool RemoveFromMap(Item item)
        {
            return RemoveFromMap(item, true);
        }

        public bool AddItemToMap(Item item, bool handleGameItem, bool newItem = true)
        {

            if (handleGameItem)
            {
                AddSpecialItems(item);

                switch (item.GetBaseItem().InteractionType)
                {
                    case InteractionType.FOOTBALL_GOAL_RED:
                    case InteractionType.footballcounterred:
                    case InteractionType.banzaiscorered:
                    case InteractionType.banzaigatered:
                    case InteractionType.freezeredcounter:
                    case InteractionType.FREEZE_RED_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(item))
                                _room.GetGameManager().AddFurnitureToTeam(item, Team.Red);
                            break;
                        }
                    case InteractionType.FOOTBALL_GOAL_GREEN:
                    case InteractionType.footballcountergreen:
                    case InteractionType.banzaiscoregreen:
                    case InteractionType.banzaigategreen:
                    case InteractionType.freezegreencounter:
                    case InteractionType.FREEZE_GREEN_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(item))
                                _room.GetGameManager().AddFurnitureToTeam(item, Team.Green);
                            break;
                        }
                    case InteractionType.FOOTBALL_GOAL_BLUE:
                    case InteractionType.footballcounterblue:
                    case InteractionType.banzaiscoreblue:
                    case InteractionType.banzaigateblue:
                    case InteractionType.freezebluecounter:
                    case InteractionType.FREEZE_BLUE_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(item))
                                _room.GetGameManager().AddFurnitureToTeam(item, Team.Blue);
                            break;
                        }
                    case InteractionType.FOOTBALL_GOAL_YELLOW:
                    case InteractionType.footballcounteryellow:
                    case InteractionType.banzaiscoreyellow:
                    case InteractionType.banzaigateyellow:
                    case InteractionType.freezeyellowcounter:
                    case InteractionType.FREEZE_YELLOW_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(item))
                                _room.GetGameManager().AddFurnitureToTeam(item, Team.Yellow);
                            break;
                        }
                    case InteractionType.freezeexit:
                        {
                            _room.GetFreeze().AddExitTile(item);
                            break;
                        }
                    case InteractionType.ROLLER:
                        {
                            if (!_room.GetRoomItemHandler().GetRollers().Contains(item))
                                _room.GetRoomItemHandler().TryAddRoller(item.Id, item);
                            break;
                        }
                }
            }

            if (item.GetBaseItem().Type != 's')
                return true;

            foreach (Point coord in item.GetCoords.ToList())
            {
                AddCoordinatedItem(item, new Point(coord.X, coord.Y));
            }

            if (item.GetX > (Model.MapSizeX - 1))
            {
                Model.AddX();
                GenerateMaps();
                return false;
            }

            if (item.GetY > (Model.MapSizeY - 1))
            {
                Model.AddY();
                GenerateMaps();
                return false;
            }

            bool @return = true;

            foreach (Point coord in item.GetCoords)
            {
                if (!ConstructMapForItem(item, coord))
                {
                    @return = false;
                }
                else
                {
                    @return = true;
                }
            }



            return @return;
        }


        public bool CanWalk(int x, int y, bool @override)
        {

            if (@override)
            {
                return true;
            }

            if (_room.GetRoomUserManager().GetUserForSquare(x, y) != null && _room.RoomBlockingEnabled == 0)
                return false;

            return true;
        }

        public bool AddItemToMap(Item Item, bool NewItem = true)
        {
            return AddItemToMap(Item, true, NewItem);
        }

        public bool ItemCanMove(Item Item, Point MoveTo)
        {
            List<ThreeDCoord> Points = GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, MoveTo.X, MoveTo.Y, Item.Rotation).Values.ToList();

            if (Points == null || Points.Count == 0)
                return true;

            foreach (ThreeDCoord Coord in Points)
            {

                if (Coord.X >= Model.MapSizeX || Coord.Y >= Model.MapSizeY)
                    return false;

                if (!SquareIsOpen(Coord.X, Coord.Y, false))
                    return false;

                continue;
            }

            return true;
        }

        public byte GetFloorStatus(Point coord)
        {
            if (coord.X > GameMap.GetUpperBound(0) || coord.Y > GameMap.GetUpperBound(1))
                return 1;

            return GameMap[coord.X, coord.Y];
        }

        public void SetFloorStatus(int X, int Y, byte Status)
        {
            GameMap[X, Y] = Status;
        }

        public double GetHeightForSquareFromData(Point coord)
        {
            if (coord.X > Model.SqFloorHeight.GetUpperBound(0) ||
                coord.Y > Model.SqFloorHeight.GetUpperBound(1))
                return 1;
            return Model.SqFloorHeight[coord.X, coord.Y];
        }

        public bool CanRollItemHere(int x, int y)
        {
            if (!ValidTile(x, y))
                return false;

            if (Model.SqState[x, y] == SquareState.Blocked)
                return false;

            return true;
        }

        public bool SquareIsOpen(int x, int y, bool pOverride)
        {
            if ((Model.MapSizeX - 1) < x || (Model.MapSizeY - 1) < y)
                return false;

            return CanWalk(GameMap[x, y], pOverride);
        }

        public bool GetHighestItemForSquare(Point Square, out Item Item)
        {
            List<Item> Items = GetAllRoomItemForSquare(Square.X, Square.Y);
            Item = null;
            double HighestZ = -1;

            if (Items != null && Items.Count() > 0)
            {
                foreach (Item uItem in Items.ToList())
                {
                    if (uItem == null)
                        continue;

                    if (uItem.TotalHeight > HighestZ)
                    {
                        HighestZ = uItem.TotalHeight;
                        Item = uItem;
                        continue;
                    }
                    else
                        continue;
                }
            }
            else
                return false;

            return true;
        }

        public double GetHeightForSquare(Point coord)
        {
            if (GetHighestItemForSquare(coord, out Item rItem))
                if (rItem != null)
                    return rItem.TotalHeight;

            return 0.0;
        }

        public Point GetChaseMovement(Item item)
        {
            int Distance = 99;
            Point Coord = new Point(0, 0);
            int iX = item.GetX;
            int iY = item.GetY;
            bool X = false;

            foreach (RoomUser User in _room.GetRoomUserManager().GetRoomUsers())
            {
                if (User.X == item.GetX || item.GetY == User.Y)
                {
                    if (User.X == item.GetX)
                    {
                        int Difference = Math.Abs(User.Y - item.GetY);
                        if (Difference < Distance)
                        {
                            Distance = Difference;
                            Coord = User.Coordinate;
                            X = false;
                        }
                        else
                            continue;

                    }
                    else if (User.Y == item.GetY)
                    {
                        int Difference = Math.Abs(User.X - item.GetX);
                        if (Difference < Distance)
                        {
                            Distance = Difference;
                            Coord = User.Coordinate;
                            X = true;
                        }
                        else
                            continue;
                    }
                    else
                        continue;
                }
            }

            if (Distance > 5)
                return item.GetSides().OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            if (X && Distance < 99)
            {
                if (iX > Coord.X)
                {
                    iX--;
                    return new Point(iX, iY);
                }
                else
                {
                    iX++;
                    return new Point(iX, iY);
                }
            }
            else if (!X && Distance < 99)
            {
                if (iY > Coord.Y)
                {
                    iY--;
                    return new Point(iX, iY);
                }
                else
                {
                    iY++;
                    return new Point(iX, iY);
                }
            }
            else
                return item.Coordinate;
        }

        public bool IsValidStep2(RoomUser User, Vector2D From, Vector2D To, bool EndOfPath, bool Override)
        {
            if (User == null)
                return false;

            if (!ValidTile(To.X, To.Y))
                return false;

            if (Override)
                return true;

            /*
             * 0 = blocked
             * 1 = open
             * 2 = last step
             * 3 = door
             * */

            List<Item> Items = _room.GetGameMap().GetAllRoomItemForSquare(To.X, To.Y);
            if (Items.Count > 0)
            {
                bool HasGroupGate = Items.ToList().Count(x => x.GetBaseItem().InteractionType == InteractionType.GUILD_GATE) > 0;
                if (HasGroupGate)
                {
                    Item I = Items.FirstOrDefault(x => x.GetBaseItem().InteractionType == InteractionType.GUILD_GATE);
                    if (I != null)
                    {
                        if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(I.GroupId, out Group Group))
                            return false;

                        if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                            return false;

                        if (Group.IsMember(User.GetClient().GetHabbo().Id))
                        {
                            I.InteractingUser = User.GetClient().GetHabbo().Id;
                            I.ExtraData = "1";
                            I.UpdateState(false, true);

                            I.RequestUpdate(4, true);

                            return true;
                        }
                        else
                        {
                            if (User.Path.Count > 0)
                                User.Path.Clear();
                            User.PathRecalcNeeded = false;
                            return false;
                        }
                    }
                }
            }

            bool Chair = false;
            double HighestZ = -1;
            foreach (Item Item in Items.ToList())
            {
                if (Item == null)
                    continue;

                if (Item.GetZ < HighestZ)
                {
                    Chair = false;
                    continue;
                }

                HighestZ = Item.GetZ;
                if (Item.GetBaseItem().IsSeat)
                    Chair = true;
            }

            if ((GameMap[To.X, To.Y] == 3 && !EndOfPath && !Chair) || (GameMap[To.X, To.Y] == 0) || (GameMap[To.X, To.Y] == 2 && !EndOfPath))
            {
                if (User.Path.Count > 0)
                    User.Path.Clear();
                User.PathRecalcNeeded = true;
            }

            double HeightDiff = SqAbsoluteHeight(To.X, To.Y) - SqAbsoluteHeight(From.X, From.Y);
            if (HeightDiff > 1.5 && !User.RidingHorse)
                return false;

            //Check this last, because ya.
            RoomUser Userx = _room.GetRoomUserManager().GetUserForSquare(To.X, To.Y);
            if (Userx != null)
            {
                if (!Userx.IsWalking && EndOfPath)
                    return false;
            }
            return true;
        }
    
        public bool IsValidStep(Vector2D from, Vector2D to, bool endOfPath, bool overriding, bool roller = false)
        {
            if (!ValidTile(to.X, to.Y))
                return false;

            if (overriding)
                return true;

            /*
             * 0 = blocked
             * 1 = open
             * 2 = last step
             * 3 = door
             * */

            if (_room.RoomBlockingEnabled == 0 && SquareHasUsers(to.X, to.Y))
                return false;

            List<Item> items = _room.GetGameMap().GetAllRoomItemForSquare(to.X, to.Y);
            if (items.Count > 0)
            {
                bool HasGroupGate = items.ToList().Count(x => x != null && x.GetBaseItem().InteractionType == InteractionType.GUILD_GATE) > 0;
                if (HasGroupGate)
                    return true;
            }

            if ((GameMap[to.X, to.Y] == 3 && !endOfPath) || GameMap[to.X, to.Y] == 0 || (GameMap[to.X, to.Y] == 2 && !endOfPath))
                return false;

            if (!roller)
            {
                double HeightDiff = SqAbsoluteHeight(to.X, to.Y) - SqAbsoluteHeight(from.X, from.Y);
                if (HeightDiff > 1.5)
                    return false;
            }

            return true;
        }

        public static bool CanWalk(byte state, bool overriding)
        {
            if (!overriding)
            {
                if (state == 3)
                    return true;
                if (state == 1)
                    return true;

                return false;
            }
            return true;
        }

        public bool ItemCanBePlaced(int x, int y)
        {
            if (Model.MapSizeX - 1 < x || Model.MapSizeY - 1 < y ||
                (x == Model.DoorX && y == Model.DoorY))
                return false;

            return GameMap[x, y] == 1;
        }

        public double SqAbsoluteHeight(int x, int y)
        {
            Point Points = new Point(x, y);


            if (_coordinatedItems.TryGetValue(Points, out List<int> Ids))
            {
                List<Item> Items = GetItemsFromIds(Ids);

                return SqAbsoluteHeight(x, y, Items);
            }
            else
                return Model.SqFloorHeight[x, y];

            #region Old
            /*
            if (mCoordinatedItems.ContainsKey(Points))
            {
                List<Item> Items = new List<Item>();
                foreach (Item Item in mCoordinatedItems[Points].ToArray())
                {
                    if (!Items.Contains(Item))
                        Items.Add(Item);
                }
                return SqAbsoluteHeight(X, Y, Items);
            }*/
            #endregion
        }

        public double SqAbsoluteHeight(int X, int Y, List<Item> ItemsOnSquare)
        {
            try
            {
                bool deduct = false;
                double HighestStack = 0;
                double deductable = 0.0;

                if (ItemsOnSquare != null && ItemsOnSquare.Count > 0)
                {
                    foreach (Item Item in ItemsOnSquare.ToList())
                    {
                        if (Item == null)
                            continue;

                        if (Item.TotalHeight > HighestStack)
                        {
                            if (Item.GetBaseItem().IsSeat || Item.GetBaseItem().InteractionType == InteractionType.BED || Item.GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
                            {
                                deduct = true;
                                deductable = Item.GetBaseItem().Height;
                            }
                            else
                                deduct = false;
                            HighestStack = Item.TotalHeight;
                        }
                    }
                }

                double floorHeight = Model.SqFloorHeight[X, Y];
                double stackHeight = HighestStack - Model.SqFloorHeight[X, Y];

                if (deduct)
                    stackHeight -= deductable;

                if (stackHeight < 0)
                    stackHeight = 0;

                return (floorHeight + stackHeight);
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
                return 0;
            }
        }

        public bool ValidTile(int X, int Y)
        {
            if (X < 0 || Y < 0 || X >= Model.MapSizeX || Y >= Model.MapSizeY)
            {
                return false;
            }

            return true;
        }

        public static Dictionary<int, ThreeDCoord> GetAffectedTiles(int Length, int Width, int PosX, int PosY, int Rotation)
        {
            int x = 0;

            var PointList = new Dictionary<int, ThreeDCoord>();

            if (Length > 1)
            {
                if (Rotation == 0 || Rotation == 4)
                {
                    for (int i = 1; i < Length; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX, PosY + i, i));

                        for (int j = 1; j < Width; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + j, PosY + i, (i < j) ? j : i));
                        }
                    }
                }
                else if (Rotation == 2 || Rotation == 6)
                {
                    for (int i = 1; i < Length; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX + i, PosY, i));

                        for (int j = 1; j < Width; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + i, PosY + j, (i < j) ? j : i));
                        }
                    }
                }
            }

            if (Width > 1)
            {
                if (Rotation == 0 || Rotation == 4)
                {
                    for (int i = 1; i < Width; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX + i, PosY, i));

                        for (int j = 1; j < Length; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + i, PosY + j, (i < j) ? j : i));
                        }
                    }
                }
                else if (Rotation == 2 || Rotation == 6)
                {
                    for (int i = 1; i < Width; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX, PosY + i, i));

                        for (int j = 1; j < Length; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + j, PosY + i, (i < j) ? j : i));
                        }
                    }
                }
            }

            return PointList;
        }

        public List<Item> GetItemsFromIds(List<int> Input)
        {
            if (Input == null || Input.Count == 0)
                return new List<Item>();

            List<Item> Items = new List<Item>();

            lock (Input)
            {
                foreach (int Id in Input.ToList())
                {
                    Item Itm = _room.GetRoomItemHandler().GetItem(Id);
                    if (Itm != null && !Items.Contains(Itm))
                        Items.Add(Itm);
                }
            }

            return Items.ToList();
        }

        public List<Item> GetRoomItemForSquare(int pX, int pY, double minZ)
        {
            var itemsToReturn = new List<Item>();


            var coord = new Point(pX, pY);
            if (_coordinatedItems.ContainsKey(coord))
            {
                var itemsFromSquare = GetItemsFromIds((List<int>)_coordinatedItems[coord]);

                foreach (Item item in itemsFromSquare)
                    if (item.GetZ > minZ)
                        if (item.GetX == pX && item.GetY == pY)
                            itemsToReturn.Add(item);
            }

            return itemsToReturn;
        }

        public List<Item> GetRoomItemForSquare(int pX, int pY)
        {
            var coord = new Point(pX, pY);
            //List<RoomItem> itemsFromSquare = new List<RoomItem>();
            var itemsToReturn = new List<Item>();

            if (_coordinatedItems.ContainsKey(coord))
            {
                var itemsFromSquare = GetItemsFromIds((List<int>)_coordinatedItems[coord]);

                foreach (Item item in itemsFromSquare)
                {
                    if (item.Coordinate.X == coord.X && item.Coordinate.Y == coord.Y)
                        itemsToReturn.Add(item);
                }
            }

            return itemsToReturn;
        }

        public List<Item> GetAllRoomItemForSquare(int x, int y)
        {
            Point coord = new Point(x, y);

            List<Item> items = new List<Item>();


            if (_coordinatedItems.TryGetValue(coord, out List<int> Ids))
                items = GetItemsFromIds(Ids);
            else
                items = new List<Item>();

            return items;
        }

        public bool SquareHasUsers(int X, int Y)
        {
            return MapGotUser(new Point(X, Y));
        }


        public static bool TilesTouching(int X1, int Y1, int X2, int Y2)
        {
            if (!(Math.Abs(X1 - X2) > 1 || Math.Abs(Y1 - Y2) > 1)) return true;
            if (X1 == X2 && Y1 == Y2) return true;
            return false;
        }

        public static int TileDistance(int X1, int Y1, int X2, int Y2)
        {
            return Math.Abs(X1 - X2) + Math.Abs(Y1 - Y2);
        }

        public DynamicRoomModel Model { get; private set; }

        public RoomModel StaticModel { get; private set; }

        public byte[,] EffectMap { get; private set; }

        public byte[,] GameMap { get; private set; }

        public void Dispose()
        {
            _userMap.Clear();
            Model.Destroy();
            _coordinatedItems.Clear();

            Array.Clear(GameMap, 0, GameMap.Length);
            Array.Clear(EffectMap, 0, EffectMap.Length);
            Array.Clear(_itemHeightMap, 0, _itemHeightMap.Length);

            _userMap = null;
            GameMap = null;
            EffectMap = null;
            _itemHeightMap = null;
            _coordinatedItems = null;

            Model = null;
            _room = null;
            StaticModel = null;
        }
    }
}