using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace LogViewer;

// source: https://stackoverflow.com/a/51442634
public class MultiValueEqualityConverter : IMultiValueConverter
{
    public object Convert(object?[]? values, Type targetType, object parameter, CultureInfo culture)
        => values?.All(o => o?.Equals(values[0]) == true) == true || values?.All(o => o == null) == true;

    public object[] ConvertBack(object? value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class ResourceKeyConverter : IValueConverter
{
    public string Prefix { get; set; } = "";
    public string Suffix { get; set; } = "";
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => $"{Prefix}{value}{Suffix}";
    
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

// source: claude-3-7-sonnet-20250219
[ContentProperty(nameof(Items))]
public class ItemExistenceConverter : IValueConverter
{
    public ObservableCollection<object> Items { get; } = [];
    
    private readonly HashSet<object> _hashSet = [];

    public ItemExistenceConverter()
    {
        Items.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null) foreach (var item in e.NewItems) if (item != null) _hashSet.Add(item);
            if (e.OldItems != null) foreach (var item in e.OldItems) if (item != null) _hashSet.Remove(item);
            
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _hashSet.Clear();
                foreach (var item in Items) if (item != null) _hashSet.Add(item);
            }
        };
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value != null && _hashSet.Contains(value);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

// source: o4-mini
public class ResourceLookupConverter : IMultiValueConverter
{
    public object Convert(object?[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            throw new ArgumentException("Too few arguments");
 
        var key = values[0];
        if (key == null || values[1] is not FrameworkElement element)
            return DependencyProperty.UnsetValue;

        var resource = 
#if DEBUG
            element.FindResource(key);
#else
            element.TryFindResource(key);
#endif
        
        return resource ?? DependencyProperty.UnsetValue;
    }
 
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
