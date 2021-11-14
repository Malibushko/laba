using System;

namespace MVC.Models
{
    public class MatchInfoModel
    {
        public string HeroThumbnail { get; set; }

        public string MatchID { get; set; }

        public DateTime StartTime { get; set; }

        public string Result { get; set; }

        public int PositiveVotes { get; set; }

        public int NegativeVotes { get; set; }

        public string KDA { get; set; }
    }
}
