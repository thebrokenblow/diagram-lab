using System.Collections.ObjectModel;
using DiagramLab.SymbolsViewModel.Commands.Interfaces;

namespace DiagramLab.SymbolsViewModel.Commands;

public class AddSymbolCommand(
    BaseSymbolViewModel symbol, 
    ObservableCollection<BaseSymbolViewModel> symbols) : ISymbolCommand
{
    public void Execute()
    {
        symbols.Add(symbol);
    }

    public void Undo()
    {
        symbols.Remove(symbol);
    }
}