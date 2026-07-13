using System;
using System.Collections.Generic;
using System.Text;

namespace LevelUp.Models;

public class AttributeProgress : ILevelProgress
{
    public int Level { get; set; } = 1;

    public decimal Experience { get; set; } = 0m;

    public decimal ExperienceToNextLevel => 100m;
}