using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SymbolsView;
using SymbolsViewModel;
using SymbolsViewModel.Menus;

namespace DiagramLab.Desktop.Views;

public partial class MainWindow : Window
{
    private Canvas? _drawingCanvas;
    private readonly MainWindowViewModel _mainWindowViewModel = new();
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = _mainWindowViewModel;
    }

    public void DrawingCanvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var pointerPosition = e.GetPosition(_drawingCanvas);
        _mainWindowViewModel.MovingSymbol(pointerPosition.X, pointerPosition.Y);
        
        e.Handled = true;
    }

    public void SymbolView_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not BaseSymbolView baseSymbolView)
        {
            return;
        }
        
        if (baseSymbolView.DataContext is not BaseSymbolViewModel baseSymbolViewModel)
        {
            return; 
        }
        
        var pointerPosition = e.GetPosition(_drawingCanvas);
        _mainWindowViewModel.SetMovingSymbol(baseSymbolViewModel, pointerPosition.X, pointerPosition.Y);
        
        e.Handled = true;
    }

    public void SymbolView_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not BaseSymbolView baseSymbolView)
        {
            return;
        }
        
        if (baseSymbolView.DataContext is not BaseSymbolViewModel baseSymbolViewModel)
        {
            return; 
        }
        
        _mainWindowViewModel.UnsetMovingSymbol(baseSymbolViewModel);
        
        e.Handled = true;
    }

    private void DrawingCanvas_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is Canvas canvas)
        {
            _drawingCanvas = canvas;
        }
    }
}