using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace MovieRate.Controls;

public class StarRatingControl : UserControl
{
    public static readonly StyledProperty<int> RatingProperty =
        AvaloniaProperty.Register<StarRatingControl, int>(nameof(Rating), defaultValue: 0);

    public int Rating
    {
        get => GetValue(RatingProperty);
        set
        {
            SetValue(RatingProperty, value);
            UpdateStars();
        }
    }

    public event System.Action<int>? RatingChanged;

    private readonly StackPanel _panel;

    public StarRatingControl()
    {
        _panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        Content = _panel;
        UpdateStars();
    }

    private void UpdateStars()
    {
        _panel.Children.Clear();
        for (int i = 1; i <= 5; i++)
        {
            var index = i;

            var container = new Panel
            {
                Width = 24,
                Height = 24,
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            var empty = new TextBlock
            {
                Text = "★",
                FontSize = 22,
                Foreground = Brushes.Gray,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            var filled = new TextBlock
            {
                Text = "★",
                FontSize = 22,
                Foreground = Brushes.Gold,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                IsVisible = Rating >= i * 2 - 1
            };

            if (Rating == i * 2 - 1)
            {
                filled.Clip = new Avalonia.Media.RectangleGeometry(new Rect(0, 0, 12, 24));
            }

            container.Children.Add(empty);
            container.Children.Add(filled);

            container.PointerPressed += (s, e) =>
            {
                var pos = e.GetPosition(container);
                var newRating = pos.X < 12 ? index * 2 - 1 : index * 2;
    
                if (newRating == Rating)
                    Rating = 0;
                else
                    Rating = newRating;
        
                RatingChanged?.Invoke(Rating);
            };

            _panel.Children.Add(container);
        }
    }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RatingProperty)
            UpdateStars();
    }
}