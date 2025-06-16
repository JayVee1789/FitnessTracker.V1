namespace FitnessTracker.V1.Models
{
    public class PoidsAnalysisResult
    {
        public List<string> ExercicesFort { get; set; } = new();
        public List<string> ExercicesFaible { get; set; } = new();
    }

    public static class PoidsAnalyzer
    {
        public static PoidsAnalysisResult Analyser(List<PoidsEntry> entries)
        {
            var grouped = entries
                .GroupBy(e => e.Exercice)
                .ToDictionary(g => g.Key, g => g.OrderBy(e => e.Date).Select(e => e.Poids).ToList());

            var scoreMap = new Dictionary<string, int>();

            foreach (var kvp in grouped)
            {
                var liste = kvp.Value;
                int score = 0;

                for (int i = 1; i < liste.Count; i++)
                {
                    if (liste[i] > liste[i - 1])
                        score += 2;
                    else if (liste[i] == liste[i - 1])
                        score += 0;
                    else
                        score -= 1;
                }

                scoreMap[kvp.Key] = score;
            }

            var meilleurs = scoreMap.OrderByDescending(kvp => kvp.Value).Take(2).Select(kvp => kvp.Key).ToList();
            var faibles = scoreMap.OrderBy(kvp => kvp.Value).Take(2).Select(kvp => kvp.Key).ToList();

            return new PoidsAnalysisResult
            {
                ExercicesFort = meilleurs,
                ExercicesFaible = faibles
            };
        }
    }
}
