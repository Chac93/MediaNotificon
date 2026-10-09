using System;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;

namespace MediaNotif
{
    public class AppSettings
    {
        public string Position { get; set; } = "TopRight"; // TopRight, TopLeft, BottomRight, BottomLeft, TopCenter
        public double DisplayDurationSeconds { get; set; } = 3.5;
        public double NotificationWidth { get; set; } = 420;
        public bool StartWithWindows { get; set; } = false;
        public int MarginX { get; set; } = 20;
        public int MarginY { get; set; } = 24;
        public string MediaSourceFilter { get; set; } = "All"; // Kept for backward compatibility
        public System.Collections.Generic.List<string> MediaSources { get; set; } = new System.Collections.Generic.List<string> { "All" };
        public string Language { get; set; } = "fr"; // "fr" or "en"
        public string Theme { get; set; } = "AuraNeo"; // AuraNeo, ClassicDunst, NordicFrost, MidnightAmoled
        public bool DoNotDisturb { get; set; } = false;
        public bool CompactMode { get; set; } = false;
    }

    public static class Loc
    {
        public static bool IsEnglish => string.Equals(SettingsManager.Current.Language, "en", StringComparison.OrdinalIgnoreCase);

        public static string Get(string fr, string en) => IsEnglish ? en : fr;

        public static string AppTitle => "MediaNotif";
        public static string AppSubtitle => Get("Personnalisation & Comportement", "Customization & Behavior");
        public static string LivePreviewTitle => Get("APERÇU EN DIRECT", "LIVE PREVIEW");
        public static string LivePreviewSubtitle => Get("Rendu dynamique selon vos réglages actuels", "Dynamic preview based on current settings");
        public static string ThemeTitle => Get("THÈME VISUEL", "VISUAL THEME");
        public static string ThemeSubtitle => Get("Choisissez le style graphique des notifications", "Choose popup graphic & color aesthetic");
        public static string PositionTitle => Get("POSITION À L'ÉCRAN", "SCREEN POSITION");
        public static string PositionSubtitle => Get("Choisissez le coin d'apparition de la popup", "Choose where the popup appears");
        public static string SourceTitle => Get("SOURCES AUDIO", "AUDIO SOURCES");
        public static string SourceSubtitle => Get("Sélectionnez les applications à écouter (YouTube Music, Music Assistant, Spotify...)", "Select which applications to monitor (YouTube Music, Music Assistant, Spotify...)");
        public static string DimensionsTitle => Get("DIMENSIONS & DURÉE", "SIZE & DURATION");
        public static string DimensionsSubtitle => Get("Ajustez la taille et le temps d'affichage", "Adjust notification size and display time");
        public static string DurationLabel => Get("Durée d'affichage", "Display Duration");
        public static string WidthLabel => Get("Largeur de la notification", "Notification Width");
        public static string LanguageTitle => Get("LANGUE / LANGUAGE", "LANGUAGE / LANGUE");
        public static string LanguageSubtitle => Get("Choisissez la langue de l'interface", "Choose interface language");
        public static string French => "🇫🇷  Français";
        public static string English => "🇬🇧  English";
        public static string SystemTitle => Get("SYSTÈME & MODES", "SYSTEM & MODES");
        public static string SystemSubtitle => Get("Comportement, démarrage et affichage discret", "Behavior, startup and stealth modes");
        public static string AutoStartTitle => Get("Démarrage automatique", "Start with Windows");
        public static string AutoStartSubtitle => Get("Lancer MediaNotif en tâche de fond avec Windows", "Run MediaNotif in background at login");
        public static string DoNotDisturb => Get("🔕  Ne pas déranger", "🔕  Do Not Disturb");
        public static string DoNotDisturbSub => Get("Couper toutes les notifications visuelles", "Mute all visual popup notifications");
        public static string CompactMode => Get("📱  Mode compact", "📱  Compact Mode");
        public static string CompactModeSub => Get("Format épuré sur une seule ligne (idéal en jeu)", "Slim single-line format (ideal in-game)");
        public static string TestBtn => Get("🎵  Tester", "🎵  Test");
        public static string SaveBtn => Get("💾  Enregistrer", "💾  Save");
        public static string SavedBtn => Get("✓  Enregistré !", "✓  Saved!");
        public static string CloseBtn => Get("Fermer", "Close");
        public static string ConfigFolder => Get("📁  Dossier de configuration", "📁  Config Folder");
        public static string Quit => Get("🚪  Quitter", "🚪  Quit");
        public static string Settings => Get("⚙️  Paramètres...", "⚙️  Settings...");
        public static string TestNotif => Get("🎵  Tester la notification", "🎵  Test Notification");
        public static string AudioSource => Get("🎧  Sources audio", "🎧  Audio Sources");
        public static string ThemeMenu => Get("🎨  Thème", "🎨  Theme");
        public static string LanguageMenu => Get("🌐  Langue", "🌐  Language");
        public static string AllSources => Get("🌐  Toutes les sources", "🌐  All Sources");
        public static string MusicAssistant => "🎼  Music Assistant";
        public static string StartupNotifTitle => Get("MediaNotif est actif !", "MediaNotif is active!");
        public static string StartupNotifBody => Get("Sources : {0} • Clic droit sur l'icône ♫", "Sources: {0} • Right-click on ♫ icon");
        public static string SourceChangedTitle => Get("Sources audio modifiées", "Audio Sources Changed");
        public static string SourceChangedBody => Get("Écoute active : {0}", "Active listening: {0}");
        public static string ThemeChangedTitle => Get("Thème visuel modifié", "Visual Theme Changed");
        public static string ThemeChangedBody => Get("Thème actif : {0}", "Active theme: {0}");
        public static string LanguageChangedTitle => Get("Langue modifiée", "Language Changed");
        public static string LanguageChangedBody => Get("Interface en français", "Interface switched to English");

        // Themes
        public static string ThemeAuraNeo => Get("✨  Aura Neo (Défaut)", "✨  Aura Neo (Default)");
        public static string ThemeClassicDunst => Get("🐧  Dunst Classic", "🐧  Classic Dunst");
        public static string ThemeNordicFrost => Get("❄️  Nordic Frost", "❄️  Nordic Frost");
        public static string ThemeMidnightAmoled => Get("🖤  Midnight AMOLED", "🖤  Midnight AMOLED");

        // Positions
        public static string PosTopRight => Get("◳  Haut Droite (Défaut)", "◳  Top Right (Default)");
        public static string PosTopLeft => Get("◰  Haut Gauche", "◰  Top Left");
        public static string PosBottomRight => Get("◲  Bas Droite", "◲  Bottom Right");
        public static string PosBottomLeft => Get("◱  Bas Gauche", "◱  Bottom Left");
        public static string PosTopCenter => Get("⏶  Haut Centre", "⏶  Top Center");

        // Pills
        public static string DurShort => Get("Court (2.5s)", "Short (2.5s)");
        public static string DurStd => Get("Standard (3.5s)", "Standard (3.5s)");
        public static string DurLong => Get("Long (5.0s)", "Long (5.0s)");

        public static string WidthCompact => Get("Compact (360px)", "Compact (360px)");
        public static string WidthStd => Get("Standard (420px)", "Standard (420px)");
        public static string WidthMax => Get("Max (480px)", "Max (480px)");
    }

    public static class SettingsManager
    {
        public static string DetectSourceKey(string? sourceAppId)
        {
            if (string.IsNullOrWhiteSpace(sourceAppId))
                return "All";

            string id = sourceAppId.ToLowerInvariant();
            if (id.Contains("music-assistant") || id.Contains("musicassistant") || id.Contains("music_assistant"))
                return "MusicAssistant";
            if (id.Contains("youtube-music") || id.Contains("youtubemusic"))
                return "YouTubeMusic";
            if (id.Contains("spotify"))
                return "Spotify";
            if (id.Contains("zen"))
                return "Zen";
            if (id.Contains("chrome"))
                return "Chrome";
            if (id.Contains("firefox"))
                return "Firefox";
            if (id.Contains("edge") || id.Contains("msedge"))
                return "Edge";
            if (id.Contains("brave"))
                return "Brave";

            return "All";
        }

        public static bool MatchesSource(string? sourceAppId, string sourceKey)
        {
            if (string.IsNullOrWhiteSpace(sourceKey) || sourceKey.Equals("All", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.IsNullOrWhiteSpace(sourceAppId))
                return false;

            string id = sourceAppId.ToLowerInvariant();
            return sourceKey switch
            {
                "MusicAssistant" => id.Contains("music-assistant") || id.Contains("musicassistant") || id.Contains("music_assistant"),
                "YouTubeMusic" => id.Contains("youtube-music") || id.Contains("youtubemusic"),
                "Spotify" => id.Contains("spotify"),
                "Zen" => id.Contains("zen"),
                "Chrome" => id.Contains("chrome"),
                "Firefox" => id.Contains("firefox"),
                "Edge" => id.Contains("edge") || id.Contains("msedge"),
                "Brave" => id.Contains("brave"),
                _ => id.Contains(sourceKey.ToLowerInvariant())
            };
        }

        public static bool MatchesSourceFilter(string? sourceAppId)
        {
            var sources = Current.MediaSources;
            if (sources == null || sources.Count == 0 || System.Linq.Enumerable.Any(sources, s => s.Equals("All", StringComparison.OrdinalIgnoreCase)))
                return true;

            if (string.IsNullOrWhiteSpace(sourceAppId))
                return false;

            return System.Linq.Enumerable.Any(sources, s => MatchesSource(sourceAppId, s));
        }

        public static bool IsSourceEnabled(string sourceKey)
        {
            var sources = Current.MediaSources;
            if (sources == null || sources.Count == 0)
                return sourceKey.Equals("All", StringComparison.OrdinalIgnoreCase);

            if (sourceKey.Equals("All", StringComparison.OrdinalIgnoreCase))
                return System.Linq.Enumerable.Any(sources, s => s.Equals("All", StringComparison.OrdinalIgnoreCase));

            return System.Linq.Enumerable.Contains(sources, sourceKey, StringComparer.OrdinalIgnoreCase);
        }

        public static void SetSource(string sourceKey)
        {
            Current.MediaSources.Clear();
            Current.MediaSources.Add(sourceKey);
            Current.MediaSourceFilter = sourceKey;
            Save();
        }

        public static void ToggleSource(string sourceKey)
        {
            if (sourceKey.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                Current.MediaSources.Clear();
                Current.MediaSources.Add("All");
                Current.MediaSourceFilter = "All";
                Save();
                return;
            }

            Current.MediaSources.RemoveAll(s => s.Equals("All", StringComparison.OrdinalIgnoreCase));

            int idx = Current.MediaSources.FindIndex(s => s.Equals(sourceKey, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                Current.MediaSources.RemoveAt(idx);
            }
            else
            {
                Current.MediaSources.Add(sourceKey);
            }

            if (Current.MediaSources.Count == 0)
            {
                Current.MediaSources.Add("All");
            }

            Current.MediaSourceFilter = string.Join(",", Current.MediaSources);
            Save();
        }

        public static string GetFilterDisplayName(string filter)
        {
            return filter switch
            {
                "MusicAssistant" => "Music Assistant",
                "YouTubeMusic" => "YouTube Music",
                "Spotify" => "Spotify",
                "Zen" => "Zen Browser",
                "Chrome" => "Google Chrome",
                "Firefox" => "Mozilla Firefox",
                "Edge" => "Microsoft Edge",
                "Brave" => "Brave Browser",
                _ => Loc.AllSources
            };
        }

        public static string GetActiveSourcesDisplayName()
        {
            var sources = Current.MediaSources;
            if (sources == null || sources.Count == 0 || System.Linq.Enumerable.Any(sources, s => s.Equals("All", StringComparison.OrdinalIgnoreCase)))
                return Loc.AllSources;

            var names = System.Linq.Enumerable.Select(sources, GetFilterDisplayName);
            return string.Join(", ", names);
        }

        public static string GetThemeDisplayName(string theme)
        {
            return theme switch
            {
                "ClassicDunst" => Loc.ThemeClassicDunst,
                "NordicFrost" => Loc.ThemeNordicFrost,
                "MidnightAmoled" => Loc.ThemeMidnightAmoled,
                _ => Loc.ThemeAuraNeo
            };
        }

        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MediaNotif"
        );
        private static readonly string ConfigFile = Path.Combine(ConfigDir, "settings.json");

        public static AppSettings Current { get; private set; } = new AppSettings();

        public static void Load()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    string json = File.ReadAllText(ConfigFile);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null)
                    {
                        Current = loaded;
                    }
                }
            }
            catch { }

            // Ensure MediaSources is initialized
            if (Current.MediaSources == null || Current.MediaSources.Count == 0)
            {
                Current.MediaSources = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(Current.MediaSourceFilter))
                {
                    foreach (var part in Current.MediaSourceFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        Current.MediaSources.Add(part);
                    }
                }
                if (Current.MediaSources.Count == 0)
                {
                    Current.MediaSources.Add("All");
                }
            }

            // Sync startup registry state
            Current.StartWithWindows = CheckStartupRegistry();
        }

        public static void Save()
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                {
                    Directory.CreateDirectory(ConfigDir);
                }

                string json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFile, json);
                ApplyStartupRegistry(Current.StartWithWindows);
            }
            catch { }
        }

        private static bool CheckStartupRegistry()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                return key?.GetValue("MediaNotif") != null;
            }
            catch
            {
                return false;
            }
        }

        private static void ApplyStartupRegistry(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null) return;

                if (enable)
                {
                    string? exePath = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue("MediaNotif", $"\"{exePath}\"");
                    }
                }
                else
                {
                    key.DeleteValue("MediaNotif", false);
                }
            }
            catch { }
        }
    }
}
