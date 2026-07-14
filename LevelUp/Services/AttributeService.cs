using System;
using System.Collections.Generic;
using System.Text;

using LevelUp.Domain.Attributes;

namespace LevelUp.Services;

public class AttributeService
{
    private readonly ProgressionService progressionService;

    public AttributeService(
        ProgressionService progressionService)
    {
        this.progressionService = progressionService;
    }
    public void AddExperience(
        PlayerAttributes attributes,
        AttributeType attributeType,
        decimal experienceEarned)
    {
        AttributeProgress attribute =
            attributes.GetAttribute(attributeType);

        progressionService.AddExperience(
            attribute,
            experienceEarned
        );
    }
}
