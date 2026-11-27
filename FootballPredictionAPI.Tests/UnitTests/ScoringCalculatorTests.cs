using FootballPredictionAPI.Scoring;
using Xunit;
using FootballPredictionAPI.Models;

namespace FootballPredictionAPI.Tests.UnitTests
{
    public class ScoringCalculatorTests
    {
        //[Fact]-> An attribute that tells xUnit this method represents an individual test
        //[Theory] -> Identifies a parameterized test method that can receive different sets of input data.
        //[InlineData] -> Provides one set of arguments to that test method.Each InlineData attribute represents an individual test case.

        //predictedhomegoals,predictedawaygoals,actualhomegoals, actualawaygoals, expectedpoints
        [Theory]
        [InlineData(3, 0, 3, 0, 4)]
        [InlineData(2, 0, 3, 0, 1)]
        [InlineData(0, 3, 3, 0, 0)]
        [InlineData(0, 3, 0, 3, 4)]
        [InlineData(0, 0, 0, 0, 4)]
        [InlineData(1, 1, 1, 1, 4)]
        [InlineData(0, 1, 0, 3, 1)]
        [InlineData(0, 0, 1, 1, 1)]
        [InlineData(2, 0, 0, 2, 0)]
        [InlineData(1, 1, 2, 0, 0)]
        [InlineData(2, 0, 1, 1, 0)]
        [InlineData(1, 1, 0, 2, 0)]
        [InlineData(0, 2, 1, 1, 0)]
        public void CalculateBasePoints_ReturnsExpectedPoints(int predictedHomeGoals,int predictedAwayGoals,int actualHomeGoals,int actualAwayGoals,int expectedPoints)
        {
            // Arrange - provided by InlineData

            // Act
            int result = ScoringCalculator.CalculateBasePoints(
                predictedHomeGoals,
                predictedAwayGoals,
                actualHomeGoals,
                actualAwayGoals);

            // Assert
            Assert.Equal(expectedPoints, result);
        }


        [Theory]
        [InlineData(2, 100, 3)]       // 2%
        [InlineData(10, 100, 1)]      // 10%
        [InlineData(50, 100, 0)]      // 50%

        // 5% boundary
        [InlineData(49, 1000, 3)]     // 4.9%
        [InlineData(50, 1000, 1)]     // 5%
        [InlineData(51, 1000, 1)]     // 5.1%

        // 20% boundary
        [InlineData(199, 1000, 1)]    // 19.9%
        [InlineData(200, 1000, 0)]    // 20%
        [InlineData(201, 1000, 0)]    // 20.1%

        // Special cases
        [InlineData(0, 0, 0)]        // No predictions
        [InlineData(0, 100, 3)]      // Nobody correct
        [InlineData(100, 100, 0)]    // Everyone correct
        [InlineData(1, 1, 0)]        // Single correct prediction
        public void CalculateRarityBonus_ReturnsExpectedBonus(int correctPredictions,int totalPredictions,int expectedBonus)
        {
            // Arrange - provided by InlineData

            // Act
            int result = ScoringCalculator.CalculateRarityBonus(
                correctPredictions,
                totalPredictions);

            // Assert
            Assert.Equal(expectedBonus, result);
        }


        [Theory]
        [InlineData(3, 0, 3, 0, 0, 0, 4)]
        [InlineData(3, 0, 3, 0, 0, 1, 5)]
        [InlineData(3, 0, 3, 0, 1, 1, 6)]
        [InlineData(3, 0, 3, 0, 1, 3, 8)]
        [InlineData(3, 0, 3, 0, 3, 3, 10)]
        [InlineData(2, 0, 3, 0, 0, 0, 1)]
        [InlineData(2, 0, 3, 0, 3, 3, 4)]
        [InlineData(0, 3, 3, 0, 3, 3, 0)]
        public void CalculateTotalPoints_ReturnsExpectedPoints(int predictedHomeGoals,int predictedAwayGoals,int actualHomeGoals,int actualAwayGoals,int outcomeRarityBonus,int exactScoreRarityBonus,int expectedPoints)
        {
            // Arrange
            var prediction = new Prediction
            {
                PredictedHomeGoals = predictedHomeGoals,
                PredictedAwayGoals = predictedAwayGoals
            };

            // Act
            int result = ScoringCalculator.CalculateTotalPoints(
                prediction,
                actualHomeGoals,
                actualAwayGoals,
                outcomeRarityBonus,
                exactScoreRarityBonus);

            // Assert
            Assert.Equal(expectedPoints, result);
        }



        [Fact]
        public void CountCorrectOutcomes_MixedPredictions_ReturnsThree()
        {
            // Arrange
            var predictions = CreateSamplePredictions();

            // Act
            int result = ScoringCalculator.CountCorrectOutcomes(
                predictions, 3, 0);

            // Assert
            Assert.Equal(3, result);
        }


        private static List<Prediction> CreateSamplePredictions()
        {
            return new List<Prediction>
            {
                new Prediction
                {
                    PredictedHomeGoals = 3,
                    PredictedAwayGoals = 0
                },
                new Prediction
                {
                    PredictedHomeGoals = 2,
                    PredictedAwayGoals = 0
                },
                new Prediction
                {
                    PredictedHomeGoals = 3,
                    PredictedAwayGoals = 0
                },
                new Prediction
                {
                    PredictedHomeGoals = 0,
                    PredictedAwayGoals = 2
                },
                new Prediction
                {
                    PredictedHomeGoals = 1,
                    PredictedAwayGoals = 1
                }
            };
        }

        [Fact]
        public void CountCorrectExactScores_MixedPredictions_ReturnsTwo()
        {
            // Arrange
            var predictions = CreateSamplePredictions();

            // Act
            int result = ScoringCalculator.CountCorrectExactScores(
                predictions, 3, 0);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void CountCorrectOutcomes_EmptyList_ReturnsZero()
        {
            // Arrange
            var predictions = new List<Prediction>();

            // Act
            int result = ScoringCalculator.CountCorrectOutcomes(
                predictions, 3, 0);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CountCorrectExactScores_EmptyList_ReturnsZero()
        {
            // Arrange
            var predictions = new List<Prediction>();

            // Act
            int result = ScoringCalculator.CountCorrectExactScores(
                predictions, 3, 0);

            // Assert
            Assert.Equal(0, result);
        }

    }
}