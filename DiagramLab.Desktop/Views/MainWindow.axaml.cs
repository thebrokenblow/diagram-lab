using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DiagramLab.Desktop.SymbolsView;
using DiagramLab.Desktop.ViewModel;

namespace DiagramLab.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _mainWindowViewModel = new MainWindowViewModel(); 
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = _mainWindowViewModel;
        
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var width = MenuSymbols.Width;
        var currentPoint = e.GetCurrentPoint(DrawingCanvas);
        _mainWindowViewModel.MovingSymbol(currentPoint.Position.X - width, currentPoint.Position.Y);
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is ActionSymbolView actionSymbolView)
        {
            actionSymbolView.MainWindowViewModel = _mainWindowViewModel;
        }
    }
}