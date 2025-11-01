using EXOApp.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Syncfusion.Maui.Charts;

namespace EXOApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogPage : ContentPage
    {
        LogViewModel viewModel;
        public LogPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            viewModel = (Models.LogViewModel)BindingContext;
            viewModel.handleChartEvent += fillData;
            ObservableCollection<string> percentages = new ObservableCollection<string>();
            percentages.Add("Raw Values");
            percentages.Add("Percentages");
            Percentages.ItemsSource = percentages;
        }

       private void fillData(List<List<ChartDataPoint>> lineData, List<ChartDataPoint> piData, List<string> SeriesNames)
{
    LineChart.Series.Clear();
    PiChart.Series.Clear();
    
    if (lineData == null || piData == null || SeriesNames == null)
    {
        return;
    }
    
    if (lineData.Count == 0 || piData.Count == 0)
    {
        return;
    }
    
    int counter = 0;
    foreach (List<ChartDataPoint> oc in lineData)
    {
        LineSeries ls = new LineSeries();
        ObservableCollection<ChartDataPoint> points = new ObservableCollection<ChartDataPoint>(oc);
        ls.ItemsSource = points;
        ls.Label = SeriesNames[counter];
        ls.XBindingPath = "XValue";  // Adjust to your ChartDataPoint property
        ls.YBindingPath = "YValue";  // Adjust to your ChartDataPoint property
        LineChart.Series.Add(ls);
        counter++;
    }
    
    PieSeries pie = new PieSeries();
    ObservableCollection<ChartDataPoint> piPoints = new ObservableCollection<ChartDataPoint>(piData);
    pie.ItemsSource = piPoints;
    pie.XBindingPath = "XValue";  // Adjust to your ChartDataPoint property
    pie.YBindingPath = "YValue";  // Adjust to your ChartDataPoint property
    pie.ShowDataLabels = true;
    pie.DataLabelSettings = new CircularDataLabelSettings
    {
        LabelPlacement = DataLabelPlacement.Outer
    };
    
    if(Percentages.SelectedIndex == 0)
    {
        if ((string) Parameters.SelectedItem == "Floating Time")
        {
            pie.DataLabelSettings.LabelStyle = new ChartDataLabelStyle { LabelFormat = "#### Minutes" };
        }
        else
        {
            pie.DataLabelSettings.LabelStyle = new ChartDataLabelStyle { LabelFormat = "### Floats" };
        }
    }
    else
    {
        pie.DataLabelSettings.LabelStyle = new ChartDataLabelStyle { LabelFormat = "#.##'%'" };
    }
    
    pie.ExplodeAll = true;
    PiChart.Series.Add(pie);
}

        protected override bool OnBackButtonPressed()
        {
            viewModel.exitCommand.Execute(null);
            return true;
        }
    }
}