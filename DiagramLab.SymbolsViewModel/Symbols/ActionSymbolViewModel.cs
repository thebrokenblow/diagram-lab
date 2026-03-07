using DiagramLab.SymbolsViewModel.Components;
using DiagramLab.SymbolsViewModel.Interfaces;

namespace DiagramLab.SymbolsViewModel;

public class ActionSymbolViewModel : BaseSymbolViewModel, IHasTextFieldViewModel
{
    public TextFieldViewModel TextFieldViewModel { get; } 
    
    public ActionSymbolViewModel()
    {
        TextFieldViewModel = new TextFieldViewModel
        {
            Width = 140,
            Height = 60,
            Text = "Действие",
            IsEnabled = false,
            OffsetX = 0,
            OffsetY = 0,
        };
    }
}