using System.Collections.ObjectModel;

namespace DiagramLab.Desktop.ViewModel;

public class MainWindowViewModel
{
    public ObservableCollection<SymbolVm> SymbolsVm { get; } = [];

    private SymbolVm? _movingSymbolVm;
    
    public MainWindowViewModel()
    {
        InitializeDefaultSymbols();
    }
    
    private void InitializeDefaultSymbols()
    {
        SymbolsVm.Add(new SymbolVm
        {
            X = 100,
            Y = 100,
            Height = 60,
            Width = 140
        });
        
        SymbolsVm.Add(new SymbolVm
        {
            X = 200,
            Y = 200,
            Height = 60,
            Width = 140
        });
    }

    public void SetMovingSymbol(SymbolVm symbolVm, double pointerX, double pointerY)
    {
        _movingSymbolVm = symbolVm;
        
        _movingSymbolVm.OffsetX = pointerX - _movingSymbolVm.X;
        _movingSymbolVm.OffsetY = pointerY - _movingSymbolVm.Y;
    }
    
    public void MovingSymbol(double x, double y)
    {
        if (_movingSymbolVm == null)
        {
            return; 
        }
        
        _movingSymbolVm.X = x - _movingSymbolVm.OffsetX;
        _movingSymbolVm.Y = y - _movingSymbolVm.OffsetY;
    }

    public void UnsetMovingSymbol(SymbolVm symbolVm)
    {
        if (_movingSymbolVm != symbolVm)
        {
            return;
        }
        
        _movingSymbolVm = null;
    }
}