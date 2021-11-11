namespace Plus.HabboHotel.LandingView.Promotions
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string ButtonText { get; set; }
        public int ButtonType { get; set; }
        public string ButtonLink { get; set; }
        public string ImageLink { get; set; }

        public Promotion(int id, string title, string text, string buttonText, int buttonType, string buttonLink, string imageLink)
        {
            this.Id = id;
            this.Title = title;
            this.Text = text;
            this.ButtonText = buttonText;
            this.ButtonType = buttonType;
            this.ButtonLink = buttonLink;
            this.ImageLink = imageLink;
        }
    }
}
