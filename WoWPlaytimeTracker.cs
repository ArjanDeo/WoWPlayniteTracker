using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WoWPlaytimeTracker
{
    public class WoWPlaytimeTracker : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private WoWPlaytimeTrackerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("27bc40f5-3792-4286-b6b9-ad51c57c9101");

        // Change to something more appropriate
        public override string Name => "Custom Library";

        // Implementing Client adds ability to open it via special menu in playnite.
        public override LibraryClient Client { get; } = new WoWPlaytimeTrackerClient();

        public WoWPlaytimeTracker(IPlayniteAPI api) : base(api)
        {
            settings = new WoWPlaytimeTrackerSettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
        }


        public string GetWoWDirectory()
        {
            foreach (var game in PlayniteApi.Database.Games)
            {
                if (game.Name.Contains("World of Warcraft"))
                {
                    return game.InstallDirectory;
                }
            }
            return null;
        }
        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {

            if (args.Game.Name.Contains("World of Warcraft"))
            {
                string wtfDirectory = $"{GetWoWDirectory()}\\_retail_\\WTF\\Account";
                var dirs = Directory.GetDirectories(wtfDirectory, "*", SearchOption.TopDirectoryOnly).ToList();
                string table = string.Empty;
                for (int i = 0; i < dirs.Count; i++)
                {
                    var dir = dirs[i];
                    if (Directory.GetFiles($"{dir}\\SavedVariables", "WoWPlayniteTracker.lua").Length != 0)
                    {
                        table = File.ReadAllText($"{dir}\\SavedVariables\\WoWPlayniteTracker.lua");
                        break;
                    }

                }
                var regex = new Regex(@"\[\s*""(.*?)""\s*\]\s*=\s*(\d+)", RegexOptions.Multiline);
                var matches = regex.Matches(table);

                ulong total = 0;

                foreach (Match match in matches)
                {
                    string characterName = match.Groups[1].Value;
                    ulong playtime = ulong.Parse(match.Groups[2].Value);
                    total += playtime;
                }

                UpdateWoWPlaytime(total);
            } 
        }
        public void UpdateWoWPlaytime(ulong playtime)
        {
            var wowDirectory = GetWoWDirectory();
            if (wowDirectory == null)
            {
                logger.Error("World of Warcraft directory not found.");
                return;
            }
            foreach (var game in PlayniteApi.Database.Games)
            {
                if (game.Name.Contains("World of Warcraft"))
                {

                    game.Playtime = playtime;
                    PlayniteApi.Database.Games.Update(game);
                    logger.Info($"Updated playtime for {game.Name} to {game.Playtime / 3600} hours.");
                }
            }
        }
        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            // Return list of user's games.
            return new List<GameMetadata>()
            {
                new GameMetadata()
                {
                    Name = "Notepad",
                    GameId = "notepad",
                    GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Type = GameActionType.File,
                            Path = "notepad.exe",
                            IsPlayAction = true
                        }
                    },
                    IsInstalled = true,
                    Icon = new MetadataFile(@"c:\Windows\notepad.exe")
                },
                new GameMetadata()
                {
                    Name = "Calculator",
                    GameId = "calc",
                    GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Type = GameActionType.File,
                            Path = "calc.exe",
                            IsPlayAction = true
                        }
                    },
                    IsInstalled = true,
                    Icon = new MetadataFile(@"https://playnite.link/applogo.png"),
                    BackgroundImage = new MetadataFile(@"https://playnite.link/applogo.png")
                }
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new WoWPlaytimeTrackerSettingsView();
        }
    }
}