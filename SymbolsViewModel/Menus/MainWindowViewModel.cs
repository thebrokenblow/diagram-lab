using System.Collections.ObjectModel;

namespace SymbolsViewModel.Menus;

public class MainWindowViewModel
{
    private BaseSymbolViewModel? _movingSymbolVm;

    public MainWindowViewModel()
    {
        InitializeDefaultSymbols();
    }

    public ObservableCollection<BaseSymbolViewModel> SymbolsVm { get; } = [];

    private void InitializeDefaultSymbols()
    {
        SymbolsVm.Add(new ActionSymbolViewModel
        {
            X = 100,
            Y = 100,
            Height = 60,
            Width = 140
        });

        SymbolsVm.Add(new ActionSymbolViewModel
        {
            X = 200,
            Y = 200,
            Height = 60,
            Width = 140
        });
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

        _movingSymbolVm.X = x - _movingSymbolVm.OffsetX;
        _movingSymbolVm.Y = y - _movingSymbolVm.OffsetY;
    }

    public void UnsetMovingSymbol(BaseSymbolViewModel symbolVm)
    {
        if (_movingSymbolVm != symbolVm) return;

        _movingSymbolVm = null;
    }
}