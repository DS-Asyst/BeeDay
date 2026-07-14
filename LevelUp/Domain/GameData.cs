using System;
using System.Collections.Generic;
using System.Text;

using LevelUp.Domain.Habits;
using LevelUp.Domain.Projects;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Domain;

public class GameData
{
    public CharacterModel Character { get; set; } = new();

    public List<Habit> Habits { get; set; } = [];

    public List<Project> Projects { get; set; } = [];
}