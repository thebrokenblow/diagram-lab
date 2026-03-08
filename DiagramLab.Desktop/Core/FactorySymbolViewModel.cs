using System;
using System.Collections.Generic;
using DiagramLab.SymbolsUi;
using DiagramLab.SymbolsViewModel;

namespace DiagramLab.Desktop.Core;

public class FactorySymbolViewModel
{
    private readonly Dictionary<Type, Func<BaseSymbolViewModel>> _symbolViewModelByType = [];
    
    public FactorySymbolViewModel()
    {
        Configure();
    }

    public BaseSymbolViewModel Create(Type symbolUiType)
    {
        ArgumentNullException.ThrowIfNull(symbolUiType);

        if (!_symbolViewModelByType.TryGetValue(symbolUiType, out var symbolViewModel))
        {
            throw new InvalidOperationException(
                $"Ошибка настройки конфигурации: тип '{symbolUiType.Name}' отсутствует в словаре FactorySymbolViewModel. " +
                $"Убедитесь, что для данного типа UI добавлена соответствующая фабричная функция в метод Configure().");
        }

        return symbolViewModel.Invoke();
    }

    private void Configure()
    {
        _symbolViewModelByType.Add(typeof(ActionSymbolUi), () =>  new ActionSymbolViewModel());
    }
}