using FootballPredictionAPI.Models;


namespace FootballPredictionAPI.Scoring
{
    //A static class cannot be instantiated. We don't need to create an object using new
    //Instead, we will call its methods directly: ScoringCalculator.CalculateBasePoints(2, 1, 2, 1);
    public static class ScoringCalculator
    {
        private static int GetOutcome(int homeGoals, int awayGoals)
        {
            // Math.Sign() returns one of three integer values:
            // 1 for a positive number (home win)
            // -1 for a negative number (away win)
            // 0 for zero (draw)
            return Math.Sign(homeGoals - awayGoals);
        }

        public static int CalculateBasePoints(int predictedHomeGoals, int predictedAwayGoals, int actualHomeGoals, int actualAwayGoals)
        {

            int points = 0;

            int predictedOutcome = GetOutcome(predictedHomeGoals, predictedAwayGoals);

            int actualOutcome = GetOutcome(actualHomeGoals, actualAwayGoals);
            
            if (predictedOutcome == actualOutcome)
            {
                points += 1;
            }
            if (predictedHomeGoals == actualHomeGoals && predictedAwayGoals == actualAwayGoals)
            {
                points += 3;
            }

            return points;
        
        }

        public static int CalculateRarityBonus(int correctPredictions,int totalPredictions)
        {
            if (totalPredictions <= 0)
            {
                return 0;
            }

            double percentage =(double)correctPredictions / totalPredictions * 100;

            if (percentage < 5)
            {
                return 3;
            }

            if (percentage < 20)
            {
                return 1;
            }

            return 0;
        }

        public static int CountCorrectOutcomes(List<Prediction> predictions,int actualHomeGoals,int actualAwayGoals)
        {
            int actualOutcome = GetOutcome(actualHomeGoals, actualAwayGoals);

            return predictions.Count(p =>GetOutcome(p.PredictedHomeGoals,p.PredictedAwayGoals) == actualOutcome);
        }

        public static int CountCorrectExactScores(List<Prediction> predictions,int actualHomeGoals,int actualAwayGoals)
        {
            return predictions.Count(p =>p.PredictedHomeGoals == actualHomeGoals && p.PredictedAwayGoals == actualAwayGoals);
        }

        public static int CalculateTotalPoints(Prediction prediction, int actualHomeGoals, int actualAwayGoals, int outcomeRarityBonus, int exactScoreRarityBonus)
        {
            int points = CalculateBasePoints(prediction.PredictedHomeGoals, prediction.PredictedAwayGoals, actualHomeGoals, actualAwayGoals);

            if (points >= 1)
            {
                points += outcomeRarityBonus;
            }

            if (prediction.PredictedHomeGoals == actualHomeGoals &&
                prediction.PredictedAwayGoals == actualAwayGoals)
            {
                points += exactScoreRarityBonus;
            }

            return points;
        }
    }
}
