using System;
using System.Collections.Generic;
using System.Text;
using LevelUp.Models;

namespace LevelUp.Services
{
    public class HabitService
    {
        private readonly List<Habit> habits = new();

        public Habit CreateHabit(
            string title,
            string description,
            int durationInMinutes)
        {
            Habit habit = new Habit
            {
                Id = habits.Count + 1,
                Title = title,
                Description = description,
                DurationInMinutes = durationInMinutes
            };

            habits.Add(habit);

            return habit;
        }

        public List<Habit> GetAllHabits()
        {
            return habits;
        }

        public decimal CompleteHabit(Habit habit)


        {
            habit.TimesCompleted++;

            return habit.ExperienceReward;

        }

        public void LoadHabits(List<Habit> loadedHabits)
        {
            habits.Clear();
            habits.AddRange(loadedHabits);
        }
    }
}