using System;
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
        set => SetValue(RatingProperty, value);
    }

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<StarRatingControl, bool>(nameof(IsReadOnly), defaultValue: false);

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public event Action<int>? RatingChanged;

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
                Cursor = IsReadOnly ? new Cursor(StandardCursorType.Arrow) : new Cursor(StandardCursorType.Hand)
            };

            var empty = new TextBlock
            {
                Text = "★",
                FontSize = 22,
                Foreground = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var filled = new TextBlock
            {
                Text = "★",
                FontSize = 22,
                Foreground = Brushes.Gold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                IsVisible = Rating >= i * 2 - 1
            };

            if (Rating == i * 2 - 1)
            {
                filled.Clip = new Avalonia.Media.RectangleGeometry(new Rect(0, 0, 12, 24));
            }

            container.Children.Add(empty);
            container.Children.Add(filled);

            if (!IsReadOnly)
            {
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
            }

            _panel.Children.Add(container);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RatingProperty || change.Property == IsReadOnlyProperty)
            UpdateStars();
    }
}