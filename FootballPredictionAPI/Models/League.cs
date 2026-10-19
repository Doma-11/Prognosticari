namespace FootballPredictionAPI.Models
{
    public class League
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;

        public int ExternalApiId { get; set; }

        public ICollection<Season> Seasons { get; set; } = new List<Season>();
    }
}
