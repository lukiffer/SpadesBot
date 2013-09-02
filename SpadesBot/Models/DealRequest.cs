using System.Collections.Generic;

namespace SpadesBot.Models
{
    public class DealRequest : BlindRequest
    {
        public List<string> hand { get; set; } 
    }
}