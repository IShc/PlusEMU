using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Core;
using Plus.HabboHotel.Items.Interactor;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Games.Freeze;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Rooms.PathFinding;
using Plus.Utilities;

namespace Plus.HabboHotel.Items
{

    public class Item
    {
        public int Id;
        public int BaseItem;
        public string ExtraData;
        public string Figure;
        public string Gender;
        public int GroupId;
        public int InteractingUser;
        public int InteractingUser2;
        public int LimitedNo;
        public int LimitedTot;
        public bool MagicRemove = false;
        public int RoomId;
        public int Rotation;
        public int UpdateCounter;
        public int UserId;
        public string Username;
        public int InteractingBallUser;
        public byte InteractionCount;
        public byte InteractionCountHelper;

        public Team Team;
        public bool PendingReset = false;
        public FreezePowerUp FreezePowerUp;
        
        public int Value;
        public string WallCoord;
        private bool _updateNeeded;

        private Room _room;
        private static Random _random = new();

        public Item(int id, int roomId, int baseItem, string extraData, int x, int y, double z, int rot, int userid, int group, int limitedNumber, int limitedStack, string wallCoord, Room room = null)
        {
            if (PlusEnvironment.GetGame().GetItemManager().GetItem(baseItem, out ItemData data))
            {
                Id = id;
                RoomId = roomId;
                _room = room;
                Data = data;
                BaseItem = baseItem;
                ExtraData = extraData;
                GroupId = group;

                GetX = x;
                GetY = y;
                if (!double.IsInfinity(z))
                    GetZ = z;
                Rotation = rot;
                UpdateNeeded = false;
                UpdateCounter = 0;
                InteractingUser = 0;
                InteractingUser2 = 0;
                InteractingBallUser = 0;
                InteractionCount = 0;
                Value = 0;

                UserId = userid;
                Username = PlusEnvironment.GetUsernameById(userid);
                
                LimitedNo = limitedNumber;
                LimitedTot = limitedStack;

                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.TELEPORT:
                        RequestUpdate(0, true);
                        break;

                    case InteractionType.HOPPER:
                        RequestUpdate(0, true);
                        break;

                    case InteractionType.ROLLER:
                        IsRoller = true;
                        if (roomId > 0)
                        {
                            GetRoom().GetRoomItemHandler().GotRollers = true;
                        }
                        break;

                    case InteractionType.banzaiscoreblue:
                    case InteractionType.footballcounterblue:
                    case InteractionType.banzaigateblue:
                    case InteractionType.FREEZE_BLUE_GATE:
                    case InteractionType.freezebluecounter:
                        Team = Team.Blue;
                        break;

                    case InteractionType.banzaiscoregreen:
                    case InteractionType.footballcountergreen:
                    case InteractionType.banzaigategreen:
                    case InteractionType.freezegreencounter:
                    case InteractionType.FREEZE_GREEN_GATE:
                        Team = Team.Green;
                        break;

                    case InteractionType.banzaiscorered:
                    case InteractionType.footballcounterred:
                    case InteractionType.banzaigatered:
                    case InteractionType.freezeredcounter:
                    case InteractionType.FREEZE_RED_GATE:
                        Team = Team.Red;
                        break;

                    case InteractionType.banzaiscoreyellow:
                    case InteractionType.footballcounteryellow:
                    case InteractionType.banzaigateyellow:
                    case InteractionType.freezeyellowcounter:
                    case InteractionType.FREEZE_YELLOW_GATE:
                        Team = Team.Yellow;
                        break;

                    case InteractionType.banzaitele:
                        {
                            ExtraData = "";
                            break;
                        }
                }

                IsWallItem = (GetBaseItem().Type.ToString().ToLower() == "i");
                IsFloorItem = (GetBaseItem().Type.ToString().ToLower() == "s");

                if (IsFloorItem)
                {
                    GetAffectedTiles = Gamemap.GetAffectedTiles(GetBaseItem().Length, GetBaseItem().Width, GetX, GetY, rot);
                }
                else if (IsWallItem)
                {
                    WallCoord = wallCoord;
                    IsWallItem = true;
                    IsFloorItem = false;
                    GetAffectedTiles = new Dictionary<int, ThreeDCoord>();
                }
            }
        }

        public ItemData Data { get; set; }

        public Dictionary<int, ThreeDCoord> GetAffectedTiles { get; private set; }

        public int GetX { get; set; }

        public int GetY { get; set; }

        public double GetZ { get; set; }

        public bool UpdateNeeded
        {
            get => _updateNeeded;
            set
            {
                if (value && GetRoom() != null)
                    GetRoom().GetRoomItemHandler().QueueRoomItemUpdate(this);
                _updateNeeded = value;
            }
        }

        public bool IsRoller { get; }

        public Point Coordinate => new Point(GetX, GetY);

        public List<Point> GetCoords
        {
            get
            {
                var toReturn = new List<Point>
                {
                    Coordinate
                };

                foreach (ThreeDCoord tile in GetAffectedTiles.Values)
                {
                    toReturn.Add(new Point(tile.X, tile.Y));
                }

                return toReturn;
            }
        }

        public List<Point> GetSides()
        {
            var sides = new List<Point>
            {
                SquareBehind,
                SquareInFront,
                SquareLeft,
                SquareRight,
                Coordinate
            };
            return sides;
        }

        public double TotalHeight
        {
            get
            {
                double curHeight = 0.0;

                if (GetBaseItem().AdjustableHeights.Count > 1)
                {
                    if (int.TryParse(ExtraData, out int num2) && (GetBaseItem().AdjustableHeights.Count) - 1 >= num2)
                        curHeight = GetZ + GetBaseItem().AdjustableHeights[num2];
                }

                if (curHeight <= 0.0)
                    curHeight = GetZ + GetBaseItem().Height;

                return curHeight;
            }
        }

        public bool IsWallItem { get; }

        public bool IsFloorItem { get; }

        public Point SquareInFront
        {
            get
            {
                var sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    sq.Y--;
                }
                else if (Rotation == 2)
                {
                    sq.X++;
                }
                else if (Rotation == 4)
                {
                    sq.Y++;
                }
                else if (Rotation == 6)
                {
                    sq.X--;
                }

                return sq;
            }
        }

        public Point SquareBehind
        {
            get
            {
                var sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    sq.Y++;
                }
                else if (Rotation == 2)
                {
                    sq.X--;
                }
                else if (Rotation == 4)
                {
                    sq.Y--;
                }
                else if (Rotation == 6)
                {
                    sq.X++;
                }

                return sq;
            }
        }

        public Point SquareLeft
        {
            get
            {
                var sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    sq.X++;
                }
                else if (Rotation == 2)
                {
                    sq.Y--;
                }
                else if (Rotation == 4)
                {
                    sq.X--;
                }
                else if (Rotation == 6)
                {
                    sq.Y++;
                }

                return sq;
            }
        }

        public Point SquareRight
        {
            get
            {
                var sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    sq.X--;
                }
                else if (Rotation == 2)
                {
                    sq.Y++;
                }
                else if (Rotation == 4)
                {
                    sq.X++;
                }
                else if (Rotation == 6)
                {
                    sq.Y--;
                }
                return sq;
            }
        }

        public IFurniInteractor Interactor
        {
            get
            {
                if (IsWired)
                {
                    return new InteractorWired();
                }

                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.GATE:
                        return new InteractorGate();

                    case InteractionType.TELEPORT:
                        return new InteractorTeleport();

                    case InteractionType.HOPPER:
                        return new InteractorHopper();

                    case InteractionType.BOTTLE:
                        return new InteractorSpinningBottle();

                    case InteractionType.DICE:
                        return new InteractorDice();

                    case InteractionType.HABBO_WHEEL:
                        return new InteractorHabboWheel();

                    case InteractionType.LOVE_SHUFFLER:
                        return new InteractorLoveShuffler();

                    case InteractionType.ONE_WAY_GATE:
                        return new InteractorOneWayGate();

                    case InteractionType.ALERT:
                        return new InteractorAlert();

                    case InteractionType.VENDING_MACHINE:
                        return new InteractorVendor();

                    case InteractionType.SCOREBOARD:
                        return new InteractorScoreboard();

                    case InteractionType.PUZZLE_BOX:
                        return new InteractorPuzzleBox();

                    case InteractionType.MANNEQUIN:
                        return new InteractorMannequin();

                    case InteractionType.banzaicounter:
                        return new InteractorBanzaiTimer();

                    case InteractionType.freezetimer:
                        return new InteractorFreezeTimer();

                    case InteractionType.FREEZE_TILE_BLOCK:
                    case InteractionType.FREEZE_TILE:
                        return new InteractorFreezeTile();

                    case InteractionType.footballcounterblue:
                    case InteractionType.footballcountergreen:
                    case InteractionType.footballcounterred:
                    case InteractionType.footballcounteryellow:
                        return new InteractorScoreCounter();

                    case InteractionType.banzaiscoreblue:
                    case InteractionType.banzaiscoregreen:
                    case InteractionType.banzaiscorered:
                    case InteractionType.banzaiscoreyellow:
                        return new InteractorBanzaiScoreCounter();

                    case InteractionType.WF_FLOOR_SWITCH_1:
                    case InteractionType.WF_FLOOR_SWITCH_2:
                        return new InteractorSwitch();

                    case InteractionType.LOVELOCK:
                        return new InteractorLoveLock();

                    case InteractionType.CANNON:
                        return new InteractorCannon();

                    case InteractionType.COUNTER:
                        return new InteractorCounter();

                    case InteractionType.NONE:
                    default:
                        return new InteractorGenericSwitch();
                }
            }
        }

        public bool IsWired
        {
            get
            {
                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.WIRED_EFFECT:
                    case InteractionType.WIRED_TRIGGER:
                    case InteractionType.WIRED_CONDITION:
                        return true;
                }

                return false;
            }
        }

        public void SetState(int pX, int pY, double pZ, Dictionary<int, ThreeDCoord> tiles)
        {
            GetX = pX;
            GetY = pY;
            if (!double.IsInfinity(pZ))
            {
                GetZ = pZ;
            }
            GetAffectedTiles = tiles;
        }

        public void ProcessUpdates()
        {
            try
            {
                UpdateCounter--;

                if (UpdateCounter <= 0)
                {
                    UpdateNeeded = false;
                    UpdateCounter = 0;

                    RoomUser user = null;
                    RoomUser user2 = null;

                    switch (GetBaseItem().InteractionType)
                    {
                        #region Group Gates
                        case InteractionType.GUILD_GATE:
                            {
                                if (ExtraData == "1")
                                {
                                    if (GetRoom().GetRoomUserManager().GetUserForSquare(GetX, GetY) == null)
                                    {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    }
                                    else
                                    {
                                        RequestUpdate(2, false);
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region Item Effects
                        case InteractionType.EFFECT:
                            {
                                if (ExtraData == "1")
                                {
                                    if (GetRoom().GetRoomUserManager().GetUserForSquare(GetX, GetY) == null)
                                    {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    }
                                    else
                                    {
                                        RequestUpdate(2, false);
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region One way gates
                        case InteractionType.ONE_WAY_GATE:

                            user = null;

                            if (InteractingUser > 0)
                            {
                                user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            }

                            if (user != null && user.X == GetX && user.Y == GetY)
                            {
                                ExtraData = "1";

                                user.MoveTo(SquareBehind);
                                user.InteractingGate = false;
                                user.GateId = 0;
                                RequestUpdate(1, false);
                                UpdateState(false, true);
                            }
                            else if (user != null && user.Coordinate == SquareBehind)
                            {
                                user.UnlockWalking();

                                ExtraData = "0";
                                InteractingUser = 0;
                                user.InteractingGate = false;
                                user.GateId = 0;
                                UpdateState(false, true);
                            }
                            else if (ExtraData == "1")
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }

                            if (user == null)
                            {
                                InteractingUser = 0;
                            }

                            break;
                        #endregion

                        #region VIP Gate
                        case InteractionType.GATE_VIP:

                            user = null;


                            if (InteractingUser > 0)
                            {
                                user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            }

                            int newY = 0;
                            int newX = 0;

                            if (user != null && user.X == GetX && user.Y == GetY)
                            {
                                if (user.RotBody == 4)
                                {
                                    newY = 1;
                                }
                                else if (user.RotBody == 0)
                                {
                                    newY = -1;
                                }
                                else if (user.RotBody == 6)
                                {
                                    newX = -1;
                                }
                                else if (user.RotBody == 2)
                                {
                                    newX = 1;
                                }


                                user.MoveTo(user.X + newX, user.Y + newY);
                                RequestUpdate(1, false);
                            }
                            else if (user != null && (user.Coordinate == SquareBehind || user.Coordinate == SquareInFront))
                            {
                                user.UnlockWalking();

                                ExtraData = "0";
                                InteractingUser = 0;
                                UpdateState(false, true);
                            }
                            else if (ExtraData == "1")
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }

                            if (user == null)
                            {
                                InteractingUser = 0;
                            }

                            break;
                        #endregion

                        #region Hopper
                        case InteractionType.HOPPER:
                            {
                                user = null;
                                user2 = null;
                                bool showHopperEffect = false;
                                bool keepDoorOpen = false;
                                int pause = 0;


                                // Do we have a primary user that wants to go somewhere?
                                if (InteractingUser > 0)
                                {
                                    user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                                    // Is this user okay?
                                    if (user != null)
                                    {
                                        // Is he in the tele?
                                        if (user.Coordinate == Coordinate)
                                        {
                                            //Remove the user from the square
                                            user.AllowOverride = false;
                                            if (user.TeleDelay == 0)
                                            {
                                                int roomHopId = ItemHopperFinder.GetAHopper(user.RoomId);
                                                int nextHopperId = ItemHopperFinder.GetHopperId(roomHopId);

                                                if (!user.IsBot && user != null && user.GetClient() != null &&
                                                    user.GetClient().GetHabbo() != null)
                                                {
                                                    user.GetClient().GetHabbo().IsHopping = true;
                                                    user.GetClient().GetHabbo().HopperId = nextHopperId;
                                                    user.GetClient().GetHabbo().PrepareRoom(roomHopId, "");
                                                    //User.GetClient().SendMessage(new RoomForwardComposer(RoomHopId));
                                                    InteractingUser = 0;
                                                }
                                            }
                                            else
                                            {
                                                user.TeleDelay--;
                                                showHopperEffect = true;
                                            }
                                        }
                                        // Is he in front of the tele?
                                        else if (user.Coordinate == SquareInFront)
                                        {
                                            user.AllowOverride = true;
                                            keepDoorOpen = true;

                                            // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                            if (user.IsWalking && (user.GoalX != GetX || user.GoalY != GetY))
                                            {
                                                user.ClearMovement(true);
                                            }

                                            user.CanWalk = false;
                                            user.AllowOverride = true;

                                            // Move into the tele
                                            user.MoveTo(Coordinate.X, Coordinate.Y, true);
                                        }
                                        // Not even near, do nothing and move on for the next user.
                                        else
                                        {
                                            InteractingUser = 0;
                                        }
                                    }
                                    else
                                    {
                                        // Invalid user, do nothing and move on for the next user. 
                                        InteractingUser = 0;
                                    }
                                }

                                if (InteractingUser2 > 0)
                                {
                                    user2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                                    // Is this user okay?
                                    if (user2 != null)
                                    {
                                        // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                        keepDoorOpen = true;
                                        user2.UnlockWalking();
                                        user2.MoveTo(SquareInFront);
                                    }

                                    // This is a one time thing, whether the user's valid or not.
                                    InteractingUser2 = 0;
                                }

                                // Set the new item state, by priority
                                if (keepDoorOpen)
                                {
                                    if (ExtraData != "1")
                                    {
                                        ExtraData = "1";
                                        UpdateState(false, true);
                                    }
                                }
                                else if (showHopperEffect)
                                {
                                    if (ExtraData != "2")
                                    {
                                        ExtraData = "2";
                                        UpdateState(false, true);
                                    }
                                }
                                else
                                {
                                    if (ExtraData != "0")
                                    {
                                        if (pause == 0)
                                        {
                                            ExtraData = "0";
                                            UpdateState(false, true);
                                            pause = 2;
                                        }
                                        else
                                        {
                                            pause--;
                                        }
                                    }
                                }

                                // We're constantly going!
                                RequestUpdate(1, false);
                                break;
                            }
                        #endregion

                        #region Teleports
                        case InteractionType.TELEPORT:
                            {
                                user = null;
                                user2 = null;

                                bool keepDoorOpen = false;
                                bool showTeleEffect = false;

                                // Do we have a primary user that wants to go somewhere?
                                if (InteractingUser > 0)
                                {
                                    user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                                    // Is this user okay?
                                    if (user != null)
                                    {
                                        // Is he in the tele?
                                        if (user.Coordinate == Coordinate)
                                        {
                                            //Remove the user from the square
                                            user.AllowOverride = false;

                                            if (ItemTeleporterFinder.IsTeleLinked(Id, GetRoom()))
                                            {
                                                showTeleEffect = true;

                                                if (true)
                                                {
                                                    // Woop! No more delay.
                                                    int teleId = ItemTeleporterFinder.GetLinkedTele(Id);
                                                    int roomId = ItemTeleporterFinder.GetTeleRoomId(teleId, GetRoom());

                                                    // Do we need to tele to the same room or gtf to another?
                                                    if (roomId == RoomId)
                                                    {
                                                        Item item = GetRoom().GetRoomItemHandler().GetItem(teleId);

                                                        if (item == null)
                                                        {
                                                            user.UnlockWalking();
                                                        }
                                                        else
                                                        {
                                                            // Set pos
                                                            user.SetPos(item.GetX, item.GetY, item.GetZ);
                                                            user.SetRot(item.Rotation, false);

                                                            // Force tele effect update (dirty)
                                                            item.ExtraData = "2";
                                                            item.UpdateState(false, true);

                                                            // Set secondary interacting user
                                                            item.InteractingUser2 = InteractingUser;
                                                            GetRoom().GetGameMap().RemoveUserFromMap(user, new Point(GetX, GetY));

                                                            InteractingUser = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (user.TeleDelay == 0)
                                                        {
                                                            // Let's run the teleport delegate to take futher care of this.. WHY DARIO?!
                                                            if (!user.IsBot && user != null && user.GetClient() != null &&
                                                                user.GetClient().GetHabbo() != null)
                                                            {
                                                                user.GetClient().GetHabbo().IsTeleporting = true;
                                                                user.GetClient().GetHabbo().TeleportingRoomId = roomId;
                                                                user.GetClient().GetHabbo().TeleportId = teleId;
                                                                user.GetClient().GetHabbo().PrepareRoom(roomId, "");
                                                                //User.GetClient().SendMessage(new RoomForwardComposer(RoomId));
                                                                InteractingUser = 0;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            user.TeleDelay--;
                                                            showTeleEffect = true;
                                                        }
                                                        //PlusEnvironment.GetGame().GetRoomManager().AddTeleAction(new TeleUserData(User.GetClient().GetMessageHandler(), User.GetClient().GetHabbo(), RoomId, TeleId));
                                                    }
                                                    GetRoom().GetGameMap().GenerateMaps();
                                                    // We're done with this tele. We have another one to bother.
                                                }
                                                else
                                                {
                                                    // We're linked, but there's a delay, so decrease the delay and wait it out.
                                                    //User.TeleDelay--;
                                                }
                                            }
                                            else
                                            {
                                                // This tele is not linked, so let's gtfo.
                                                user.UnlockWalking();
                                                InteractingUser = 0;
                                            }
                                        }
                                        // Is he in front of the tele?
                                        else if (user.Coordinate == SquareInFront)
                                        {
                                                user.AllowOverride = true;
                                                // Open the door
                                                keepDoorOpen = true;

                                                // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                                if (user.IsWalking && (user.GoalX != GetX || user.GoalY != GetY))
                                                {
                                                    user.ClearMovement(true);
                                                }

                                                user.CanWalk = false;
                                                user.AllowOverride = true;

                                                // Move into the tele
                                                user.MoveTo(Coordinate.X, Coordinate.Y, true);
                                            
                                        }
                                        // Not even near, do nothing and move on for the next user.
                                        else
                                        {
                                            InteractingUser = 0;
                                        }
                                    }
                                    else
                                    {
                                        // Invalid user, do nothing and move on for the next user. 
                                        InteractingUser = 0;
                                    }
                                }

                                // Do we have a secondary user that wants to get out of the tele?
                                if (InteractingUser2 > 0)
                                {
                                    user2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                                    // Is this user okay?
                                    if (user2 != null)
                                    {
                                        // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                        keepDoorOpen = true;
                                        user2.UnlockWalking();
                                        user2.MoveTo(SquareInFront);
                                    }

                                    // This is a one time thing, whether the user's valid or not.
                                    InteractingUser2 = 0;
                                }

                                // Set the new item state, by priority
                                if (showTeleEffect)
                                {
                                    if (ExtraData != "2")
                                    {
                                        ExtraData = "2";
                                        UpdateState(false, true);
                                    }
                                }
                                else if (keepDoorOpen)
                                {
                                    if (ExtraData != "1")
                                    {
                                        ExtraData = "1";
                                        UpdateState(false, true);
                                    }
                                }
                                else
                                {
                                    if (ExtraData != "0")
                                    {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    }
                                }

                                // We're constantly going!
                                RequestUpdate(1, false);
                                break;
                            }
                        #endregion

                        #region Bottle
                        case InteractionType.BOTTLE:
                            ExtraData = RandomNumber.GenerateNewRandom(0, 7).ToString();
                            UpdateState();
                            break;
                        #endregion

                        #region Dice
                        case InteractionType.DICE:
                            {
                                string[] numbers = new string[] { "1", "2", "3", "4", "5", "6" };
                                if (ExtraData == "-1")
                                    ExtraData = RandomizeStrings(numbers)[0];
                                UpdateState();
                            }
                            break;
                        #endregion

                        #region Habbo Wheel
                        case InteractionType.HABBO_WHEEL:
                            ExtraData = RandomNumber.GenerateRandom(1, 10).ToString();
                            UpdateState();
                            break;
                        #endregion

                        #region Love Shuffler
                        case InteractionType.LOVE_SHUFFLER:

                            if (ExtraData == "0")
                            {
                                ExtraData = RandomNumber.GenerateNewRandom(1, 4).ToString();
                                RequestUpdate(20, false);
                            }
                            else if (ExtraData != "-1")
                            {
                                ExtraData = "-1";
                            }

                            UpdateState(false, true);
                            break;
                        #endregion

                        #region Alert
                        case InteractionType.ALERT:
                            if (ExtraData == "1")
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }
                            break;
                        #endregion

                        #region Vending Machine
                        case InteractionType.VENDING_MACHINE:

                            if (ExtraData == "1")
                            {
                                user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                                if (user == null)
                                    break;
                                user.UnlockWalking();
                                if (GetBaseItem().VendingIds.Count > 0)
                                {
                                    int randomDrink = GetBaseItem().VendingIds[RandomNumber.GenerateRandom(0, (GetBaseItem().VendingIds.Count - 1))];
                                    user.CarryItem(randomDrink);
                                }


                                InteractingUser = 0;
                                ExtraData = "0";

                                UpdateState(false, true);
                            }
                            break;
                        #endregion

                        #region Scoreboard
                        case InteractionType.SCOREBOARD:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;


                                int seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (InteractionCountHelper == 1)
                                    {
                                        seconds--;
                                        InteractionCountHelper = 0;

                                        ExtraData = seconds.ToString();
                                        UpdateState();
                                    }
                                    else
                                        InteractionCountHelper++;

                                    UpdateCounter = 1;
                                }
                                else
                                    UpdateCounter = 0;

                                break;
                            }
                        #endregion

                        #region Banzai Counter
                        case InteractionType.banzaicounter:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;

                                int seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (InteractionCountHelper == 1)
                                    {
                                        seconds--;
                                        InteractionCountHelper = 0;

                                        if (GetRoom().GetBanzai().IsBanzaiActive)
                                        {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        InteractionCountHelper++;

                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    UpdateCounter = 0;
                                    GetRoom().GetBanzai().BanzaiEnd();
                                }

                                break;
                            }
                        #endregion

                        #region Banzai Tele
                        case InteractionType.banzaitele:
                            {
                                ExtraData = string.Empty;
                                UpdateState();
                                break;
                            }
                        #endregion

                        #region Banzai Floor
                        case InteractionType.banzaifloor:
                            {
                                if (Value == 3)
                                {
                                    if (InteractionCountHelper == 1)
                                    {
                                        InteractionCountHelper = 0;

                                        switch (Team)
                                        {
                                            case Team.Blue:
                                                {
                                                    ExtraData = "11";
                                                    break;
                                                }

                                            case Team.Green:
                                                {
                                                    ExtraData = "8";
                                                    break;
                                                }

                                            case Team.Red:
                                                {
                                                    ExtraData = "5";
                                                    break;
                                                }

                                            case Team.Yellow:
                                                {
                                                    ExtraData = "14";
                                                    break;
                                                }
                                        }
                                    }
                                    else
                                    {
                                        ExtraData = "";
                                        InteractionCountHelper++;
                                    }

                                    UpdateState();

                                    InteractionCount++;

                                    if (InteractionCount < 16)
                                    {
                                        UpdateCounter = 1;
                                    }
                                    else
                                        UpdateCounter = 0;
                                }
                                break;
                            }
                        #endregion

                        #region Banzai Puck
                        case InteractionType.banzaipuck:
                            {
                                if (InteractionCount > 4)
                                {
                                    InteractionCount++;
                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    InteractionCount = 0;
                                    UpdateCounter = 0;
                                }

                                break;
                            }
                        #endregion

                        #region Freeze Tile
                        case InteractionType.FREEZE_TILE:
                            {
                                if (InteractingUser > 0)
                                {
                                    ExtraData = "11000";
                                    UpdateState(false, true);
                                    GetRoom().GetFreeze().onFreezeTiles(this, FreezePowerUp);
                                    InteractingUser = 0;
                                    InteractionCountHelper = 0;
                                }
                                break;
                            }
                        #endregion

                        #region Football Counter
                        case InteractionType.COUNTER:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;

                                int seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (InteractionCountHelper == 1)
                                    {
                                        seconds--;
                                        InteractionCountHelper = 0;
                                        if (GetRoom().GetSoccer().GameIsStarted)
                                        {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        InteractionCountHelper++;

                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    UpdateNeeded = false;
                                    GetRoom().GetSoccer().StopGame();
                                }

                                break;
                            }
                        #endregion

                        #region Freeze Timer
                        case InteractionType.freezetimer:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;

                                int seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (InteractionCountHelper == 1)
                                    {
                                        seconds--;
                                        InteractionCountHelper = 0;
                                        if (GetRoom().GetFreeze().GameIsStarted)
                                        {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        InteractionCountHelper++;

                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    UpdateNeeded = false;
                                    GetRoom().GetFreeze().StopGame();
                                }

                                break;
                            }
                        #endregion

                        #region Pressure Pad
                        case InteractionType.PRESSURE_PAD:
                            {
                                ExtraData = "1";
                                UpdateState();
                                break;
                            }
                        #endregion

                        #region Wired
                        case InteractionType.WIRED_EFFECT:
                        case InteractionType.WIRED_TRIGGER:
                        case InteractionType.WIRED_CONDITION:
                            {
                                if (ExtraData == "1")
                                {
                                    ExtraData = "0";
                                    UpdateState(false, true);
                                }
                            }
                            break;
                        #endregion

                        #region Cannon
                        case InteractionType.CANNON:
                            {
                                if (ExtraData != "1")
                                    break;

                                #region Target Calculation
                                Point targetStart = Coordinate;
                                List<Point> targetSquares = new List<Point>();
                                switch (Rotation)
                                {
                                    case 0:
                                        {
                                            targetStart = new Point(GetX - 1, GetY);

                                            if (!targetSquares.Contains(targetStart))
                                                targetSquares.Add(targetStart);

                                            for (int I = 1; I <= 3; I++)
                                            {
                                                Point targetSquare = new Point(targetStart.X - I, targetStart.Y);

                                                if (!targetSquares.Contains(targetSquare))
                                                    targetSquares.Add(targetSquare);
                                            }

                                            break;
                                        }

                                    case 2:
                                        {
                                            targetStart = new Point(GetX, GetY - 1);

                                            if (!targetSquares.Contains(targetStart))
                                                targetSquares.Add(targetStart);

                                            for (int I = 1; I <= 3; I++)
                                            {
                                                Point targetSquare = new Point(targetStart.X, targetStart.Y - I);

                                                if (!targetSquares.Contains(targetSquare))
                                                    targetSquares.Add(targetSquare);
                                            }

                                            break;
                                        }

                                    case 4:
                                        {
                                            targetStart = new Point(GetX + 2, GetY);

                                            if (!targetSquares.Contains(targetStart))
                                                targetSquares.Add(targetStart);

                                            for (int I = 1; I <= 3; I++)
                                            {
                                                Point targetSquare = new Point(targetStart.X + I, targetStart.Y);

                                                if (!targetSquares.Contains(targetSquare))
                                                    targetSquares.Add(targetSquare);
                                            }

                                            break;
                                        }

                                    case 6:
                                        {
                                            targetStart = new Point(GetX, GetY + 2);


                                            if (!targetSquares.Contains(targetStart))
                                                targetSquares.Add(targetStart);

                                            for (int I = 1; I <= 3; I++)
                                            {
                                                Point targetSquare = new Point(targetStart.X, targetStart.Y + I);

                                                if (!targetSquares.Contains(targetSquare))
                                                    targetSquares.Add(targetSquare);
                                            }

                                            break;
                                        }
                                }
                                #endregion

                                if (targetSquares.Count > 0)
                                {
                                    foreach (Point square in targetSquares.ToList())
                                    {
                                        List<RoomUser> affectedUsers = _room.GetGameMap().GetRoomUsers(square).ToList();

                                        if (affectedUsers == null || affectedUsers.Count == 0)
                                            continue;

                                        foreach (RoomUser target in affectedUsers)
                                        {
                                            if (target == null || target.IsBot || target.IsPet)
                                                continue;

                                            if (target.GetClient() == null || target.GetClient().GetHabbo() == null)
                                                continue;

                                            if (_room.CheckRights(target.GetClient(), true))
                                                continue;

                                            target.ApplyEffect(4);
                                            target.GetClient().SendPacket(new RoomNotificationComposer("Kicked from room", "You were hit by a cannonball!", "room_kick_cannonball", ""));
                                            target.ApplyEffect(0);
                                            _room.GetRoomUserManager().RemoveUserFromRoom(target.GetClient(), true);
                                        }
                                    }
                                }

                                ExtraData = "2";
                                UpdateState(false, true);
                            }
                            break;
                            #endregion
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public static string[] RandomizeStrings(string[] arr)
        {
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
            // Add all strings from array
            // Add new random int each time
            foreach (string s in arr)
            {
                list.Add(new KeyValuePair<int, string>(_random.Next(), s));
            }
            // Sort the list by the random number
            var sorted = from item in list
                         orderby item.Key
                         select item;
            // Allocate new string array
            string[] result = new string[arr.Length];
            // Copy values to array
            int index = 0;
            foreach (KeyValuePair<int, string> pair in sorted)
            {
                result[index] = pair.Value;
                index++;
            }
            // Return copied array
            return result;
        }

        public void RequestUpdate(int cycles, bool setUpdate)
        {
            UpdateCounter = cycles;
            if (setUpdate)
                UpdateNeeded = true;
        }

        public void UpdateState()
        {
            UpdateState(true, true);
        }

        public void UpdateState(bool inDb, bool inRoom)
        {
            if (GetRoom() == null)
                return;

            if (inDb)
                GetRoom().GetRoomItemHandler().UpdateItem(this);

            if (inRoom)
            {
                if (IsFloorItem)
                    GetRoom().SendPacket(new ObjectUpdateComposer(this, GetRoom().OwnerId));
                else
                    GetRoom().SendPacket(new ItemUpdateComposer(this, GetRoom().OwnerId));
            }
        }

        public void ResetBaseItem()
        {
            Data = null;
            Data = GetBaseItem();
        }

        public ItemData GetBaseItem()
        {
            if (Data == null)
            {
                if (PlusEnvironment.GetGame().GetItemManager().GetItem(BaseItem, out ItemData I))
                    Data = I;
            }

            return Data;
        }

        public Room GetRoom()
        {
            if (_room != null)
                return _room;
            
            if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out Room room))
                return room;

            return null;
        }

        public void UserFurniCollision(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerUserFurniCollision, user.GetClient().GetHabbo(), this);
        }

        public void UserWalksOnFurni(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            if (GetBaseItem().InteractionType == InteractionType.TENT || GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
            {
                GetRoom().AddUserToTent(Id, user);
            }

            GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerWalkOnFurni, user.GetClient().GetHabbo(), this);
            user.LastItem = this;
        }

        public void UserWalksOffFurni(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            if (GetBaseItem().InteractionType == InteractionType.TENT || GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
                GetRoom().RemoveUserFromTent(Id, user);

            GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerWalkOffFurni, user.GetClient().GetHabbo(), this);
        }

        public void Destroy()
        {
            _room = null;
            Data = null;
            GetAffectedTiles.Clear();
        }
    }
}