using CommunityToolkit.Mvvm.ComponentModel;

namespace DiagramLab.Desktop.ViewModel;

public partial class SymbolVm : ObservableObject 
{
    [ObservableProperty]
    private double _x;
    
    [ObservableProperty]
    private double _y;

    [ObservableProperty] 
    private double _width;

    [ObservableProperty] 
    private double _height;
}