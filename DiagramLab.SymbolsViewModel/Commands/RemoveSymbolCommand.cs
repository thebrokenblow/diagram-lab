using System.Collections.ObjectModel;
using DiagramLab.SymbolsViewModel.Commands.Interfaces;

namespace DiagramLab.SymbolsViewModel.Commands;

public class RemoveSymbolCommand(
    ObservableCollection<BaseSymbolViewModel> symbols,
    ObservableCollection<BaseSymbolViewModel> selectedSymbols)
    : ISymbolCommand
{
    private readonly List<BaseSymbolViewModel> _copySelectedSymbols = [..selectedSymbols];

    public void Execute()
    {
        foreach (var selectedSymbol in selectedSymbols)
        {
            symbols?.Remove(selectedSymbol);   
        }
        
        selectedSymbols.Clear();
    }

    public void Undo()
    {
        foreach (var selectedSymbol in _copySelectedSymbols)
        {
            selectedSymbols.Add(selectedSymbol);
        }
        
        foreach (var selectedSymbol in selectedSymbols)
        {
            symbols?.Add(selectedSymbol);
        }
    }
}