using DiagramLab.SymbolsViewModel.Components;
using DiagramLab.SymbolsViewModel.Interfaces;

namespace DiagramLab.SymbolsViewModel;

public class ActionSymbolViewModel : BaseSymbolViewModel, IHasTextFieldViewModel
{
    public TextFieldViewModel TextFieldViewModel { get; } 
 
    private const int DefaultWidth = 140;
    private const int DefaultHeight = 60;
    private const string DefaultBackground = "#FF52C0AA";
    
    public ActionSymbolViewModel()
    {
        Width = DefaultWidth;
        Height = DefaultHeight;
        Background = DefaultBackground;
            
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