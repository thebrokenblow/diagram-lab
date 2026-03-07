using CommunityToolkit.Mvvm.ComponentModel;

namespace DiagramLab.SymbolsViewModel.Components;
 
public partial class TextFieldViewModel : ObservableObject
{
    [ObservableProperty] 
    private double _height;

    [ObservableProperty] 
    private double _width;
    
    [ObservableProperty] 
    private double _offsetX;
    
    [ObservableProperty] 
    private double _offsetY;
    
    [ObservableProperty] 
    private string? _text;
    
    [ObservableProperty] 
    private bool _isEnabled;
}