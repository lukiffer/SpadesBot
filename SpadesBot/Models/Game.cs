using System;

namespace SpadesBot.Models
{
    public class Game
    {
        public Guid id { get; set; }
        public int dealer { get; set; }
        public int leader { get; set; }
        public int team1_score { get; set; }
        public int team1_bags { get; set; }
        public int team2_score { get; set; }
        public int team2_bags { get; set; }
    }
}