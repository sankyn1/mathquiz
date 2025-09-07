// File: Behaviors/DigitOnlyBehavior.cs
using Microsoft.Maui.Controls;

namespace MathQuiz.Behaviors;

public class DigitOnlyBehavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry entry)
    {
        entry.TextChanged += OnEntryTextChanged;
        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnEntryTextChanged;
        base.OnDetachingFrom(entry);
    }

    private void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry)
            return;

        var newText = e.NewTextValue;

        if (string.IsNullOrEmpty(newText))
            return;

        // Remove any non-digit characters
        var digitsOnly = new string(newText.Where(char.IsDigit).ToArray());

        if (newText != digitsOnly)
        {
            entry.Text = digitsOnly;
        }
    }
}
