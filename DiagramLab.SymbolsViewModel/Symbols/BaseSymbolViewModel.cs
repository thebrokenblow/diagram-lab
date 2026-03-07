using CommunityToolkit.Mvvm.ComponentModel;

namespace DiagramLab.SymbolsViewModel;

public partial class BaseSymbolViewModel : ObservableObject
{
    [ObservableProperty] 
    private double _height;

    [ObservableProperty] 
    private double _width;

    [ObservableProperty] 
    private double _x;

    [ObservableProperty] 
    private double _y;

    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
}