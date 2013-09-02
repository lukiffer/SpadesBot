namespace SpadesBot.Models
{
    public class Round
    {
        public int leader { get; set; }
        public int? player1_bid { get; set; }
        public int player1_taken { get; set; }
        public int? player2_bid { get; set; }
        public int player2_taken { get; set; }
        public int? player3_bid { get; set; }
        public int player3_taken { get; set; }
        public int? player4_bid { get; set; }
        public int player4_taken { get; set; }
    }
}