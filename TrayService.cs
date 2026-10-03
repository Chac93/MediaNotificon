using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace MediaNotif
{
    public static class TrayService
    {
        private static NotifyIcon? _trayIcon;
        private static ContextMenuStrip? _contextMenu;

        public static event Action? OnTestRequested;
        public static event Action? OnSettingsRequested;
        public static event Action? OnQuitRequested;
        public static event Action<string>? OnSourceFilterChanged;
        public static event Action<string>? OnThemeChanged;
        public static event Action<string>? OnLanguageChanged;

        private static readonly (string Key, string Label)[] Sources = new[]
        {
            ("All", "🌐  All"),
            ("YouTubeMusic", "🎵  YouTube Music (Desktop)"),
            ("Spotify", "🟢  Spotify"),
            ("Zen", "🌀  Zen Browser"),
            ("Chrome", "🔴  Google Chrome"),
            ("Firefox", "🦊  Mozilla Firefox"),
            ("Edge", "🌊  Microsoft Edge"),
            ("Brave", "🦁  Brave Browser")
        };

        private static readonly List<ToolStripMenuItem> _sourceMenuItems = new();
        private static readonly List<ToolStripMenuItem> _themeMenuItems = new();
        private static readonly List<ToolStripMenuItem> _languageMenuItems = new();

        private static ToolStripMenuItem? _sourceMenu;
        private static ToolStripMenuItem? _themeMenu;
        private static ToolStripMenuItem? _langMenu;
        private static ToolStripMenuItem? _dndItem;
        private static ToolStripMenuItem? _compactItem;
        private static ToolStripMenuItem? _testItem;
        private static ToolStripMenuItem? _settingsItem;
        private static ToolStripMenuItem? _folderItem;
        private static ToolStripMenuItem? _quitItem;

        public static void Initialize()
        {
            try
            {
                var icon = CreateAppIcon();
                _contextMenu = new ContextMenuStrip
                {
                    Renderer = new CatppuccinMenuRenderer(),
                    ShowImageMargin = false
                };

                RebuildContextMenu();

                _trayIcon = new NotifyIcon
                {
                    Icon = icon,
                    ContextMenuStrip = _contextMenu,
                    Visible = true
                };
                UpdateTrayTooltip();

                _trayIcon.DoubleClick += (s, e) =>
                {
                    OnSettingsRequested?.Invoke();
                };
            }
            catch { }
        }

        public static void RebuildContextMenu()
        {
            if (_contextMenu == null) return;
            _contextMenu.Items.Clear();
            _sourceMenuItems.Clear();
            _themeMenuItems.Clear();
            _languageMenuItems.Clear();

            // Title header item
            var titleItem = new ToolStripMenuItem("MediaNotif")
            {
                Enabled = false,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(137, 180, 250) // #89b4fa
            };
            _contextMenu.Items.Add(titleItem);
            _contextMenu.Items.Add(new ToolStripSeparator());

            // Do Not Disturb Item
            _dndItem = new ToolStripMenuItem(Loc.DoNotDisturb, null, (s, e) =>
            {
                SettingsManager.Current.DoNotDisturb = !SettingsManager.Current.DoNotDisturb;
                SettingsManager.Save();
                UpdateDndAndCompactChecks();
                UpdateTrayTooltip();
            })
            {
                ForeColor = Color.FromArgb(205, 214, 244),
                Checked = SettingsManager.Current.DoNotDisturb
            };
            _contextMenu.Items.Add(_dndItem);

            // Compact Mode Item
            _compactItem = new ToolStripMenuItem(Loc.CompactMode, null, (s, e) =>
            {
                SettingsManager.Current.CompactMode = !SettingsManager.Current.CompactMode;
                SettingsManager.Save();
                UpdateDndAndCompactChecks();
            })
            {
                ForeColor = Color.FromArgb(205, 214, 244),
                Checked = SettingsManager.Current.CompactMode
            };
            _contextMenu.Items.Add(_compactItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            // Theme Submenu
            _themeMenu = new ToolStripMenuItem(Loc.ThemeMenu)
            {
                ForeColor = Color.FromArgb(205, 214, 244)
            };

            var themes = new[]
            {
                ("AuraNeo", Loc.ThemeAuraNeo),
                ("ClassicDunst", Loc.ThemeClassicDunst),
                ("NordicFrost", Loc.ThemeNordicFrost),
                ("MidnightAmoled", Loc.ThemeMidnightAmoled)
            };

            foreach (var (key, label) in themes)
            {
                var item = new ToolStripMenuItem(label, null, (s, e) => SelectTheme(key))
                {
                    ForeColor = Color.FromArgb(205, 214, 244),
                    Tag = key
                };
                _themeMenuItems.Add(item);
                _themeMenu.DropDownItems.Add(item);
            }
            UpdateThemeMenuCheckmarks();
            _contextMenu.Items.Add(_themeMenu);

            // Source Audio Submenu
            _sourceMenu = new ToolStripMenuItem(Loc.AudioSource)
            {
                ForeColor = Color.FromArgb(205, 214, 244)
            };

            foreach (var (key, fallbackLabel) in Sources)
            {
                string displayLabel = key == "All" ? Loc.AllSources : fallbackLabel;
                var item = new ToolStripMenuItem(displayLabel, null, (s, e) =>
                {
                    SelectSource(key);
                })
                {
                    ForeColor = Color.FromArgb(205, 214, 244),
                    Tag = key
                };
                _sourceMenuItems.Add(item);
                _sourceMenu.DropDownItems.Add(item);
            }
            UpdateSourceMenuCheckmarks();
            _contextMenu.Items.Add(_sourceMenu);

            // Language Submenu
            _langMenu = new ToolStripMenuItem(Loc.LanguageMenu)
            {
                ForeColor = Color.FromArgb(205, 214, 244)
            };

            var frItem = new ToolStripMenuItem(Loc.French, null, (s, e) => SelectLanguage("fr"))
            {
                ForeColor = Color.FromArgb(205, 214, 244),
                Tag = "fr"
            };
            var enItem = new ToolStripMenuItem(Loc.English, null, (s, e) => SelectLanguage("en"))
            {
                ForeColor = Color.FromArgb(205, 214, 244),
                Tag = "en"
            };

            _languageMenuItems.Add(frItem);
            _languageMenuItems.Add(enItem);
            _langMenu.DropDownItems.Add(frItem);
            _langMenu.DropDownItems.Add(enItem);
            UpdateLanguageMenuCheckmarks();

            _contextMenu.Items.Add(_langMenu);
            _contextMenu.Items.Add(new ToolStripSeparator());

            // Test Notification
            _testItem = new ToolStripMenuItem(Loc.TestNotif, null, (s, e) =>
            {
                OnTestRequested?.Invoke();
            })
            {
                ForeColor = Color.FromArgb(205, 214, 244)
            };
            _contextMenu.Items.Add(_testItem);

            // Settings item
            _settingsItem = new ToolStripMenuItem(Loc.Settings, null, (s, e) =>
            {
                OnSettingsRequested?.Invoke();
            })
            {
                ForeColor = Color.FromArgb(205, 214, 244)
            };
            _contextMenu.Items.Add(_settingsItem);

            // Open App Folder
            _folderItem = new ToolStripMenuItem(Loc.ConfigFolder, null, (s, e) =>
            {
                string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MediaNotif");
                if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
                Process.Start(new ProcessStartInfo("explorer.exe", appData) { UseShellExecute = true });
            })
            {
                ForeColor = Color.FromArgb(205, 214, 244)
            };
            _contextMenu.Items.Add(_folderItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            // Quit item
            _quitItem = new ToolStripMenuItem(Loc.Quit, null, (s, e) =>
            {
                Dispose();
                OnQuitRequested?.Invoke();
            })
            {
                ForeColor = Color.FromArgb(243, 139, 168) // #f38ba8
            };
            _contextMenu.Items.Add(_quitItem);
        }

        public static void SelectTheme(string theme)
        {
            SettingsManager.Current.Theme = theme;
            SettingsManager.Save();
            UpdateThemeMenuCheckmarks();

            OnThemeChanged?.Invoke(theme);
        }

        public static void UpdateThemeMenuCheckmarks()
        {
            string currentTheme = SettingsManager.Current.Theme;
            foreach (var item in _themeMenuItems)
            {
                if (item.Tag is string theme)
                {
                    item.Checked = string.Equals(theme, currentTheme, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        public static void SelectSource(string key)
        {
            SettingsManager.Current.MediaSourceFilter = key;
            SettingsManager.Save();
            UpdateSourceMenuCheckmarks();
            UpdateTrayTooltip();

            OnSourceFilterChanged?.Invoke(key);
        }

        public static void SelectLanguage(string lang)
        {
            SettingsManager.Current.Language = lang;
            SettingsManager.Save();
            UpdateLanguageMenuCheckmarks();
            RebuildContextMenu();
            UpdateTrayTooltip();

            OnLanguageChanged?.Invoke(lang);
        }

        public static void UpdateSourceMenuCheckmarks()
        {
            string current = SettingsManager.Current.MediaSourceFilter;
            foreach (var item in _sourceMenuItems)
            {
                if (item.Tag is string key)
                {
                    item.Checked = string.Equals(key, current, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        public static void UpdateLanguageMenuCheckmarks()
        {
            string currentLang = SettingsManager.Current.Language;
            foreach (var item in _languageMenuItems)
            {
                if (item.Tag is string lang)
                {
                    item.Checked = string.Equals(lang, currentLang, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        public static void UpdateDndAndCompactChecks()
        {
            if (_dndItem != null) _dndItem.Checked = SettingsManager.Current.DoNotDisturb;
            if (_compactItem != null) _compactItem.Checked = SettingsManager.Current.CompactMode;
        }

        public static void UpdateTrayTooltip()
        {
            if (_trayIcon == null) return;
            string filterName = SettingsManager.GetFilterDisplayName(SettingsManager.Current.MediaSourceFilter);
            string dnd = SettingsManager.Current.DoNotDisturb ? " [🔕 Muted]" : "";
            string tip = $"MediaNotif{dnd}\nSource: {filterName}";
            if (tip.Length > 63) tip = tip.Substring(0, 63);
            _trayIcon.Text = tip;
        }

        private static Icon CreateAppIcon()
        {
            // Try loading image if present
            try
            {
                string[] possiblePaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", "media", "icon.jpg"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.jpg"),
                    Path.Combine(Directory.GetCurrentDirectory(), "docs", "media", "icon.jpg"),
                    Path.Combine(Directory.GetCurrentDirectory(), "icon.jpg")
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        using var bmpImage = new Bitmap(path);
                        using var squareBmp = new Bitmap(32, 32);
                        using (var g = Graphics.FromImage(squareBmp))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            using var pathShape = new GraphicsPath();
                            pathShape.AddEllipse(1, 1, 30, 30);
                            g.SetClip(pathShape);
                            g.DrawImage(bmpImage, 0, 0, 32, 32);
                        }
                        IntPtr hIcon = squareBmp.GetHicon();
                        return Icon.FromHandle(hIcon);
                    }
                }
            }
            catch { }

            // High-DPI rendered Catppuccin glyph icon
            using var bmp = new Bitmap(32, 32);
            using var gBmp = Graphics.FromImage(bmp);
            gBmp.SmoothingMode = SmoothingMode.AntiAlias;
            gBmp.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (var brush = new LinearGradientBrush(
                new Rectangle(0, 0, 32, 32),
                Color.FromArgb(137, 180, 250), // #89b4fa Blue
                Color.FromArgb(203, 166, 247), // #cba6f7 Mauve
                LinearGradientMode.ForwardDiagonal))
            {
                gBmp.FillEllipse(brush, 1, 1, 30, 30);
            }

            using var font = new Font("Segoe UI Symbol", 15, FontStyle.Bold, GraphicsUnit.Pixel);
            using var textBrush = new SolidBrush(Color.FromArgb(17, 17, 27)); // #11111b Crust
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            gBmp.DrawString("♫", font, textBrush, new RectangleF(0, 0, 32, 32), sf);

            IntPtr fallbackHIcon = bmp.GetHicon();
            return Icon.FromHandle(fallbackHIcon);
        }

        public static void Dispose()
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }
        }
    }

    public class CatppuccinMenuRenderer : ToolStripProfessionalRenderer
    {
        public CatppuccinMenuRenderer() : base(new CatppuccinColorTable()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Selected)
            {
                base.OnRenderMenuItemBackground(e);
                return;
            }

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rc = new Rectangle(2, 0, e.Item.Width - 4, e.Item.Height);
            using var brush = new SolidBrush(Color.FromArgb(49, 50, 68)); // #313244 Surface0
            g.FillRectangle(brush, rc);

            if (e.Item is ToolStripMenuItem mi && mi.Checked)
            {
                using var checkPen = new Pen(Color.FromArgb(137, 180, 250), 2);
                g.DrawRectangle(checkPen, 3, 2, e.Item.Width - 6, e.Item.Height - 4);
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rc = new Rectangle(e.ImageRectangle.Left + 2, e.ImageRectangle.Top + 2, 14, 14);
            using var brush = new SolidBrush(Color.FromArgb(137, 180, 250)); // #89b4fa
            g.FillEllipse(brush, rc);

            using var pen = new Pen(Color.FromArgb(17, 17, 27), 2);
            g.DrawLines(pen, new[]
            {
                new Point(rc.Left + 3, rc.Top + 7),
                new Point(rc.Left + 6, rc.Top + 10),
                new Point(rc.Left + 11, rc.Top + 4)
            });
        }
    }

    public class CatppuccinColorTable : ProfessionalColorTable
    {
        public override Color MenuBorder => Color.FromArgb(49, 50, 68); // #313244
        public override Color ToolStripDropDownBackground => Color.FromArgb(24, 24, 37); // #181825 Mantle
        public override Color MenuItemSelected => Color.FromArgb(49, 50, 68);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(49, 50, 68);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(49, 50, 68);
        public override Color MenuItemBorder => Color.Transparent;
        public override Color SeparatorDark => Color.FromArgb(49, 50, 68);
        public override Color SeparatorLight => Color.Transparent;
        public override Color CheckBackground => Color.FromArgb(49, 50, 68);
        public override Color CheckSelectedBackground => Color.FromArgb(69, 71, 90);
        public override Color CheckPressedBackground => Color.FromArgb(69, 71, 90);
        public override Color ImageMarginGradientBegin => Color.FromArgb(24, 24, 37);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(24, 24, 37);
        public override Color ImageMarginGradientEnd => Color.FromArgb(24, 24, 37);
    }
}
