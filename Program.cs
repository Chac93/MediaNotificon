using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Media.Control;

namespace MediaNotif
{
    class Program
    {
        private static string _lastTrackId = string.Empty;
        private static Dispatcher? _wpfDispatcher;
        private static DunstPopupWindow? _currentPopup;
        private static SettingsWindow? _settingsWindow;

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Any(a => a.Equals("--capture-docs", StringComparison.OrdinalIgnoreCase)))
            {
                SettingsManager.Load();
                CaptureDocumentationScreenshots();
                return;
            }

            bool isTestMode = args.Any(a => a.Equals("--test", StringComparison.OrdinalIgnoreCase));

            using var mutex = new Mutex(true, "MediaNotif_SingleInstanceMutex", out bool createdNew);
            if (!createdNew && !isTestMode)
            {
                try
                {
                    using var ev = EventWaitHandle.OpenExisting("MediaNotif_ShowSettingsEvent");
                    ev.Set();
                }
                catch { }
                return;
            }

            using var showSettingsEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "MediaNotif_ShowSettingsEvent");
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        showSettingsEvent.WaitOne();
                        _wpfDispatcher?.BeginInvoke(OpenSettingsWindow);
                    }
                    catch { break; }
                }
            });

            // Load persistent settings
            SettingsManager.Load();

            var app = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            _wpfDispatcher = app.Dispatcher;

            // Setup System Tray Icon & Right-Click Menu
            TrayService.OnTestRequested += () =>
            {
                _wpfDispatcher?.BeginInvoke(() =>
                {
                    ShowDunstNotification("Starboy", "The Weeknd ft. Daft Punk", null);
                });
            };

            TrayService.OnSettingsRequested += () =>
            {
                _wpfDispatcher?.BeginInvoke(OpenSettingsWindow);
            };

            TrayService.OnQuitRequested += () =>
            {
                _wpfDispatcher?.BeginInvoke(() =>
                {
                    Application.Current.Shutdown();
                });
            };

            TrayService.OnSourceFilterChanged += (newFilter) =>
            {
                _lastTrackId = string.Empty;
                _wpfDispatcher?.BeginInvoke(() =>
                {
                    ShowDunstNotification(
                        Loc.SourceChangedTitle,
                        string.Format(Loc.SourceChangedBody, SettingsManager.GetFilterDisplayName(newFilter)),
                        null
                    );
                });
            };

            TrayService.OnThemeChanged += (newTheme) =>
            {
                _wpfDispatcher?.BeginInvoke(() =>
                {
                    ShowDunstNotification(
                        Loc.ThemeChangedTitle,
                        string.Format(Loc.ThemeChangedBody, SettingsManager.GetThemeDisplayName(newTheme)),
                        null
                    );
                });
            };

            TrayService.OnLanguageChanged += (newLang) =>
            {
                _wpfDispatcher?.BeginInvoke(() =>
                {
                    ShowDunstNotification(
                        Loc.LanguageChangedTitle,
                        Loc.LanguageChangedBody,
                        null
                    );
                });
            };

            TrayService.Initialize();

            // Always show welcoming visual confirmation on startup
            _wpfDispatcher.BeginInvoke(() =>
            {
                string filterName = SettingsManager.GetFilterDisplayName(SettingsManager.Current.MediaSourceFilter);
                ShowDunstNotification(
                    Loc.StartupNotifTitle,
                    string.Format(Loc.StartupNotifBody, filterName),
                    null
                );
            });

            Task.Run(async () =>
            {
                try
                {
                    Log("Background worker starting...");
                    var manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                    Log("GSMTC manager acquired successfully.");

                    void HookSessions()
                    {
                        try
                        {
                            var current = manager.GetCurrentSession();
                            if (current != null)
                            {
                                current.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                                current.MediaPropertiesChanged += OnMediaPropertiesChanged;
                            }
                            foreach (var s in manager.GetSessions())
                            {
                                s.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                                s.MediaPropertiesChanged += OnMediaPropertiesChanged;
                            }
                        }
                        catch { }
                    }

                    async void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession s, MediaPropertiesChangedEventArgs e)
                    {
                        await CheckMediaSessionAsync(manager);
                    }

                    manager.CurrentSessionChanged += async (s, e) =>
                    {
                        HookSessions();
                        await CheckMediaSessionAsync(manager);
                    };

                    manager.SessionsChanged += async (s, e) =>
                    {
                        HookSessions();
                        await CheckMediaSessionAsync(manager);
                    };

                    HookSessions();

                    while (true)
                    {
                        await CheckMediaSessionAsync(manager);
                        await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    Log($"Worker exception: {ex}");
                }
            });

            app.Run();

            // Clean up tray icon on exit
            TrayService.Dispose();
        }

        private static void OpenSettingsWindow()
        {
            if (_settingsWindow != null && _settingsWindow.IsLoaded)
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow();
            _settingsWindow.OnTestRequested += () =>
            {
                ShowDunstNotification("Starboy", "The Weeknd ft. Daft Punk", null);
            };
            _settingsWindow.Closed += (s, e) => _settingsWindow = null;
            _settingsWindow.Show();
        }

        private static async Task CheckMediaSessionAsync(GlobalSystemMediaTransportControlsSessionManager manager)
        {
            try
            {
                var sessions = manager.GetSessions();
                if (sessions == null || sessions.Count == 0) return;

                // 1. Prefer actively playing session first that matches the filter
                foreach (var s in sessions)
                {
                    try
                    {
                        if (!SettingsManager.MatchesSourceFilter(s.SourceAppUserModelId)) continue;

                        var playback = s.GetPlaybackInfo();
                        if (playback != null && playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                        {
                            if (await TryProcessSession(s)) return;
                        }
                    }
                    catch { }
                }

                // 2. Fall back to current session if it matches filter
                var current = manager.GetCurrentSession();
                if (current != null && SettingsManager.MatchesSourceFilter(current.SourceAppUserModelId))
                {
                    if (await TryProcessSession(current)) return;
                }

                // 3. Fall back to any other session matching filter
                foreach (var s in sessions)
                {
                    try
                    {
                        if (!SettingsManager.MatchesSourceFilter(s.SourceAppUserModelId)) continue;
                        if (await TryProcessSession(s)) return;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Log($"CheckMediaSessionAsync error: {ex.Message}");
            }
        }

        private static async Task<bool> TryProcessSession(GlobalSystemMediaTransportControlsSession session)
        {
            try
            {
                var media = await session.TryGetMediaPropertiesAsync();
                if (media == null) return false;

                string title = media.Title ?? string.Empty;
                string artist = media.Artist ?? string.Empty;
                string album = media.AlbumTitle ?? string.Empty;

                if (string.IsNullOrWhiteSpace(title)) return false;

                string trackId = $"{title}|{artist}|{album}";
                if (trackId == _lastTrackId) return true; // Already displayed

                _lastTrackId = trackId;
                Log($"New Track Detected: {title} by {artist}");

                byte[]? coverBytes = null;
                if (media.Thumbnail != null)
                {
                    try
                    {
                        using var streamRef = await media.Thumbnail.OpenReadAsync();
                        using var stream = streamRef.AsStreamForRead();
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        coverBytes = ms.ToArray();
                        Log($"Cover extracted immediately ({coverBytes.Length} bytes).");
                    }
                    catch (Exception ex)
                    {
                        Log($"Immediate cover extraction note: {ex.Message}");
                    }
                }

                _wpfDispatcher?.BeginInvoke(() =>
                {
                    ShowDunstNotification(title, artist, coverBytes, trackId);
                });

                // Asynchronous retry for late-arriving artwork
                if (coverBytes == null || coverBytes.Length == 0)
                {
                    _ = Task.Run(async () =>
                    {
                        for (int attempt = 1; attempt <= 6; attempt++)
                        {
                            await Task.Delay(250 + attempt * 200);
                            try
                            {
                                var freshProps = await session.TryGetMediaPropertiesAsync();
                                if (freshProps?.Thumbnail != null)
                                {
                                    using var streamRef = await freshProps.Thumbnail.OpenReadAsync();
                                    using var stream = streamRef.AsStreamForRead();
                                    using var ms = new MemoryStream();
                                    await stream.CopyToAsync(ms);
                                    byte[] retryBytes = ms.ToArray();
                                    if (retryBytes.Length > 0)
                                    {
                                        Log($"Cover acquired on retry attempt #{attempt} ({retryBytes.Length} bytes).");
                                        _wpfDispatcher?.BeginInvoke(() =>
                                        {
                                            if (_currentPopup != null && _currentPopup.IsLoaded && _currentPopup.CurrentTrackId == trackId)
                                            {
                                                _currentPopup.UpdateCover(retryBytes);
                                            }
                                        });
                                        break;
                                    }
                                }
                            }
                            catch { }
                        }
                    });
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void ShowDunstNotification(string title, string artist, byte[]? coverBytes, string trackId = "")
        {
            if (SettingsManager.Current.DoNotDisturb)
            {
                Log("Notification muted (Do Not Disturb is active).");
                return;
            }

            try
            {
                _currentPopup?.CloseInstant();
                _currentPopup = new DunstPopupWindow(
                    title,
                    artist,
                    coverBytes,
                    SettingsManager.Current.DisplayDurationSeconds,
                    SettingsManager.Current.NotificationWidth,
                    SettingsManager.Current.Position,
                    SettingsManager.Current.CompactMode,
                    SettingsManager.Current.Theme,
                    trackId
                );
                _currentPopup.Show();
            }
            catch (Exception ex)
            {
                Log($"ShowDunstNotification error: {ex}");
            }
        }

        public static void Log(string msg)
        {
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MediaNotif");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, "medianotif.log");
                File.AppendAllText(file, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\n");
            }
            catch { }
        }

        private static void CaptureDocumentationScreenshots()
        {
            var app = new Application();
            app.Startup += (s, e) =>
            {
                try
                {
                    string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", "media");
                    if (!Directory.Exists(outDir))
                    {
                        outDir = Path.Combine(Directory.GetCurrentDirectory(), "docs", "media");
                        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
                    }

                    byte[]? sampleArt = null;
                    string iconPath = Path.Combine(outDir, "icon.jpg");
                    if (File.Exists(iconPath)) sampleArt = File.ReadAllBytes(iconPath);

                    var captures = new (string Filename, string Title, string Artist, string Theme, bool Compact)[]
                    {
                        ("theme_auraneo.png", "Starboy", "The Weeknd ft. Daft Punk", "AuraNeo", false),
                        ("theme_classicdunst.png", "Midnight City", "M83 — Hurry Up, We're Dreaming", "ClassicDunst", false),
                        ("theme_nordicfrost.png", "Resonance", "HOME — Odyssey", "NordicFrost", false),
                        ("theme_midnightamoled.png", "After Dark", "Mr.Kitty — Time", "MidnightAmoled", false),
                        ("theme_compact.png", "Blinding Lights", "The Weeknd", "AuraNeo", true)
                    };

                    foreach (var item in captures)
                    {
                        var notif = new DunstPopupWindow(
                            item.Title,
                            item.Artist,
                            sampleArt,
                            9999,
                            420,
                            "TopRight",
                            item.Compact,
                            item.Theme
                        );
                        SaveWindowAsPng(notif, Path.Combine(outDir, item.Filename));
                    }

                    // Capture Settings Window
                    var settings = new SettingsWindow();
                    SaveWindowAsPng(settings, Path.Combine(outDir, "settings_window.png"));

                    Console.WriteLine("All documentation screenshots captured successfully in " + outDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Capture error: " + ex);
                }
                finally
                {
                    app.Shutdown();
                }
            };
            app.Run();
        }

        private static void SaveWindowAsPng(Window win, string outputPath)
        {
            if (win.Content is FrameworkElement element)
            {
                double targetWidth = win.Width > 0 ? win.Width : 420;
                double targetHeight = win.Height > 0 ? win.Height : double.PositiveInfinity;

                element.Measure(new Size(targetWidth, targetHeight));
                double w = element.DesiredSize.Width > 0 ? element.DesiredSize.Width : targetWidth;
                double h = element.DesiredSize.Height > 0 ? element.DesiredSize.Height : (win.Height > 0 ? win.Height : 90);
                element.Arrange(new Rect(0, 0, w, h));
                element.UpdateLayout();

                int pixelWidth = (int)Math.Ceiling(w);
                int pixelHeight = (int)Math.Ceiling(h);

                var rtb = new RenderTargetBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(element);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                using var fs = File.Open(outputPath, FileMode.Create, FileAccess.Write);
                encoder.Save(fs);
            }
        }
    }

    public class DunstPopupWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public string CurrentTrackId { get; }
        private readonly double _duration;
        private readonly string _position;
        private readonly bool _isCompact;
        private readonly string _theme;
        private DispatcherTimer? _dismissTimer;
        private bool _isClosing = false;
        private Border? _coverContainer;

        public DunstPopupWindow(
            string title,
            string artist,
            byte[]? coverBytes,
            double duration,
            double width,
            string position,
            bool isCompact,
            string theme,
            string trackId = "")
        {
            CurrentTrackId = trackId;
            _duration = duration;
            _position = position;
            _isCompact = isCompact;
            _theme = theme;

            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            ShowInTaskbar = false;
            Topmost = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            Focusable = false;

            double targetWidth = Math.Max(320, width);
            Width = targetWidth;
            SizeToContent = SizeToContent.Height;

            bool isClassic = string.Equals(_theme, "ClassicDunst", StringComparison.OrdinalIgnoreCase);
            bool isNordic = string.Equals(_theme, "NordicFrost", StringComparison.OrdinalIgnoreCase);
            bool isAmoled = string.Equals(_theme, "MidnightAmoled", StringComparison.OrdinalIgnoreCase);

            Brush bgBrush = isAmoled
                ? new SolidColorBrush(Color.FromRgb(0, 0, 0))
                : (isNordic
                    ? new SolidColorBrush(Color.FromRgb(46, 52, 64))
                    : new SolidColorBrush(Color.FromRgb(24, 24, 37)));

            Brush borderBrush = isClassic
                ? new SolidColorBrush(Color.FromRgb(137, 180, 250)) // Classic Dunst full border
                : (isNordic
                    ? new SolidColorBrush(Color.FromRgb(76, 86, 106))
                    : (isAmoled
                        ? new SolidColorBrush(Color.FromRgb(39, 39, 42))
                        : new SolidColorBrush(Color.FromRgb(49, 50, 68))));

            Brush accentBrush = isNordic
                ? new SolidColorBrush(Color.FromRgb(136, 192, 208))
                : (isAmoled
                    ? new SolidColorBrush(Color.FromRgb(243, 139, 168))
                    : new SolidColorBrush(Color.FromRgb(137, 180, 250)));

            var rootBorder = new Border
            {
                Background = bgBrush,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(isClassic ? 1.8 : 1.2),
                CornerRadius = new CornerRadius(_isCompact ? 10 : (isClassic ? 12 : 14)),
                Cursor = Cursors.Hand,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 24,
                    ShadowDepth = 6,
                    Opacity = 0.85
                }
            };

            var rootGrid = new Grid();
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Top Gradient Bar for modern themes
            if (!isClassic)
            {
                var topGradient = isNordic
                    ? new LinearGradientBrush(Color.FromRgb(136, 192, 208), Color.FromRgb(129, 161, 193), new Point(0, 0), new Point(1, 0))
                    : (isAmoled
                        ? new LinearGradientBrush(Color.FromRgb(243, 139, 168), Color.FromRgb(203, 166, 247), new Point(0, 0), new Point(1, 0))
                        : new LinearGradientBrush(Color.FromRgb(137, 180, 250), Color.FromRgb(203, 166, 247), new Point(0, 0), new Point(1, 0)));

                var topAccent = new Border
                {
                    Height = 3.0,
                    CornerRadius = new CornerRadius(_isCompact ? 10 : 14, _isCompact ? 10 : 14, 0, 0),
                    Background = topGradient
                };
                Grid.SetRow(topAccent, 0);
                rootGrid.Children.Add(topAccent);
            }

            var contentBorder = new Border
            {
                Padding = _isCompact ? new Thickness(12, 8, 14, 8) : new Thickness(16, 14, 18, 14)
            };
            Grid.SetRow(contentBorder, 1);
            rootGrid.Children.Add(contentBorder);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            double coverSize = _isCompact ? 28 : (isClassic ? 54 : 58);

            // Cover Art Container
            _coverContainer = new Border
            {
                Width = coverSize,
                Height = coverSize,
                Margin = new Thickness(0, 0, _isCompact ? 10 : 16, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            SetCoverContent(coverBytes);

            Grid.SetColumn(_coverContainer, 0);
            grid.Children.Add(_coverContainer);

            if (_isCompact)
            {
                var lineStack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    MaxWidth = targetWidth - 50
                };

                var titleBlock = new TextBlock
                {
                    Text = title,
                    Foreground = isNordic ? new SolidColorBrush(Color.FromRgb(236, 239, 244)) : (isAmoled ? Brushes.White : new SolidColorBrush(Color.FromRgb(205, 214, 244))),
                    FontWeight = FontWeights.Bold,
                    FontSize = 12.5,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var separatorBlock = new TextBlock
                {
                    Text = "  •  ",
                    Foreground = accentBrush,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var artistBlock = new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(artist) ? "Unknown" : artist,
                    Foreground = isNordic ? new SolidColorBrush(Color.FromRgb(216, 222, 233)) : (isAmoled ? new SolidColorBrush(Color.FromRgb(161, 161, 170)) : new SolidColorBrush(Color.FromRgb(186, 194, 222))),
                    FontSize = 11.5,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center
                };

                lineStack.Children.Add(titleBlock);
                lineStack.Children.Add(separatorBlock);
                lineStack.Children.Add(artistBlock);

                Grid.SetColumn(lineStack, 1);
                grid.Children.Add(lineStack);
            }
            else
            {
                var textStack = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    MaxWidth = targetWidth - 30
                };

                if (isClassic)
                {
                    textStack.Children.Add(new TextBlock
                    {
                        Text = "NOW PLAYING",
                        FontSize = 9.5,
                        FontWeight = FontWeights.ExtraBold,
                        Foreground = accentBrush,
                        Margin = new Thickness(0, 0, 0, 4)
                    });
                }

                var titleBlock = new TextBlock
                {
                    Text = title,
                    Foreground = isNordic ? new SolidColorBrush(Color.FromRgb(236, 239, 244)) : (isAmoled ? Brushes.White : new SolidColorBrush(Color.FromRgb(205, 214, 244))),
                    FontWeight = FontWeights.Bold,
                    FontSize = isClassic ? 13.5 : 14.5,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(0, 0, 0, 3)
                };

                var artistBlock = new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(artist) ? "Unknown" : artist,
                    Foreground = isNordic ? new SolidColorBrush(Color.FromRgb(216, 222, 233)) : (isAmoled ? new SolidColorBrush(Color.FromRgb(161, 161, 170)) : new SolidColorBrush(Color.FromRgb(186, 194, 222))),
                    FontSize = 12.5,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                textStack.Children.Add(titleBlock);
                textStack.Children.Add(artistBlock);

                Grid.SetColumn(textStack, 1);
                grid.Children.Add(textStack);
            }

            contentBorder.Child = grid;
            rootBorder.Child = rootGrid;
            Content = rootBorder;

            // Click to dismiss
            rootBorder.MouseLeftButtonDown += (s, e) => CloseWithAnimation();

            Loaded += OnWindowLoaded;
        }

        public void UpdateCover(byte[]? coverBytes)
        {
            if (_coverContainer == null || coverBytes == null || coverBytes.Length == 0) return;
            SetCoverContent(coverBytes);
        }

        private void SetCoverContent(byte[]? coverBytes)
        {
            if (_coverContainer == null) return;

            bool isClassic = string.Equals(_theme, "ClassicDunst", StringComparison.OrdinalIgnoreCase);
            bool isNordic = string.Equals(_theme, "NordicFrost", StringComparison.OrdinalIgnoreCase);
            bool isAmoled = string.Equals(_theme, "MidnightAmoled", StringComparison.OrdinalIgnoreCase);

            Brush accentBrush = isNordic
                ? new SolidColorBrush(Color.FromRgb(136, 192, 208))
                : (isAmoled
                    ? new SolidColorBrush(Color.FromRgb(243, 139, 168))
                    : new SolidColorBrush(Color.FromRgb(137, 180, 250)));

            if (coverBytes != null && coverBytes.Length > 0)
            {
                try
                {
                    var image = new BitmapImage();
                    using (var ms = new MemoryStream(coverBytes))
                    {
                        ms.Position = 0;
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.None;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        image.Freeze();
                    }

                    _coverContainer.CornerRadius = new CornerRadius(_isCompact ? 6 : (isClassic ? 8 : 10));
                    _coverContainer.Background = Brushes.Transparent;
                    _coverContainer.ClipToBounds = true;
                    _coverContainer.Child = new Image
                    {
                        Source = image,
                        Stretch = Stretch.UniformToFill
                    };
                    return;
                }
                catch (Exception ex)
                {
                    Program.Log($"SetCoverContent image load error: {ex.Message}");
                }
            }

            // Fallback Album Art
            _coverContainer.CornerRadius = new CornerRadius(_isCompact ? 6 : (isClassic ? 8 : 10));
            _coverContainer.Background = new SolidColorBrush(Color.FromRgb(30, 30, 46));
            _coverContainer.Child = new TextBlock
            {
                Text = "♫",
                FontSize = _isCompact ? 14 : 22,
                Foreground = accentBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            int exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);

            SetWindowPos(helper.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Reposition();

            // Fade in animation
            Opacity = 0.0;
            var anim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(220))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(OpacityProperty, anim);

            _dismissTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_duration)
            };
            _dismissTimer.Tick += (s, ev) =>
            {
                _dismissTimer.Stop();
                CloseWithAnimation();
            };
            _dismissTimer.Start();
        }

        private void Reposition()
        {
            var workArea = SystemParameters.WorkArea;
            int marginX = SettingsManager.Current.MarginX;
            int marginY = SettingsManager.Current.MarginY;

            switch (_position)
            {
                case "TopLeft":
                    Left = workArea.Left + marginX;
                    Top = workArea.Top + marginY;
                    break;
                case "BottomLeft":
                    Left = workArea.Left + marginX;
                    Top = workArea.Bottom - ActualHeight - marginY;
                    break;
                case "BottomRight":
                    Left = workArea.Right - ActualWidth - marginX;
                    Top = workArea.Bottom - ActualHeight - marginY;
                    break;
                case "TopCenter":
                    Left = workArea.Left + (workArea.Width - ActualWidth) / 2.0;
                    Top = workArea.Top + marginY;
                    break;
                case "TopRight":
                default:
                    Left = workArea.Right - ActualWidth - marginX;
                    Top = workArea.Top + marginY;
                    break;
            }
        }

        public void CloseWithAnimation()
        {
            if (_isClosing) return;
            _isClosing = true;
            _dismissTimer?.Stop();

            var anim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            anim.Completed += (s, e) => Close();
            BeginAnimation(OpacityProperty, anim);
        }

        public void CloseInstant()
        {
            _isClosing = true;
            _dismissTimer?.Stop();
            Close();
        }
    }
}
