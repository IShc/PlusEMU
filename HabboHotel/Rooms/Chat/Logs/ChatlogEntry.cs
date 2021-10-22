using System;

using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Logs
{
    public sealed class ChatlogEntry
    {
        private int _playerId;
        private int _roomId;
        private string _message;
        private double _timestamp;

        private WeakReference _playerReference;
        private WeakReference _roomReference;

        public ChatlogEntry(int playerId, int roomId, string message, double timestamp, Habbo player = null, RoomData instance = null)
        {
            _playerId = playerId;
            _roomId = roomId;
            _message = message;
            _timestamp = timestamp;

            if (player != null)
                _playerReference = new WeakReference(player);

            if (instance != null)
                _roomReference = new WeakReference(instance);
        }

        public int PlayerId
        {
            get { return _playerId; }
        }

        public int RoomId
        {
            get { return _roomId; }
        }

        public string Message
        {
            get { return _message; }
        }

        public double Timestamp
        {
            get { return _timestamp; }
        }

        public Habbo PlayerNullable()
        {
            if (_playerReference.IsAlive)
            {
                Habbo playerObj = (Habbo)_playerReference.Target;

                return playerObj;
            }

            return null;
        }

        public Room RoomNullable()
        {
            if (_roomReference.IsAlive)
            {
                Room roomObj = (Room)_roomReference.Target;
                if (roomObj.mDisposed)
                    return null;
                return roomObj;
            }
            return null;
        }
    }
}
