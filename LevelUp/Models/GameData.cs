using System;
using System.Collections.Generic;
using System.Text;

namespace LevelUp.Models;

public class GameData
{
    public Character Character { get; set; } = new();

    public List<Habit> Habits { get; set; } = new();

    public List<Project> Projects { get; set; } = new();
}
