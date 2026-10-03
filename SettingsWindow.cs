using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Imaging;

namespace MediaNotif
{
    public class SettingsWindow : Window
    {
        private string _selectedPosition;
        private string _selectedSource;
        private string _selectedLanguage;
        private string _selectedTheme;
        private double _selectedDuration;
        private double _selectedWidth;
        private bool _selectedStartup;
        private bool _selectedCompact;
        private bool _selectedDnd;

        private readonly Dictionary<string, Border> _positionButtons = new();
        private readonly Dictionary<string, Border> _sourceButtons = new();
        private readonly Dictionary<string, Border> _languageButtons = new();
        private readonly Dictionary<string, Border> _themeButtons = new();

        private readonly TextBlock _headerSubtitle;
        private readonly TextBlock _previewHeaderTitle;
        private readonly TextBlock _previewHeaderSub;
        private readonly TextBlock _themeHeaderTitle;
        private readonly TextBlock _themeHeaderSub;
        private readonly TextBlock _posHeaderTitle;
        private readonly TextBlock _posHeaderSub;
        private readonly TextBlock _srcHeaderTitle;
        private readonly TextBlock _srcHeaderSub;
        private readonly TextBlock _dimHeaderTitle;
        private readonly TextBlock _dimHeaderSub;
        private readonly TextBlock _langHeaderTitle;
        private readonly TextBlock _langHeaderSub;
        private readonly TextBlock _sysHeaderTitle;
        private readonly TextBlock _sysHeaderSub;

        private readonly TextBlock _durTitle;
        private readonly TextBlock _widthTitle;
        private readonly TextBlock _autoStartTitle;
        private readonly TextBlock _autoStartSub;
        private readonly TextBlock _compactTitle;
        private readonly TextBlock _compactSub;
        private readonly TextBlock _dndTitle;
        private readonly TextBlock _dndSub;

        private readonly TextBlock _durationBadge;
        private readonly TextBlock _widthBadge;
        private readonly Slider _durationSlider;
        private readonly Slider _widthSlider;

        private readonly Border _toggleSwitchStartup;
        private readonly Ellipse _toggleThumbStartup;
        private readonly Border _toggleSwitchCompact;
        private readonly Ellipse _toggleThumbCompact;
        private readonly Border _toggleSwitchDnd;
        private readonly Ellipse _toggleThumbDnd;

        private readonly Border _previewNotification;
        private readonly Border _previewContainer;

        private readonly TextBlock _testBtnText;
        private readonly TextBlock _saveBtnText;
        private readonly TextBlock _closeBtnText;
        private readonly Border _saveBtn;

        private DispatcherTimer? _saveFeedbackTimer;

        public event Action? OnTestRequested;

        public SettingsWindow()
        {
            _selectedPosition = SettingsManager.Current.Position;
            _selectedSource = SettingsManager.Current.MediaSourceFilter;
            _selectedLanguage = SettingsManager.Current.Language;
            _selectedTheme = SettingsManager.Current.Theme;
            _selectedDuration = SettingsManager.Current.DisplayDurationSeconds;
            _selectedWidth = Math.Min(SettingsManager.Current.NotificationWidth, 480);
            _selectedStartup = SettingsManager.Current.StartWithWindows;
            _selectedCompact = SettingsManager.Current.CompactMode;
            _selectedDnd = SettingsManager.Current.DoNotDisturb;

            Title = "MediaNotif — Settings";
            Width = 520;
            Height = 760;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;

            // Apply modern dark sleek scrollbar style (Catppuccin Mocha)
            try
            {
                string scrollbarXaml = @"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <Style TargetType=""{x:Type ScrollBar}"">
        <Setter Property=""Stylus.IsPressAndHoldEnabled"" Value=""false""/>
        <Setter Property=""Stylus.IsFlicksEnabled"" Value=""false""/>
        <Setter Property=""Background"" Value=""Transparent""/>
        <Setter Property=""BorderBrush"" Value=""Transparent""/>
        <Setter Property=""Width"" Value=""8""/>
        <Setter Property=""MinWidth"" Value=""8""/>
        <Setter Property=""Template"">
            <Setter.Value>
                <ControlTemplate TargetType=""{x:Type ScrollBar}"">
                    <Grid Background=""Transparent"" SnapsToDevicePixels=""true"">
                        <Track x:Name=""PART_Track"" IsDirectionReversed=""true"">
                            <Track.DecreaseRepeatButton>
                                <RepeatButton Command=""{x:Static ScrollBar.PageUpCommand}"" Opacity=""0"" Focusable=""false"">
                                    <RepeatButton.Template>
                                        <ControlTemplate TargetType=""{x:Type RepeatButton}"">
                                            <Rectangle Fill=""Transparent""/>
                                        </ControlTemplate>
                                    </RepeatButton.Template>
                                </RepeatButton>
                            </Track.DecreaseRepeatButton>
                            <Track.IncreaseRepeatButton>
                                <RepeatButton Command=""{x:Static ScrollBar.PageDownCommand}"" Opacity=""0"" Focusable=""false"">
                                    <RepeatButton.Template>
                                        <ControlTemplate TargetType=""{x:Type RepeatButton}"">
                                            <Rectangle Fill=""Transparent""/>
                                        </ControlTemplate>
                                    </RepeatButton.Template>
                                </RepeatButton>
                            </Track.IncreaseRepeatButton>
                            <Track.Thumb>
                                <Thumb Focusable=""false"">
                                    <Thumb.Template>
                                        <ControlTemplate TargetType=""{x:Type Thumb}"">
                                            <Border x:Name=""thumbBorder"" Background=""#45475a"" CornerRadius=""4"" Margin=""1,2,1,2""/>
                                            <ControlTemplate.Triggers>
                                                <Trigger Property=""IsMouseOver"" Value=""true"">
                                                    <Setter TargetName=""thumbBorder"" Property=""Background"" Value=""#89b4fa""/>
                                                </Trigger>
                                                <Trigger Property=""IsDragging"" Value=""true"">
                                                    <Setter TargetName=""thumbBorder"" Property=""Background"" Value=""#b4befe""/>
                                                </Trigger>
                                            </ControlTemplate.Triggers>
                                        </ControlTemplate>
                                    </Thumb.Template>
                                </Thumb>
                            </Track.Thumb>
                        </Track>
                    </Grid>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>";

                var rd = (ResourceDictionary)System.Windows.Markup.XamlReader.Parse(scrollbarXaml);
                Resources.MergedDictionaries.Add(rd);
            }
            catch { }

            // Root Window Frame with Deep Drop Shadow
            var rootBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 46)),   // #1e1e2e Base
                BorderBrush = new SolidColorBrush(Color.FromRgb(137, 180, 250)), // #89b4fa Blue
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(16),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 32,
                    ShadowDepth = 8,
                    Opacity = 0.85
                }
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Scrollable Body
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Footer

            // ==========================================
            // 1. HEADER (Draggable, Modern Branding)
            // ==========================================
            var header = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)), // #181825 Mantle
                CornerRadius = new CornerRadius(14, 14, 0, 0),
                Padding = new Thickness(20, 16, 16, 16),
                BorderBrush = new SolidColorBrush(Color.FromRgb(49, 50, 68)), // #313244
                BorderThickness = new Thickness(0, 0, 0, 1),
                Cursor = Cursors.SizeAll
            };
            header.MouseLeftButtonDown += (s, e) => DragMove();

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var brandStack = new StackPanel { Orientation = Orientation.Horizontal };

            // Brand Icon Badge (Using generated icon or neon gradient fallback)
            var iconBadge = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(137, 180, 250)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 12, 0),
                ClipToBounds = true
            };

            bool loadedImg = false;
            try
            {
                string[] possiblePaths = new[]
                {
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", "media", "icon.jpg"),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.jpg"),
                    System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "docs", "media", "icon.jpg"),
                    System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "icon.jpg")
                };

                foreach (var path in possiblePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.UriSource = new Uri(System.IO.Path.GetFullPath(path));
                        bmp.EndInit();
                        bmp.Freeze();

                        iconBadge.Child = new Image
                        {
                            Source = bmp,
                            Width = 36,
                            Height = 36,
                            Stretch = Stretch.UniformToFill
                        };
                        Icon = bmp;
                        loadedImg = true;
                        break;
                    }
                }
            }
            catch { }

            if (!loadedImg)
            {
                iconBadge.Background = new LinearGradientBrush(
                    Color.FromRgb(137, 180, 250), // #89b4fa
                    Color.FromRgb(203, 166, 247), // #cba6f7 Mauve
                    new Point(0, 0),
                    new Point(1, 1)
                );
                iconBadge.Child = new TextBlock
                {
                    Text = "♫",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(17, 17, 27)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
            brandStack.Children.Add(iconBadge);

            var titleInfoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            titleInfoStack.Children.Add(new TextBlock
            {
                Text = Loc.AppTitle,
                FontWeight = FontWeights.ExtraBold,
                FontSize = 16,
                Foreground = Brushes.White
            });
            _headerSubtitle = new TextBlock
            {
                Text = Loc.AppSubtitle,
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Color.FromRgb(166, 173, 200)) // #a6adc8
            };
            titleInfoStack.Children.Add(_headerSubtitle);
            brandStack.Children.Add(titleInfoStack);

            Grid.SetColumn(brandStack, 0);
            headerGrid.Children.Add(brandStack);

            // Close button in header (✕)
            var closeHeaderBtn = new Border
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(15),
                Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = "✕",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            closeHeaderBtn.MouseEnter += (s, e) => closeHeaderBtn.Background = new SolidColorBrush(Color.FromRgb(243, 139, 168)); // Red hover
            closeHeaderBtn.MouseLeave += (s, e) => closeHeaderBtn.Background = new SolidColorBrush(Color.FromRgb(49, 50, 68));
            closeHeaderBtn.MouseLeftButtonDown += (s, e) => Close();

            Grid.SetColumn(closeHeaderBtn, 1);
            headerGrid.Children.Add(closeHeaderBtn);

            header.Child = headerGrid;
            Grid.SetRow(header, 0);
            mainGrid.Children.Add(header);

            // ==========================================
            // 2. SCROLLABLE BODY
            // ==========================================
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(20, 16, 20, 16)
            };

            var bodyStack = new StackPanel();

            // --- SECTION 1: LANGUE / LANGUAGE ---
            var langHeader = CreateHeaderSection(Loc.LanguageTitle, Loc.LanguageSubtitle, out _langHeaderTitle, out _langHeaderSub);
            bodyStack.Children.Add(langHeader);

            var langGrid = new Grid { Margin = new Thickness(0, 0, 0, 18) };
            langGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            langGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            langGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var btnFr = CreateLanguageButton("fr", Loc.French);
            var btnEn = CreateLanguageButton("en", Loc.English);

            Grid.SetColumn(btnFr, 0);
            Grid.SetColumn(btnEn, 2);

            langGrid.Children.Add(btnFr);
            langGrid.Children.Add(btnEn);
            bodyStack.Children.Add(langGrid);
            HighlightSelectedLanguage();

            // --- SECTION 2: THÈME VISUEL (VISUAL THEME) ---
            var themeHeader = CreateHeaderSection(Loc.ThemeTitle, Loc.ThemeSubtitle, out _themeHeaderTitle, out _themeHeaderSub);
            bodyStack.Children.Add(themeHeader);

            var themeGrid = new Grid { Margin = new Thickness(0, 0, 0, 18) };
            themeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            themeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            themeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            themeGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            themeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            themeGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var btnAura = CreateThemeButton("AuraNeo", Loc.ThemeAuraNeo);
            var btnDunst = CreateThemeButton("ClassicDunst", Loc.ThemeClassicDunst);
            var btnNordic = CreateThemeButton("NordicFrost", Loc.ThemeNordicFrost);
            var btnAmoled = CreateThemeButton("MidnightAmoled", Loc.ThemeMidnightAmoled);

            Grid.SetRow(btnAura, 0); Grid.SetColumn(btnAura, 0);
            Grid.SetRow(btnDunst, 0); Grid.SetColumn(btnDunst, 2);
            Grid.SetRow(btnNordic, 2); Grid.SetColumn(btnNordic, 0);
            Grid.SetRow(btnAmoled, 2); Grid.SetColumn(btnAmoled, 2);

            themeGrid.Children.Add(btnAura);
            themeGrid.Children.Add(btnDunst);
            themeGrid.Children.Add(btnNordic);
            themeGrid.Children.Add(btnAmoled);
            bodyStack.Children.Add(themeGrid);
            HighlightSelectedTheme();

            // --- SECTION 3: APERÇU EN DIRECT (LIVE PREVIEW) ---
            var prevHeader = CreateHeaderSection(Loc.LivePreviewTitle, Loc.LivePreviewSubtitle, out _previewHeaderTitle, out _previewHeaderSub);
            bodyStack.Children.Add(prevHeader);

            _previewContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 18),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            _previewNotification = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(137, 180, 250)),
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = Math.Min(_selectedWidth, 440)
            };

            UpdatePreviewLayout();

            _previewContainer.Child = _previewNotification;
            bodyStack.Children.Add(_previewContainer);

            // --- SECTION 3: POSITION À L'ÉCRAN ---
            var posHeader = CreateHeaderSection(Loc.PositionTitle, Loc.PositionSubtitle, out _posHeaderTitle, out _posHeaderSub);
            bodyStack.Children.Add(posHeader);

            var positionGrid = new Grid { Margin = new Thickness(0, 0, 0, 18) };
            positionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            positionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            positionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            positionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            positionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            positionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            positionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            positionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var btnTopLeft = CreatePositionButton("TopLeft", Loc.PosTopLeft);
            var btnTopRight = CreatePositionButton("TopRight", Loc.PosTopRight);
            var btnBottomLeft = CreatePositionButton("BottomLeft", Loc.PosBottomLeft);
            var btnBottomRight = CreatePositionButton("BottomRight", Loc.PosBottomRight);
            var btnTopCenter = CreatePositionButton("TopCenter", Loc.PosTopCenter);

            Grid.SetRow(btnTopLeft, 0); Grid.SetColumn(btnTopLeft, 0);
            Grid.SetRow(btnTopRight, 0); Grid.SetColumn(btnTopRight, 2);
            Grid.SetRow(btnBottomLeft, 2); Grid.SetColumn(btnBottomLeft, 0);
            Grid.SetRow(btnBottomRight, 2); Grid.SetColumn(btnBottomRight, 2);
            Grid.SetRow(btnTopCenter, 4); Grid.SetColumn(btnTopCenter, 0); Grid.SetColumnSpan(btnTopCenter, 3);

            positionGrid.Children.Add(btnTopLeft);
            positionGrid.Children.Add(btnTopRight);
            positionGrid.Children.Add(btnBottomLeft);
            positionGrid.Children.Add(btnBottomRight);
            positionGrid.Children.Add(btnTopCenter);

            bodyStack.Children.Add(positionGrid);
            HighlightSelectedPosition();

            // --- SECTION 4: SOURCE AUDIO ---
            var srcHeader = CreateHeaderSection(Loc.SourceTitle, Loc.SourceSubtitle, out _srcHeaderTitle, out _srcHeaderSub);
            bodyStack.Children.Add(srcHeader);

            var sourceGrid = new Grid { Margin = new Thickness(0, 0, 0, 18) };
            sourceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sourceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            sourceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sourceGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            sourceGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            sourceGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            sourceGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            sourceGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            sourceGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            sourceGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var srcAll = CreateSourceButton("All", Loc.AllSources);
            var srcYtm = CreateSourceButton("YouTubeMusic", "🎵  YouTube Music");
            var srcSpotify = CreateSourceButton("Spotify", "🟢  Spotify");
            var srcZen = CreateSourceButton("Zen", "🌀  Zen Browser");
            var srcChrome = CreateSourceButton("Chrome", "🔴  Google Chrome");
            var srcFirefox = CreateSourceButton("Firefox", "🦊  Mozilla Firefox");
            var srcEdge = CreateSourceButton("Edge", "🌊  Microsoft Edge");
            var srcBrave = CreateSourceButton("Brave", "🦁  Brave Browser");

            Grid.SetRow(srcAll, 0); Grid.SetColumn(srcAll, 0);
            Grid.SetRow(srcYtm, 0); Grid.SetColumn(srcYtm, 2);
            Grid.SetRow(srcSpotify, 2); Grid.SetColumn(srcSpotify, 0);
            Grid.SetRow(srcZen, 2); Grid.SetColumn(srcZen, 2);
            Grid.SetRow(srcChrome, 4); Grid.SetColumn(srcChrome, 0);
            Grid.SetRow(srcFirefox, 4); Grid.SetColumn(srcFirefox, 2);
            Grid.SetRow(srcEdge, 6); Grid.SetColumn(srcEdge, 0);
            Grid.SetRow(srcBrave, 6); Grid.SetColumn(srcBrave, 2);

            sourceGrid.Children.Add(srcAll);
            sourceGrid.Children.Add(srcYtm);
            sourceGrid.Children.Add(srcSpotify);
            sourceGrid.Children.Add(srcZen);
            sourceGrid.Children.Add(srcChrome);
            sourceGrid.Children.Add(srcFirefox);
            sourceGrid.Children.Add(srcEdge);
            sourceGrid.Children.Add(srcBrave);

            bodyStack.Children.Add(sourceGrid);
            HighlightSelectedSource();

            // --- SECTION 5: DIMENSIONS & DURÉE ---
            var dimHeader = CreateHeaderSection(Loc.DimensionsTitle, Loc.DimensionsSubtitle, out _dimHeaderTitle, out _dimHeaderSub);
            bodyStack.Children.Add(dimHeader);

            var slidersCard = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16, 14, 16, 14),
                Margin = new Thickness(0, 0, 0, 18)
            };

            var slidersStack = new StackPanel();

            // Durée
            var durHeaderGrid = new Grid();
            durHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            durHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _durTitle = new TextBlock
            {
                Text = Loc.DurationLabel,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            Grid.SetColumn(_durTitle, 0);
            durHeaderGrid.Children.Add(_durTitle);

            _durationBadge = CreatePillBadge($"{_selectedDuration:0.0} s");
            Grid.SetColumn(_durationBadge, 1);
            durHeaderGrid.Children.Add(_durationBadge);
            slidersStack.Children.Add(durHeaderGrid);

            _durationSlider = new Slider
            {
                Minimum = 1.5,
                Maximum = 8.0,
                Value = _selectedDuration,
                TickFrequency = 0.5,
                IsSnapToTickEnabled = true,
                Margin = new Thickness(0, 8, 0, 8)
            };
            _durationSlider.ValueChanged += (s, e) =>
            {
                _selectedDuration = e.NewValue;
                _durationBadge.Text = $"{e.NewValue:0.0} s";
            };
            slidersStack.Children.Add(_durationSlider);

            // Quick pills for duration
            var durPills = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 14) };
            durPills.Children.Add(CreateQuickPill(Loc.DurShort, () => _durationSlider.Value = 2.5));
            durPills.Children.Add(CreateQuickPill(Loc.DurStd, () => _durationSlider.Value = 3.5));
            durPills.Children.Add(CreateQuickPill(Loc.DurLong, () => _durationSlider.Value = 5.0));
            slidersStack.Children.Add(durPills);

            // Largeur
            var widthHeaderGrid = new Grid();
            widthHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            widthHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _widthTitle = new TextBlock
            {
                Text = Loc.WidthLabel,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            Grid.SetColumn(_widthTitle, 0);
            widthHeaderGrid.Children.Add(_widthTitle);

            _widthBadge = CreatePillBadge($"{_selectedWidth:0} px");
            Grid.SetColumn(_widthBadge, 1);
            widthHeaderGrid.Children.Add(_widthBadge);
            slidersStack.Children.Add(widthHeaderGrid);

            _widthSlider = new Slider
            {
                Minimum = 340,
                Maximum = 480,
                Value = Math.Min(_selectedWidth, 480),
                TickFrequency = 10,
                IsSnapToTickEnabled = true,
                Margin = new Thickness(0, 8, 0, 8)
            };
            _widthSlider.ValueChanged += (s, e) =>
            {
                _selectedWidth = e.NewValue;
                _widthBadge.Text = $"{e.NewValue:0} px";
                _previewNotification.Width = Math.Min(e.NewValue, 440);
            };
            slidersStack.Children.Add(_widthSlider);

            // Quick pills for width
            var widthPills = new StackPanel { Orientation = Orientation.Horizontal };
            widthPills.Children.Add(CreateQuickPill(Loc.WidthCompact, () => _widthSlider.Value = 360));
            widthPills.Children.Add(CreateQuickPill(Loc.WidthStd, () => _widthSlider.Value = 420));
            widthPills.Children.Add(CreateQuickPill(Loc.WidthMax, () => _widthSlider.Value = 480));
            slidersStack.Children.Add(widthPills);

            slidersCard.Child = slidersStack;
            bodyStack.Children.Add(slidersCard);

            // --- SECTION 6: SYSTÈME & MODES (TOGGLE SWITCHES) ---
            var sysHeader = CreateHeaderSection(Loc.SystemTitle, Loc.SystemSubtitle, out _sysHeaderTitle, out _sysHeaderSub);
            bodyStack.Children.Add(sysHeader);

            var sysCardsStack = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

            // 1. Compact Mode Toggle Card
            var compactCard = CreateToggleCard(
                Loc.CompactMode,
                Loc.CompactModeSub,
                _selectedCompact,
                out _compactTitle,
                out _compactSub,
                out _toggleSwitchCompact,
                out _toggleThumbCompact
            );
            var swCompact = _toggleSwitchCompact;
            var thCompact = _toggleThumbCompact;
            compactCard.MouseLeftButtonDown += (s, e) =>
            {
                _selectedCompact = !_selectedCompact;
                UpdateToggleVisual(swCompact, thCompact, _selectedCompact);
                UpdatePreviewLayout();
            };
            sysCardsStack.Children.Add(compactCard);

            // 2. Do Not Disturb Toggle Card
            var dndCard = CreateToggleCard(
                Loc.DoNotDisturb,
                Loc.DoNotDisturbSub,
                _selectedDnd,
                out _dndTitle,
                out _dndSub,
                out _toggleSwitchDnd,
                out _toggleThumbDnd
            );
            var swDnd = _toggleSwitchDnd;
            var thDnd = _toggleThumbDnd;
            dndCard.MouseLeftButtonDown += (s, e) =>
            {
                _selectedDnd = !_selectedDnd;
                UpdateToggleVisual(swDnd, thDnd, _selectedDnd);
            };
            sysCardsStack.Children.Add(dndCard);

            // 3. Startup Toggle Card
            var startupCard = CreateToggleCard(
                Loc.AutoStartTitle,
                Loc.AutoStartSubtitle,
                _selectedStartup,
                out _autoStartTitle,
                out _autoStartSub,
                out _toggleSwitchStartup,
                out _toggleThumbStartup
            );
            var swStartup = _toggleSwitchStartup;
            var thStartup = _toggleThumbStartup;
            startupCard.MouseLeftButtonDown += (s, e) =>
            {
                _selectedStartup = !_selectedStartup;
                UpdateToggleVisual(swStartup, thStartup, _selectedStartup);
            };
            sysCardsStack.Children.Add(startupCard);

            bodyStack.Children.Add(sysCardsStack);

            scrollViewer.Content = bodyStack;
            Grid.SetRow(scrollViewer, 1);
            mainGrid.Children.Add(scrollViewer);

            // ==========================================
            // 3. FOOTER (Actions bar: Test / Save / Close)
            // ==========================================
            var footer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)),
                CornerRadius = new CornerRadius(0, 0, 14, 14),
                Padding = new Thickness(20, 14, 20, 16),
                BorderBrush = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };

            var footerGrid = new Grid();
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Tester
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Spacer
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Fermer
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) }); // Gap
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Enregistrer

            // Button: Tester
            var testBtn = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)), // #313244
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 9, 16, 9),
                Cursor = Cursors.Hand
            };
            _testBtnText = new TextBlock
            {
                Text = Loc.TestBtn,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            testBtn.Child = _testBtnText;
            testBtn.MouseEnter += (s, e) => testBtn.Background = new SolidColorBrush(Color.FromRgb(69, 71, 90));
            testBtn.MouseLeave += (s, e) => testBtn.Background = new SolidColorBrush(Color.FromRgb(49, 50, 68));
            testBtn.MouseLeftButtonDown += (s, e) =>
            {
                ApplyCurrentState();
                OnTestRequested?.Invoke();
            };
            Grid.SetColumn(testBtn, 0);
            footerGrid.Children.Add(testBtn);

            // Button: Fermer (Close)
            var closeBtn = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)), // #313244
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 9, 16, 9),
                Cursor = Cursors.Hand
            };
            _closeBtnText = new TextBlock
            {
                Text = Loc.CloseBtn,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244))
            };
            closeBtn.Child = _closeBtnText;
            closeBtn.MouseEnter += (s, e) => closeBtn.Background = new SolidColorBrush(Color.FromRgb(69, 71, 90));
            closeBtn.MouseLeave += (s, e) => closeBtn.Background = new SolidColorBrush(Color.FromRgb(49, 50, 68));
            closeBtn.MouseLeftButtonDown += (s, e) => Close();
            Grid.SetColumn(closeBtn, 2);
            footerGrid.Children.Add(closeBtn);

            // Button: Enregistrer (Save) — Stays on window with visual confirmation badge!
            _saveBtn = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(137, 180, 250)), // #89b4fa
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 9, 20, 9),
                Cursor = Cursors.Hand,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(137, 180, 250),
                    BlurRadius = 12,
                    ShadowDepth = 1,
                    Opacity = 0.4
                }
            };
            _saveBtnText = new TextBlock
            {
                Text = Loc.SaveBtn,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(17, 17, 27))
            };
            _saveBtn.Child = _saveBtnText;

            _saveBtn.MouseEnter += (s, e) =>
            {
                if (_saveFeedbackTimer == null || !_saveFeedbackTimer.IsEnabled)
                {
                    _saveBtn.Background = new SolidColorBrush(Color.FromRgb(180, 206, 255));
                }
            };
            _saveBtn.MouseLeave += (s, e) =>
            {
                if (_saveFeedbackTimer == null || !_saveFeedbackTimer.IsEnabled)
                {
                    _saveBtn.Background = new SolidColorBrush(Color.FromRgb(137, 180, 250));
                }
            };

            _saveBtn.MouseLeftButtonDown += (s, e) =>
            {
                ApplyCurrentState();
                SettingsManager.Save();

                // Instant feedback without closing the window abruptly!
                _saveBtn.Background = new SolidColorBrush(Color.FromRgb(166, 227, 161)); // #a6e3a1 Green
                _saveBtnText.Text = Loc.SavedBtn;
                _saveBtnText.Foreground = new SolidColorBrush(Color.FromRgb(17, 17, 27));

                _saveFeedbackTimer?.Stop();
                _saveFeedbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1600) };
                _saveFeedbackTimer.Tick += (s2, e2) =>
                {
                    _saveFeedbackTimer.Stop();
                    _saveBtn.Background = new SolidColorBrush(Color.FromRgb(137, 180, 250));
                    _saveBtnText.Text = Loc.SaveBtn;
                };
                _saveFeedbackTimer.Start();
            };

            Grid.SetColumn(_saveBtn, 4);
            footerGrid.Children.Add(_saveBtn);

            footer.Child = footerGrid;
            Grid.SetRow(footer, 2);
            mainGrid.Children.Add(footer);
            rootBorder.Child = mainGrid;
            Content = rootBorder;
        }

        private void UpdatePreviewLayout()
        {
            _previewNotification.Child = null;

            bool isClassic = string.Equals(_selectedTheme, "ClassicDunst", StringComparison.OrdinalIgnoreCase);
            bool isNordic = string.Equals(_selectedTheme, "NordicFrost", StringComparison.OrdinalIgnoreCase);
            bool isAmoled = string.Equals(_selectedTheme, "MidnightAmoled", StringComparison.OrdinalIgnoreCase);

            Brush bgBrush = isAmoled
                ? new SolidColorBrush(Color.FromRgb(0, 0, 0))
                : (isNordic
                    ? new SolidColorBrush(Color.FromRgb(46, 52, 64))
                    : new SolidColorBrush(Color.FromRgb(24, 24, 37)));

            Brush borderBrush = isClassic
                ? new SolidColorBrush(Color.FromRgb(137, 180, 250))
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

            _previewNotification.Background = bgBrush;
            _previewNotification.BorderBrush = borderBrush;
            _previewNotification.BorderThickness = new Thickness(isClassic ? 1.6 : 1.2);
            _previewNotification.CornerRadius = new CornerRadius(_selectedCompact ? 10 : (isClassic ? 12 : 14));
            _previewNotification.Width = Math.Min(_selectedWidth, 440);

            var rootGrid = new Grid();
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Top Glowing Accent Strip
            if (!isClassic)
            {
                var topGradient = isNordic
                    ? new LinearGradientBrush(Color.FromRgb(136, 192, 208), Color.FromRgb(129, 161, 193), new Point(0, 0), new Point(1, 0))
                    : (isAmoled
                        ? new LinearGradientBrush(Color.FromRgb(243, 139, 168), Color.FromRgb(203, 166, 247), new Point(0, 0), new Point(1, 0))
                        : new LinearGradientBrush(Color.FromRgb(137, 180, 250), Color.FromRgb(203, 166, 247), new Point(0, 0), new Point(1, 0)));

                var topAccent = new Border
                {
                    Height = 2.5,
                    CornerRadius = new CornerRadius(_selectedCompact ? 8 : 10, _selectedCompact ? 8 : 10, 0, 0),
                    Background = topGradient
                };
                Grid.SetRow(topAccent, 0);
                rootGrid.Children.Add(topAccent);
            }

            var contentBorder = new Border();
            Grid.SetRow(contentBorder, 1);
            rootGrid.Children.Add(contentBorder);

            if (_selectedCompact)
            {
                // Compact mode preview (slim single-line)
                contentBorder.Padding = new Thickness(10, 7, 14, 7);

                var prevGrid = new Grid();
                prevGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                prevGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var prevCover = new Border
                {
                    Width = 26,
                    Height = 26,
                    CornerRadius = new CornerRadius(5),
                    Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = new TextBlock
                    {
                        Text = "♫",
                        FontSize = 13,
                        Foreground = accentBrush,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Grid.SetColumn(prevCover, 0);
                prevGrid.Children.Add(prevCover);

                var lineStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                lineStack.Children.Add(new TextBlock
                {
                    Text = "Starboy",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = isNordic ? new SolidColorBrush(Color.FromRgb(236, 239, 244)) : (isAmoled ? Brushes.White : new SolidColorBrush(Color.FromRgb(205, 214, 244)))
                });
                lineStack.Children.Add(new TextBlock
                {
                    Text = "  •  ",
                    FontSize = 11.5,
                    FontWeight = FontWeights.Bold,
                    Foreground = accentBrush
                });
                lineStack.Children.Add(new TextBlock
                {
                    Text = "The Weeknd",
                    FontSize = 11.5,
                    Foreground = isNordic ? new SolidColorBrush(Color.FromRgb(216, 222, 233)) : (isAmoled ? new SolidColorBrush(Color.FromRgb(161, 161, 170)) : new SolidColorBrush(Color.FromRgb(186, 194, 222)))
                });

                Grid.SetColumn(lineStack, 1);
                prevGrid.Children.Add(lineStack);
                contentBorder.Child = prevGrid;
            }
            else
            {
                // Standard mode preview
                contentBorder.Padding = new Thickness(12, 10, 14, 10);

                var prevGrid = new Grid();
                prevGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                prevGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var prevCover = new Border
                {
                    Width = isClassic ? 44 : 46,
                    Height = isClassic ? 44 : 46,
                    CornerRadius = new CornerRadius(8),
                    Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                    Margin = new Thickness(0, 0, 12, 0),
                    Child = new TextBlock
                    {
                        Text = "♫",
                        FontSize = 20,
                        Foreground = accentBrush,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Grid.SetColumn(prevCover, 0);
                prevGrid.Children.Add(prevCover);

                var prevTextStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

                if (isClassic)
                {
                    prevTextStack.Children.Add(new TextBlock
                    {
                        Text = "NOW PLAYING",
                        FontSize = 8.5,
                        FontWeight = FontWeights.ExtraBold,
                        Foreground = accentBrush,
                        Margin = new Thickness(0, 0, 0, 2)
                    });
                }

                prevTextStack.Children.Add(new TextBlock
                {
                    Text = "Midnight City",
                    FontSize = isClassic ? 12.5 : 13.5,
                    FontWeight = FontWeights.Bold,
                    Foreground = isNordic ? new SolidColorBrush(Color.FromRgb(236, 239, 244)) : (isAmoled ? Brushes.White : new SolidColorBrush(Color.FromRgb(205, 214, 244))),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(0, 0, 0, 2)
                });
                prevTextStack.Children.Add(new TextBlock
                {
                    Text = "M83 — Hurry Up, We're Dreaming",
                    FontSize = 11.5,
                    Foreground = isNordic ? new SolidColorBrush(Color.FromRgb(216, 222, 233)) : (isAmoled ? new SolidColorBrush(Color.FromRgb(161, 161, 170)) : new SolidColorBrush(Color.FromRgb(186, 194, 222))),
                    TextTrimming = TextTrimming.CharacterEllipsis
                });
                Grid.SetColumn(prevTextStack, 1);
                prevGrid.Children.Add(prevTextStack);

                contentBorder.Child = prevGrid;
            }

            _previewNotification.Child = rootGrid;
        }

        private Border CreateToggleCard(
            string title,
            string subtitle,
            bool initialValue,
            out TextBlock titleBlock,
            out TextBlock subBlock,
            out Border toggleSwitch,
            out Ellipse toggleThumb)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16, 12, 16, 12),
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            subBlock = new TextBlock
            {
                Text = subtitle,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(166, 173, 200)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            textStack.Children.Add(titleBlock);
            textStack.Children.Add(subBlock);
            Grid.SetColumn(textStack, 0);
            grid.Children.Add(textStack);

            toggleSwitch = new Border
            {
                Width = 46,
                Height = 24,
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(initialValue ? Color.FromRgb(166, 227, 161) : Color.FromRgb(69, 71, 90)), // #a6e3a1 Green
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(2.5)
            };

            toggleThumb = new Ellipse
            {
                Width = 19,
                Height = 19,
                Fill = Brushes.White,
                HorizontalAlignment = initialValue ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };
            toggleSwitch.Child = toggleThumb;

            Grid.SetColumn(toggleSwitch, 1);
            grid.Children.Add(toggleSwitch);
            card.Child = grid;
            return card;
        }

        private StackPanel CreateHeaderSection(string title, string subtitle, out TextBlock titleBlock, out TextBlock subBlock)
        {
            var sp = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };
            titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 11,
                FontWeight = FontWeights.ExtraBold,
                Foreground = new SolidColorBrush(Color.FromRgb(137, 180, 250)), // #89b4fa
                Margin = new Thickness(0, 0, 0, 2)
            };
            subBlock = new TextBlock
            {
                Text = subtitle,
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Color.FromRgb(166, 173, 200)) // #a6adc8
            };
            sp.Children.Add(titleBlock);
            sp.Children.Add(subBlock);
            return sp;
        }

        private Border CreateLanguageButton(string langKey, string label)
        {
            var btn = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14, 10, 14, 10),
                Cursor = Cursors.Hand,
                Tag = langKey,
                Child = new TextBlock
                {
                    Text = label,
                    FontSize = 12.5,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244)),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            btn.MouseLeftButtonDown += (s, e) =>
            {
                _selectedLanguage = langKey;
                SettingsManager.Current.Language = langKey;
                HighlightSelectedLanguage();
                UpdateDynamicTexts();
                TrayService.RebuildContextMenu();
            };

            _languageButtons[langKey] = btn;
            return btn;
        }

        private void HighlightSelectedLanguage()
        {
            foreach (var kv in _languageButtons)
            {
                bool isSelected = string.Equals(kv.Key, _selectedLanguage, StringComparison.OrdinalIgnoreCase);
                kv.Value.Background = new SolidColorBrush(isSelected ? Color.FromRgb(137, 180, 250) : Color.FromRgb(24, 24, 37));
                kv.Value.BorderBrush = new SolidColorBrush(isSelected ? Color.FromRgb(137, 180, 250) : Color.FromRgb(49, 50, 68));

                if (kv.Value.Child is TextBlock tb)
                {
                    tb.Foreground = new SolidColorBrush(isSelected ? Color.FromRgb(17, 17, 27) : Color.FromRgb(205, 214, 244));
                    tb.FontWeight = isSelected ? FontWeights.Bold : FontWeights.SemiBold;
                }
            }
        }

        private Border CreateThemeButton(string themeKey, string label)
        {
            var btn = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 9, 12, 9),
                Cursor = Cursors.Hand,
                Tag = themeKey,
                Child = new TextBlock
                {
                    Text = label,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244)),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            btn.MouseLeftButtonDown += (s, e) =>
            {
                _selectedTheme = themeKey;
                HighlightSelectedTheme();
                UpdatePreviewLayout();
            };

            _themeButtons[themeKey] = btn;
            return btn;
        }

        private void HighlightSelectedTheme()
        {
            foreach (var kv in _themeButtons)
            {
                bool isSelected = string.Equals(kv.Key, _selectedTheme, StringComparison.OrdinalIgnoreCase);
                kv.Value.Background = new SolidColorBrush(isSelected ? Color.FromRgb(137, 180, 250) : Color.FromRgb(24, 24, 37));
                kv.Value.BorderBrush = new SolidColorBrush(isSelected ? Color.FromRgb(137, 180, 250) : Color.FromRgb(49, 50, 68));

                if (kv.Value.Child is TextBlock tb)
                {
                    tb.Foreground = new SolidColorBrush(isSelected ? Color.FromRgb(17, 17, 27) : Color.FromRgb(205, 214, 244));
                    tb.FontWeight = isSelected ? FontWeights.Bold : FontWeights.SemiBold;
                }
            }
        }

        private Border CreatePositionButton(string tag, string label)
        {
            var btn = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14, 10, 14, 10),
                Cursor = Cursors.Hand,
                Tag = tag,
                Child = new TextBlock
                {
                    Text = label,
                    FontSize = 12.5,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244)),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            btn.MouseLeftButtonDown += (s, e) =>
            {
                _selectedPosition = tag;
                HighlightSelectedPosition();
            };

            _positionButtons[tag] = btn;
            return btn;
        }

        private void HighlightSelectedPosition()
        {
            foreach (var kv in _positionButtons)
            {
                bool isSelected = kv.Key == _selectedPosition;
                // Active position uses Flamingo/Pink (#f5c2e7) as seen in the mockup
                kv.Value.Background = new SolidColorBrush(isSelected ? Color.FromRgb(245, 194, 231) : Color.FromRgb(24, 24, 37));
                kv.Value.BorderBrush = new SolidColorBrush(isSelected ? Color.FromRgb(245, 194, 231) : Color.FromRgb(49, 50, 68));

                if (kv.Value.Child is TextBlock tb)
                {
                    tb.Foreground = new SolidColorBrush(isSelected ? Color.FromRgb(17, 17, 27) : Color.FromRgb(205, 214, 244));
                    tb.FontWeight = isSelected ? FontWeights.Bold : FontWeights.SemiBold;
                }
            }
        }

        private TextBlock CreatePillBadge(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 12.5,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(137, 180, 250)),
                Padding = new Thickness(8, 2, 8, 2)
            };
        }

        private Border CreateQuickPill(string label, Action onClick)
        {
            var pill = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(0, 0, 8, 0),
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    Text = label,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244))
                }
            };
            pill.MouseEnter += (s, e) => pill.Background = new SolidColorBrush(Color.FromRgb(69, 71, 90));
            pill.MouseLeave += (s, e) => pill.Background = new SolidColorBrush(Color.FromRgb(49, 50, 68));
            pill.MouseLeftButtonDown += (s, e) => onClick();
            return pill;
        }

        private Border CreateSourceButton(string tag, string label)
        {
            var btn = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 9, 12, 9),
                Cursor = Cursors.Hand,
                Tag = tag,
                Child = new TextBlock
                {
                    Text = label,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244)),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            btn.MouseLeftButtonDown += (s, e) =>
            {
                _selectedSource = tag;
                HighlightSelectedSource();
            };

            _sourceButtons[tag] = btn;
            return btn;
        }

        private void HighlightSelectedSource()
        {
            foreach (var kv in _sourceButtons)
            {
                bool isSelected = string.Equals(kv.Key, _selectedSource, StringComparison.OrdinalIgnoreCase);
                kv.Value.Background = new SolidColorBrush(isSelected ? Color.FromRgb(137, 180, 250) : Color.FromRgb(24, 24, 37));
                kv.Value.BorderBrush = new SolidColorBrush(isSelected ? Color.FromRgb(137, 180, 250) : Color.FromRgb(49, 50, 68));

                if (kv.Value.Child is TextBlock tb)
                {
                    tb.Foreground = new SolidColorBrush(isSelected ? Color.FromRgb(17, 17, 27) : Color.FromRgb(205, 214, 244));
                    tb.FontWeight = isSelected ? FontWeights.Bold : FontWeights.SemiBold;
                }
            }
        }

        private void UpdateToggleVisual(Border toggleSwitch, Ellipse toggleThumb, bool isActive)
        {
            toggleSwitch.Background = new SolidColorBrush(isActive ? Color.FromRgb(166, 227, 161) : Color.FromRgb(69, 71, 90)); // #a6e3a1 Green
            toggleThumb.HorizontalAlignment = isActive ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        private void UpdateDynamicTexts()
        {
            _headerSubtitle.Text = Loc.AppSubtitle;
            _langHeaderTitle.Text = Loc.LanguageTitle;
            _langHeaderSub.Text = Loc.LanguageSubtitle;
            _previewHeaderTitle.Text = Loc.LivePreviewTitle;
            _previewHeaderSub.Text = Loc.LivePreviewSubtitle;
            _posHeaderTitle.Text = Loc.PositionTitle;
            _posHeaderSub.Text = Loc.PositionSubtitle;
            _srcHeaderTitle.Text = Loc.SourceTitle;
            _srcHeaderSub.Text = Loc.SourceSubtitle;
            _dimHeaderTitle.Text = Loc.DimensionsTitle;
            _dimHeaderSub.Text = Loc.DimensionsSubtitle;
            _sysHeaderTitle.Text = Loc.SystemTitle;
            _sysHeaderSub.Text = Loc.SystemSubtitle;

            _durTitle.Text = Loc.DurationLabel;
            _widthTitle.Text = Loc.WidthLabel;
            _compactTitle.Text = Loc.CompactMode;
            _compactSub.Text = Loc.CompactModeSub;
            _dndTitle.Text = Loc.DoNotDisturb;
            _dndSub.Text = Loc.DoNotDisturbSub;
            _autoStartTitle.Text = Loc.AutoStartTitle;
            _autoStartSub.Text = Loc.AutoStartSubtitle;

            _testBtnText.Text = Loc.TestBtn;
            _saveBtnText.Text = Loc.SaveBtn;
            _closeBtnText.Text = Loc.CloseBtn;

            // Theme buttons texts
            _themeHeaderTitle.Text = Loc.ThemeTitle;
            _themeHeaderSub.Text = Loc.ThemeSubtitle;
            if (_themeButtons.TryGetValue("AuraNeo", out var bAura) && bAura.Child is TextBlock tbAura) tbAura.Text = Loc.ThemeAuraNeo;
            if (_themeButtons.TryGetValue("ClassicDunst", out var bDunst) && bDunst.Child is TextBlock tbDunst) tbDunst.Text = Loc.ThemeClassicDunst;
            if (_themeButtons.TryGetValue("NordicFrost", out var bNordic) && bNordic.Child is TextBlock tbNordic) tbNordic.Text = Loc.ThemeNordicFrost;
            if (_themeButtons.TryGetValue("MidnightAmoled", out var bAmoled) && bAmoled.Child is TextBlock tbAmoled) tbAmoled.Text = Loc.ThemeMidnightAmoled;

            // Position buttons texts
            if (_positionButtons.TryGetValue("TopLeft", out var bTL) && bTL.Child is TextBlock tbTL) tbTL.Text = Loc.PosTopLeft;
            if (_positionButtons.TryGetValue("TopRight", out var bTR) && bTR.Child is TextBlock tbTR) tbTR.Text = Loc.PosTopRight;
            if (_positionButtons.TryGetValue("BottomLeft", out var bBL) && bBL.Child is TextBlock tbBL) tbBL.Text = Loc.PosBottomLeft;
            if (_positionButtons.TryGetValue("BottomRight", out var bBR) && bBR.Child is TextBlock tbBR) tbBR.Text = Loc.PosBottomRight;
            if (_positionButtons.TryGetValue("TopCenter", out var bTC) && bTC.Child is TextBlock tbTC) tbTC.Text = Loc.PosTopCenter;

            // Source all button text
            if (_sourceButtons.TryGetValue("All", out var bAll) && bAll.Child is TextBlock tbAll) tbAll.Text = Loc.AllSources;
        }

        private void ApplyCurrentState()
        {
            SettingsManager.Current.Position = _selectedPosition;
            SettingsManager.Current.MediaSourceFilter = _selectedSource;
            SettingsManager.Current.Language = _selectedLanguage;
            SettingsManager.Current.Theme = _selectedTheme;
            SettingsManager.Current.DisplayDurationSeconds = _selectedDuration;
            SettingsManager.Current.NotificationWidth = _selectedWidth;
            SettingsManager.Current.StartWithWindows = _selectedStartup;
            SettingsManager.Current.CompactMode = _selectedCompact;
            SettingsManager.Current.DoNotDisturb = _selectedDnd;
            TrayService.UpdateThemeMenuCheckmarks();
            TrayService.UpdateSourceMenuCheckmarks();
            TrayService.UpdateLanguageMenuCheckmarks();
            TrayService.UpdateDndAndCompactChecks();
        }
    }
}
