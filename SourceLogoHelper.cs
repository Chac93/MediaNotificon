using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MediaNotif
{
    public static class SourceLogoHelper
    {
        public static string GetSourceDisplayName(string? sourceKey)
        {
            return sourceKey switch
            {
                "MusicAssistant" => "Music Assistant",
                "YouTubeMusic" => "YouTube Music",
                "Spotify" => "Spotify",
                "Zen" => "Zen Browser",
                "Chrome" => "Google Chrome",
                "Firefox" => "Mozilla Firefox",
                "Edge" => "Microsoft Edge",
                "Brave" => "Brave Browser",
                _ => "Media Player"
            };
        }

        public static Color GetSourceAccentColor(string? sourceKey)
        {
            return sourceKey switch
            {
                "MusicAssistant" => Color.FromRgb(24, 188, 242), // #18bcf2 (Music Assistant Cyan)
                "YouTubeMusic" => Color.FromRgb(255, 0, 0),     // #ff0000 (YouTube Red)
                "Spotify" => Color.FromRgb(30, 215, 96),        // #1ed760 (Spotify Green)
                "Zen" => Color.FromRgb(53, 196, 243),            // #35c4f3 (Zen Cyan/Blue)
                "Chrome" => Color.FromRgb(234, 67, 53),          // #ea4335 (Chrome Red)
                "Firefox" => Color.FromRgb(255, 113, 57),        // #ff7139 (Firefox Orange)
                "Edge" => Color.FromRgb(0, 120, 215),           // #0078d7 (Edge Blue)
                "Brave" => Color.FromRgb(255, 85, 0),           // #ff5500 (Brave Orange)
                _ => Color.FromRgb(137, 180, 250)               // #89b4fa (Catppuccin Blue)
            };
        }

        public static FrameworkElement CreateBadge(string? sourceKey, double size, bool withBorder = true)
        {
            var container = new Border
            {
                Width = size,
                Height = size,
                CornerRadius = new CornerRadius(size / 2.0),
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 37)), // Dark base
                BorderBrush = withBorder ? new SolidColorBrush(Color.FromRgb(17, 17, 27)) : Brushes.Transparent,
                BorderThickness = new Thickness(withBorder ? 1.5 : 0),
                ClipToBounds = true
            };

            var logo = CreateVectorLogo(sourceKey, size - (withBorder ? 2.5 : 0));
            container.Child = logo;
            return container;
        }

        public static FrameworkElement CreateVectorLogo(string? sourceKey, double size)
        {
            var vb = new Viewbox
            {
                Width = size,
                Height = size,
                Stretch = Stretch.Uniform
            };

            var canvas = new Canvas
            {
                Width = 24,
                Height = 24
            };

            switch (sourceKey)
            {
                case "MusicAssistant":
                    BuildMusicAssistantLogo(canvas);
                    break;
                case "YouTubeMusic":
                    BuildYouTubeMusicLogo(canvas);
                    break;
                case "Spotify":
                    BuildSpotifyLogo(canvas);
                    break;
                case "Zen":
                    BuildZenLogo(canvas);
                    break;
                case "Chrome":
                    BuildChromeLogo(canvas);
                    break;
                case "Firefox":
                    BuildFirefoxLogo(canvas);
                    break;
                case "Edge":
                    BuildEdgeLogo(canvas);
                    break;
                case "Brave":
                    BuildBraveLogo(canvas);
                    break;
                default:
                    BuildGenericLogo(canvas);
                    break;
            }

            vb.Child = canvas;
            return vb;
        }

        private static void BuildMusicAssistantLogo(Canvas canvas)
        {
            // Music Assistant: Official cyan background with stylized music notes & beam
            var bg = new Ellipse
            {
                Width = 24,
                Height = 24,
                Fill = new SolidColorBrush(Color.FromRgb(24, 188, 242)) // #18bcf2
            };
            canvas.Children.Add(bg);

            // Double musical note with connecting beam
            var noteGeometry = Geometry.Parse("M 7,16 A 2.6,2.2 0 1,1 9.5,13.5 L 9.5,6.5 L 17,4.8 L 17,14.5 A 2.6,2.2 0 1,1 19.5,12.2 L 19.5,3.2 L 7.5,5.5 L 7.5,16 Z");
            var notePath = new Path
            {
                Data = noteGeometry,
                Fill = Brushes.White
            };
            canvas.Children.Add(notePath);
        }

        private static void BuildYouTubeMusicLogo(Canvas canvas)
        {
            // YouTube Music: Red circle, concentric white ring, solid white play triangle
            var bg = new Ellipse
            {
                Width = 24,
                Height = 24,
                Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0)) // #ff0000
            };
            canvas.Children.Add(bg);

            // Inner concentric circle
            var ring = new Ellipse
            {
                Width = 16,
                Height = 16,
                Stroke = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
                StrokeThickness = 1.3
            };
            Canvas.SetLeft(ring, 4);
            Canvas.SetTop(ring, 4);
            canvas.Children.Add(ring);

            // Play Triangle
            var triangle = new Polygon
            {
                Points = new PointCollection { new Point(9.5, 7.5), new Point(16.5, 12), new Point(9.5, 16.5) },
                Fill = Brushes.White
            };
            canvas.Children.Add(triangle);
        }

        private static void BuildSpotifyLogo(Canvas canvas)
        {
            // Spotify: Official green circle with 3 black sound waves
            var bg = new Ellipse
            {
                Width = 24,
                Height = 24,
                Fill = new SolidColorBrush(Color.FromRgb(30, 215, 96)) // #1ed760
            };
            canvas.Children.Add(bg);

            var darkBrush = new SolidColorBrush(Color.FromRgb(18, 18, 18));

            // Sound waves
            var arc1 = new Path
            {
                Data = Geometry.Parse("M 6.2,8.5 Q 12,6.2 17.8,8.8"),
                Stroke = darkBrush,
                StrokeThickness = 2.1,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            var arc2 = new Path
            {
                Data = Geometry.Parse("M 7.2,12 Q 12,10.2 16.8,12.3"),
                Stroke = darkBrush,
                StrokeThickness = 1.8,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            var arc3 = new Path
            {
                Data = Geometry.Parse("M 8.2,15.2 Q 12,13.8 15.8,15.5"),
                Stroke = darkBrush,
                StrokeThickness = 1.5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            canvas.Children.Add(arc1);
            canvas.Children.Add(arc2);
            canvas.Children.Add(arc3);
        }

        private static void BuildZenLogo(Canvas canvas)
        {
            var bg = new Ellipse
            {
                Width = 24,
                Height = 24,
                Fill = new LinearGradientBrush(
                    Color.FromRgb(53, 196, 243),
                    Color.FromRgb(139, 92, 246),
                    new Point(0, 0),
                    new Point(1, 1))
            };
            canvas.Children.Add(bg);

            var spiral = new Path
            {
                Data = Geometry.Parse("M 12,4 A 8,8 0 1,1 5,14 A 6,6 0 1,1 11,8 A 4,4 0 1,1 14,13"),
                Stroke = Brushes.White,
                StrokeThickness = 1.8,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            canvas.Children.Add(spiral);
        }

        private static void BuildChromeLogo(Canvas canvas)
        {
            var bg = new Ellipse
            {
                Width = 24,
                Height = 24,
                Fill = new SolidColorBrush(Color.FromRgb(234, 67, 53)) // #ea4335 Chrome Red
            };
            canvas.Children.Add(bg);

            // Chrome tri-color wedges simulation
            var wedgeYellow = new Path
            {
                Data = Geometry.Parse("M 12,12 L 23.5,8 A 12,12 0 0,1 16.5,23.2 Z"),
                Fill = new SolidColorBrush(Color.FromRgb(251, 188, 5)) // #fbbc05 Yellow
            };
            var wedgeGreen = new Path
            {
                Data = Geometry.Parse("M 12,12 L 16.5,23.2 A 12,12 0 0,1 0.5,12 Z"),
                Fill = new SolidColorBrush(Color.FromRgb(52, 168, 83)) // #34a853 Green
            };
            canvas.Children.Add(wedgeYellow);
            canvas.Children.Add(wedgeGreen);

            var whiteRing = new Ellipse
            {
                Width = 11,
                Height = 11,
                Fill = Brushes.White
            };
            Canvas.SetLeft(whiteRing, 6.5);
            Canvas.SetTop(whiteRing, 6.5);
            canvas.Children.Add(whiteRing);

            var centerBlue = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromRgb(66, 133, 244)) // #4285f4 Blue
            };
            Canvas.SetLeft(centerBlue, 8);
            Canvas.SetTop(centerBlue, 8);
            canvas.Children.Add(centerBlue);
        }

        private static void BuildFirefoxLogo(Canvas canvas)
        {
            var bg = new Ellipse
            {
                Width = 24,
                Height = 24,
                Fill = new LinearGradientBrush(
                    Color.FromRgb(255, 148, 0),
                    Color.FromRgb(255, 54, 84),
                    new Point(0, 0),
                    new Point(1, 1))
            };
            canvas.Children.Add(bg);

            var globe = new Ellipse
            {
                Width = 13,
                Height = 13,
                Fill = new SolidColorBrush(Color.FromRgb(88, 44, 131))
            };
            Canvas.SetLeft(globe, 6);
            Canvas.SetTop(globe, 5.5);
            canvas.Children.Add(globe);

            var flame = new Path
            {
                Data = Geometry.Parse("M 6,19 C 4,14 6,8 12,5 C 10,8 11,11 14,10 C 13,13 15,15 17,14 C 18,17 14,20 11,20 C 8,20 6.5,19.5 6,19 Z"),
                Fill = new SolidColorBrush(Color.FromRgb(255, 186, 0))
            };
            canvas.Children.Add(flame);
        }

        private static void BuildEdgeLogo(Canvas canvas)
        {
            var bg = new Ellipse
            {
                Width = 24,
                Height = 24,
                Fill = new LinearGradientBrush(
                    Color.FromRgb(0, 120, 215),
                    Color.FromRgb(0, 200, 83),
                    new Point(0, 0),
                    new Point(1, 1))
            };
            canvas.Children.Add(bg);

            var wave = new Path
            {
                Data = Geometry.Parse("M 5,12 C 5,6 9,4 14,4 C 11,6 10,9 12,12 C 14,15 18,14 19,17 C 19,20 15,21 11,21 C 7,21 5,17 5,12 Z"),
                Fill = Brushes.White
            };
            canvas.Children.Add(wave);
        }

        private static void BuildBraveLogo(Canvas canvas)
        {
            var bg = new Ellipse
            {
                Width = 24,
                Height = 24,
                Fill = new SolidColorBrush(Color.FromRgb(255, 85, 0)) // #ff5500 Brave Orange
            };
            canvas.Children.Add(bg);

            // Lion head silhouette / shield
            var lion = new Path
            {
                Data = Geometry.Parse("M 12,4 L 18,7 L 17,14 L 12,20 L 7,14 L 6,7 Z"),
                Fill = Brushes.White
            };
            canvas.Children.Add(lion);
        }

        private static void BuildGenericLogo(Canvas canvas)
        {
            var bg = new Ellipse
            {
                Width = 24,
                Height = 24,
                Fill = new SolidColorBrush(Color.FromRgb(137, 180, 250)) // #89b4fa
            };
            canvas.Children.Add(bg);

            var note = new Path
            {
                Data = Geometry.Parse("M 9,16 A 2.5,2 0 1,1 11.5,14 L 11.5,7 L 16.5,5.5 L 16.5,13 A 2.5,2 0 1,1 19,11 L 19,4 L 9,6.5 Z"),
                Fill = new SolidColorBrush(Color.FromRgb(17, 17, 27))
            };
            canvas.Children.Add(note);
        }
    }
}
