namespace Plus.HabboHotel.Rooms.Chat.Styles
{
    public sealed class ChatStyle
    {
        private int _id;
        private string _name;
        private string _requiredRight;

        public ChatStyle(int id, string name, string requiredRight)
        {
            _id = id;
            _name = name;
            _requiredRight = requiredRight;
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string RequiredRight
        {
            get { return _requiredRight; }
            set { _requiredRight = value; }
        }
    }
}