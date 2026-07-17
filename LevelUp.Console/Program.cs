using LevelUp.Application;
using LevelUp.Infrastructure.Persistence;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

using SqliteGameDataStore dataStore = new();
ApplicationBootstrap.Build(dataStore).Show();
