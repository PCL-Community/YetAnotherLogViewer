using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LogViewer;

// source: o4-mini | https://stackoverflow.com/a/28647821
public class ResourceBinding : MultiBinding
{
    private static readonly ResourceLookupConverter ResourceLookupConverter = new();
    
    public ResourceBinding()
    {
        base.Converter = ResourceLookupConverter;
        base.Mode = BindingMode.OneWay;
        Bindings.Clear();
        Bindings.Add(_binding);
        Bindings.Add(new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
    }

    public ResourceBinding(PropertyPath path) : this() { Path = path; }

    #region Binding Members

    private readonly Binding _binding = new() { Mode = BindingMode.OneWay };

    public object? Source { get => _binding.Source; set => _binding.Source = value; }

    public PropertyPath? Path { get => _binding.Path; set => _binding.Path = value; }

    public string? XPath { get => _binding.XPath; set => _binding.XPath = value; }

    public RelativeSource? RelativeSource { get => _binding.RelativeSource; set => _binding.RelativeSource = value; }

    public string? ElementName { get => _binding.ElementName; set => _binding.ElementName = value; }

    public new IValueConverter? Converter { get => _binding.Converter; set => _binding.Converter = value; }

    public new object? ConverterParameter { get => _binding.ConverterParameter; set => _binding.ConverterParameter = value; }

    [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
    public new CultureInfo? ConverterCulture { get => _binding.ConverterCulture; set => _binding.ConverterCulture = value; }
    
    public new BindingMode Mode { get => _binding.Mode; set => _binding.Mode = value; }
    
    #endregion
}
