using Avalonia.Controls;
using Avalonia.Input;
using DiagramLab.Desktop.ViewModel;

namespace DiagramLab.Desktop.SymbolsView;

public partial class ActionSymbolView : UserControl
{
    public MainWindowViewModel? MainWindowViewModel { get; set; }

    public ActionSymbolView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not InputElement actionSymbolView)
        {
            return;
        }
        
        if (actionSymbolView.DataContext is not SymbolVm symbolVm)
        {
            return; 
        }
        
        MainWindowViewModel?.SetMovingSymbol(symbolVm);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not InputElement actionSymbolView)
        {
            return;
        }
        
        if (actionSymbolView.DataContext is not SymbolVm symbolVm)
        {
            return; 
        }
        
        MainWindowViewModel?.UnsetMovingSymbol(symbolVm);
    }
}