using FootballPredictionAPI.DTOs.FootballApi;
using System.Net;
using System.Net.Http.Json;


namespace FootballPredictionAPI.Services
{
    public class FootballApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FootballApiService> _logger;

        public FootballApiService(IHttpClientFactory httpClientFactory, ILogger<FootballApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("FootballData");
            _logger = logger;
        }

        public async Task<FootballCompetitionsResponseDto> GetCompetitionsAsync()
        {
            return await GetFromApiAsync<FootballCompetitionsResponseDto>(
                "competitions");
        }

        // Get teams for a specific competition (GetTeamsAsync("PL"))
        public async Task<FootballTeamsResponseDto> GetTeamsAsync(string competitionCode)
        {
            return await GetFromApiAsync<FootballTeamsResponseDto>(
                $"competitions/{competitionCode}/teams");
        }

        public async Task<FootballMatchesResponseDto> GetMatchesAsync(string competitionCode)
        {
            return await GetFromApiAsync<FootballMatchesResponseDto>(
                $"competitions/{competitionCode}/matches");
        }


        //we introduced generics for all 3 functions above since they all do the same thing: 1.fetch data, 2.check if too many request 3.deserialize into objects from json
        //DRY principle (Don't Repeat Yourself)
        private async Task<T> GetFromApiAsync<T>(string endpoint)
        {
            HttpResponseMessage response;

            try
            {
                response = await _httpClient.GetAsync(endpoint);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "Football API request timed out. Endpoint: {Endpoint}",
                    endpoint);

                throw;
            }

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Football API request failed. Endpoint: {Endpoint}, StatusCode: {StatusCode}",
                        endpoint,
                        (int)response.StatusCode);
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new HttpRequestException(
                        "Football API rate limit exceeded.",
                        null,
                        HttpStatusCode.TooManyRequests);
                }

                response.EnsureSuccessStatusCode();
                if (response.Headers.TryGetValues("X-RequestsAvailable", out var availableValues))
                {
                    _logger.LogInformation(
                        "Football API requests remaining: {RequestsAvailable}",
                        availableValues.FirstOrDefault());
                }
                if (response.Headers.TryGetValues("X-RequestCounter-Reset", out var resetValues))
                {
                    _logger.LogInformation(
                        "Football API rate-limit counter resets in {ResetSeconds} seconds",
                        resetValues.FirstOrDefault());
                }

                var result = await response.Content.ReadFromJsonAsync<T>();

                return result ?? throw new InvalidOperationException(
                    "Football API returned an empty response.");
            }
        }
    }
}