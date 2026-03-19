using System;
using System.Collections.Generic;
using DiagramLab.SymbolsUi.ToolboxSymbols;
using DiagramLab.SymbolsViewModel;

namespace DiagramLab.Desktop.Core;

public class FactorySymbolViewModel
{
    private readonly Dictionary<string, Func<BaseSymbolViewModel>> _symbolViewModelByNameTypeToolboxSymbol = [];
    
    public FactorySymbolViewModel()
    {
        Configure();
    }

    public BaseSymbolViewModel Create(string nameTypeToolboxSymbol)
    {
        ArgumentNullException.ThrowIfNull(nameTypeToolboxSymbol);

        if (!_symbolViewModelByNameTypeToolboxSymbol.TryGetValue(nameTypeToolboxSymbol, out var symbolViewModel))
        {
            throw new InvalidOperationException(
                $"Тип символа '{nameTypeToolboxSymbol}' не зарегистрирован в FactorySymbolViewModel. Добавьте его в метод Configure()");
        }

        return symbolViewModel.Invoke();
    }

    private void Configure()
    {
        _symbolViewModelByNameTypeToolboxSymbol.Add(
            typeof(ActionSymbolToolbox).ToString(), 
            () => new ActionSymbolViewModel());
    }
}