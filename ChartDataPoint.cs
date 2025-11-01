using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents the chart dataPoint. This class is used to set dataPoint for the series items source.
/// </summary>
public class ChartDataPoint : INotifyPropertyChanged
{
    private IComparable xValue;
    private double yValue;
    private double size;
    private double open;
    private double high;
    private double low;
    private double close;
    private double volume;

    /// <summary>
    /// Gets or sets the x-axis value of the data point.
    /// </summary>
    public IComparable XValue
    {
        get => xValue;
        set
        {
            xValue = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the y-axis value of the data point.
    /// </summary>
    public double YValue
    {
        get => yValue;
        set
        {
            yValue = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the size value of the data point for bubble series.
    /// </summary>
    public double Size
    {
        get => size;
        set
        {
            size = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the high value of the data point.
    /// </summary>
    public double High
    {
        get => high;
        set
        {
            high = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the low value of the data point.
    /// </summary>
    public double Low
    {
        get => low;
        set
        {
            low = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the open value of the data point.
    /// </summary>
    public double Open
    {
        get => open;
        set
        {
            open = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the close value of the data point.
    /// </summary>
    public double Close
    {
        get => close;
        set
        {
            close = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the volume value of financial data.
    /// </summary>
    public double Volume
    {
        get => volume;
        set
        {
            volume = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ChartDataPoint()
    {
    }

    public ChartDataPoint(IComparable xValue, double yValue)
    {
        XValue = xValue;
        YValue = yValue;
    }

    public ChartDataPoint(IComparable xValue, double value1, double value2)
    {
        XValue = xValue;
        High = value1;
        YValue = value1;
        Low = value2;
        Size = value2;
    }

    public ChartDataPoint(IComparable xValue, double open, double high, double low, double close)
    {
        XValue = xValue;
        High = high;
        Low = low;
        Open = open;
        Close = close;
    }

    public ChartDataPoint(IComparable xValue, double open, double high, double low, double close, double volume)
    {
        XValue = xValue;
        High = high;
        Low = low;
        Open = open;
        Close = close;
        Volume = volume;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}