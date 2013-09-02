namespace SpadesBot.Models
{
    public class Book
    {
        public int leader { get; set; }
        public string player1_card { get; set; }
        public string player2_card { get; set; }
        public string player3_card { get; set; }
        public string player4_card { get; set; }
        public int winner { get; set; }
    }
}