using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DiagramLab.Desktop.SymbolsView;
using DiagramLab.Desktop.ViewModel;

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

    public void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not SymbolView symbolView)
        {
            return;
        }
        
        if (symbolView.DataContext is not SymbolVm symbolVm)
        {
            return; 
        }
        
        var pointerPosition = e.GetPosition(_drawingCanvas);
        _mainWindowViewModel.SetMovingSymbol(symbolVm, pointerPosition.X, pointerPosition.Y);
        
        e.Handled = true;
    }

    public void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not SymbolView symbolView)
        {
            return;
        }
        
        if (symbolView.DataContext is not SymbolVm symbolVm)
        {
            return; 
        }
        
        _mainWindowViewModel.UnsetMovingSymbol(symbolVm);
        
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