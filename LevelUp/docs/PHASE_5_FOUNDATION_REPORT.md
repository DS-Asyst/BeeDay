# Phase 5 Foundation Report

## Scope

This delivery reorganizes the main navigation and introduces the first two personal-life modules without implementing achievements, health consequences, or a fictional economy.

## Navigation

- Character remains focused on identity and progression.
- Diary groups Trainings, Quests, Projects, and Chapters.
- Library is an independent reading-tracking module.
- Backpack is a resource hub and currently contains Wallet.

## Library

- books have Planned, Reading, Completed, and Archived states;
- at most two books may be Reading simultaneously;
- newly created books start at page 1;
- each progress entry stores previous page, current page, and date;
- page regression and pages beyond the total are rejected;
- reading awards 0.5 XP per newly registered page;
- completed books release an active-reading slot.

## Wallet

- transactions are Deposits or Withdrawals;
- every transaction has amount, description, and occurrence date;
- withdrawals require a justification;
- withdrawals cannot exceed the available balance;
- editing and deletion preserve a non-negative resulting balance;
- monthly net movement can be consulted;
- Wallet is real-money tracking and never an RPG reward currency.

## Persistence

Books, reading history, and wallet transactions are part of the central GameData snapshot.

## Deferred work

- achievements and level titles;
- financial goals and analytics;
- life or energy consequences;
- Backpack items beyond Wallet;
- reading streaks and richer statistics.
