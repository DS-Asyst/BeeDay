using System;
using System.Collections.Generic;
using System.Text;

namespace LevelUp.Models
{
    public class GameData
    {
        public Character Character { get; set; } = new Character();

        public List<Habit> Habits { get; set; } = new List<Habit>();
    }
}
