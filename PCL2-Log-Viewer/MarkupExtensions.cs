using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace LogViewer;

// source: o4-mini | https://stackoverflow.com/a/28647821
public class ResourceBinding : MultiBinding
{
    private static readonly ResourceLookupConverter ResourceLookupConverter = new();
    
    public ResourceBinding()
    {
        base.Converter = ResourceLookupConverter;
        Bindings.Clear();
        Bindings.Add(_binding);
        Bindings.Add(new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
    }

    public ResourceBinding(PropertyPath path) : this()
    {
        Path = path;
    }

    #region Binding Members

    private readonly Binding _binding = new();

    /// <summary> The source path (for CLR bindings).</summary>
    public object? Source { get => _binding.Source; set => _binding.Source = value; }

    /// <summary> The source path (for CLR bindings).</summary>
    public PropertyPath? Path { get => _binding.Path; set => _binding.Path = value; }

    /// <summary> The XPath path (for XML bindings).</summary>
    public string? XPath { get => _binding.XPath; set => _binding.XPath = value; }

    /// <summary> The Converter to apply </summary>
    public new IValueConverter? Converter { get => _binding.Converter; set => _binding.Converter = value; }

    /// <summary>
    /// The parameter to pass to converter.
    /// </summary>
    /// <value></value>
    public new object? ConverterParameter { get => _binding.ConverterParameter; set => _binding.ConverterParameter = value; }

    /// <summary> Culture in which to evaluate the converter </summary>
    [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
    public new CultureInfo? ConverterCulture { get => _binding.ConverterCulture; set => _binding.ConverterCulture = value; }

    /// <summary>
    /// Description of the object to use as the source, relative to the target element.
    /// </summary>
    public RelativeSource? RelativeSource { get => _binding.RelativeSource; set => _binding.RelativeSource = value; }

    /// <summary> Name of the element to use as the source </summary>
    public string? ElementName { get => _binding.ElementName; set => _binding.ElementName = value; }
    
    #endregion
}
