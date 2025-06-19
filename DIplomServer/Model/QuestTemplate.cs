namespace DIplomServer.Model
{
    public class QuestTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public GoalType GoalType { get; set; } // теперь строго enum
        public int GoalValue { get; set; }
        public int RewardExperience { get; set; }
    }

    public enum GoalType
    {
        COMPLETE_HABITS_TOTAL,
        STREAK_DAYS,
        COMPLETE_MORNING_HABITS,
        COMPLETE_HARD_HABITS
    }
}
