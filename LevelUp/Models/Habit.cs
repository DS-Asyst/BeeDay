using System;
using System.Collections.Generic;
using System.Text;

namespace LevelUp.Models
{
    public class Habit
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
        public int TimesCompleted { get; set; }
        public decimal ExperiencesPerMinute { get; set; } = 0.1m;
        public decimal ExperienceReward =>
            DurationInMinutes * ExperiencesPerMinute;

    }
}
