using System.Collections.ObjectModel;
using DiagramLab.SymbolsViewModel.Interfaces;

namespace DiagramLab.SymbolsViewModel.Menus;

public class MainWindowViewModel
{
    private BaseSymbolViewModel? _movingSymbolVm;
    private const int GridCellSize = 15;
    
    public static double[] GridRect => [0, 0, GridCellSize, GridCellSize];
    public ObservableCollection<BaseSymbolViewModel> SymbolsVm { get; } = [];
    private ObservableCollection<IHasTextFieldViewModel> SymbolsHasTextFieldVm { get; } = [];
    
    public MainWindowViewModel()
    {
        InitializeDefaultSymbols();
    }
    

    private void InitializeDefaultSymbols()
    {
        var actionSymbolViewModel1 = new ActionSymbolViewModel
        {
            X = 100,
            Y = 100,
            Height = 60,
            Width = 140
        };

        var actionSymbolViewModel2 = new ActionSymbolViewModel
        {
            X = 200,
            Y = 200,
            Height = 60,
            Width = 140
        };
        
        SymbolsVm.Add(actionSymbolViewModel1);
        SymbolsVm.Add(actionSymbolViewModel2);
        
        SymbolsHasTextFieldVm.Add(actionSymbolViewModel1);
        SymbolsHasTextFieldVm.Add(actionSymbolViewModel2);
    }

    public void SetMovingSymbol(BaseSymbolViewModel symbolVm, double pointerX, double pointerY)
    {
        _movingSymbolVm = symbolVm;

        _movingSymbolVm.OffsetX = pointerX - _movingSymbolVm.X;
        _movingSymbolVm.OffsetY = pointerY - _movingSymbolVm.Y;
    }

    public void MovingSymbol(double x, double y)
    {
        if (_movingSymbolVm == null) return;

        _movingSymbolVm.X = x - _movingSymbolVm.OffsetX - (x - _movingSymbolVm.OffsetX) % GridCellSize;
        _movingSymbolVm.Y = y - _movingSymbolVm.OffsetY - (y - _movingSymbolVm.OffsetY) % GridCellSize;
    }

    public void UnsetMovingSymbol(BaseSymbolViewModel symbolVm)
    {
        if (_movingSymbolVm != symbolVm) return;

        _movingSymbolVm = null;
    }

    public void SetEditableStatus(IHasTextFieldViewModel iHasTextFieldViewModel)
    {
        iHasTextFieldViewModel.TextFieldViewModel.IsEnabled = true;
    }
    
    public void UnsetEditableStatus()
    {
        foreach (var symbolHasTextFieldVm in SymbolsHasTextFieldVm)
        {
            symbolHasTextFieldVm.TextFieldViewModel.IsEnabled = false;
        }
    }
}