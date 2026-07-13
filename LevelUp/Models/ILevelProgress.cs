using System;
using System.Collections.Generic;
using System.Text;

namespace LevelUp.Models;

public interface ILevelProgress
{
    int Level { get; set; }

    decimal Experience { get; set; }

    decimal ExperienceToNextLevel { get; }
}
