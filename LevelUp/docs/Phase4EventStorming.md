# Phase 4 Event Storming — Milestones and Boss Encounters

## Ubiquitous Language

- **Project:** a long-running real-world objective.
- **Quest:** an actionable task. It may be independent, linked only to a Project, or linked to one Milestone inside that Project.
- **Milestone:** an ordered, optional chapter of a Project. A Project may have no Milestones or many Milestones.
- **Boss Encounter:** an optional challenge linked to exactly one Milestone. It is not a full combat system in Phase 4.
- **Reward:** milestone reward metadata. Reward delivery is intentionally deferred to the Gold and Rewards phase.

## Core Policies

1. A Milestone always belongs to one Project.
2. Milestone order is unique inside a Project.
3. Only one Milestone may be Active in a Project.
4. The first Milestone can be activated automatically when its Project becomes Active.
5. Later Milestones remain Locked and are activated sequentially.
6. A Quest may belong to at most one Milestone.
7. A Quest and its Milestone must belong to the same Project.
8. Completed or archived Quests cannot change Project or Milestone associations.
9. Archived Quests do not count toward progress.
10. A completed Milestone cannot receive new Quests or be deleted.
11. A Milestone without a Boss completes automatically when its quest requirement is met.
12. A Milestone with a Boss unlocks the Boss when its quest requirement is met; defeating the Boss completes the Milestone.
13. A final Boss may complete the Project after all valid Quests and Milestones are complete.
14. Quest progress and Milestone progress are displayed separately.
15. Rewards are claimed explicitly and only once, but crediting XP, Gold, or Titles is deferred.

## Commands

- CreateMilestone
- UpdateMilestone
- ActivateMilestone
- CompleteManualMilestone
- ArchiveMilestone
- DeleteMilestone
- AssignQuestToMilestone
- RemoveQuestFromMilestone
- CreateBossEncounter
- UnlockBossEncounter
- DefeatBossEncounter
- ClaimMilestoneReward

## Conceptual Domain Events

The events below guide workflows but are not implemented as an event bus yet:

- MilestoneCreated
- MilestoneUnlocked
- MilestoneActivated
- MilestoneCompleted
- MilestoneArchived
- BossEncounterCreated
- BossUnlocked
- BossDefeated
- MilestoneRewardClaimed

## Automatic Reactions

When a Quest is completed:

1. Project quest progress is recalculated.
2. Milestone quest progress is recalculated when applicable.
3. A Milestone without a Boss may complete automatically.
4. A Milestone with a Boss may unlock its Boss.
5. The next Milestone may be unlocked and activated after the current Milestone completes.
6. The Project may complete when all valid Quests and Milestones are complete.

When a Boss is defeated:

1. The linked Milestone completes.
2. The next Milestone is unlocked and activated when present.
3. A final Boss may complete the Project.
