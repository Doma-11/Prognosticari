namespace FootballPredictionAPI.DTOs.FootballApi
{
    public class FootballScoreDto
    {
        public FootballFullTimeDto? FullTime { get; set; }
        

        public class FootballFullTimeDto
        {
            public int? Home { get; set; }
            public int? Away { get; set; }
        }
    }
}
