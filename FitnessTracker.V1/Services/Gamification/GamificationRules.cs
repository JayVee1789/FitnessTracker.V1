using FitnessTracker.V1.Models.Gamification;

namespace FitnessTracker.V1.Services.Gamification;

public static class GamificationRules
{
    public const int SessionCompletedXp = 100;
    public const int PerfectSessionBonusXp = 50;
    public const int XpPerLevel = 500;

    public static int GetLevel(int totalXp) => Math.Max(1, totalXp / XpPerLevel + 1);

    public static int GetCurrentLevelXp(int totalXp) => Math.Max(0, totalXp % XpPerLevel);

    public static int GetXpToNextLevel(int totalXp) => XpPerLevel - GetCurrentLevelXp(totalXp);

    public static double GetLevelProgressPercent(int totalXp) =>
        Math.Clamp(GetCurrentLevelXp(totalXp) * 100d / XpPerLevel, 0, 100);

    public static int GetSessionXp(bool allExercisesDone, bool isRestDay)
    {
        if (isRestDay)
            return SessionCompletedXp;

        return allExercisesDone
            ? SessionCompletedXp + PerfectSessionBonusXp
            : SessionCompletedXp;
    }

    public static string GetLevelTitle(int level) => level switch
    {
        <= 2 => "Départ solide",
        <= 5 => "Régulier",
        <= 9 => "Machine",
        <= 14 => "Athlète",
        _ => "Légende"
    };

    public static string GetMotivation(GamificationDbModel? gamification)
    {
        if (gamification is null)
            return "Commence une séance pour lancer ta progression.";

        var level = GetLevel(gamification.TotalXP);
        var next = GetXpToNextLevel(gamification.TotalXP);

        return next == XpPerLevel
            ? $"Niveau {level} atteint. Prochaine séance : +{SessionCompletedXp} XP."
            : $"{next} XP avant le niveau {level + 1}.";
    }
}
