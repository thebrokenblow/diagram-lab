using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SymbolsView;
using SymbolsViewModel;
using SymbolsViewModel.Interfaces;
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
            e.Handled = true;
            return;
        }

        if (e.ClickCount == 2)
        {
            if (baseSymbolView.DataContext is not IHasTextFieldViewModel iHasTextFieldViewModel)
            {
                e.Handled = true;
                return;
            }
            
            _mainWindowViewModel.SetEditableStatus(iHasTextFieldViewModel);
        }
        else
        {
            if (baseSymbolView.DataContext is not BaseSymbolViewModel baseSymbolViewModel)
            {
                e.Handled = true;
                return;
            }
            
            var pointerPosition = e.GetPosition(_drawingCanvas);
            _mainWindowViewModel.SetMovingSymbol(baseSymbolViewModel, pointerPosition.X, pointerPosition.Y);
        }
        
        e.Handled = true;
    }

    public void SymbolView_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not BaseSymbolView { DataContext: BaseSymbolViewModel baseSymbolViewModel })
        {
            e.Handled = true;
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

    private void DrawingCanvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _mainWindowViewModel.UnsetEditableStatus();
    }
}