using DiagramLab.SymbolsViewModel.Commands.Interfaces;

namespace DiagramLab.SymbolsViewModel.Commands;

public class ChangeCoordinateCommand(
    BaseSymbolViewModel baseSymbolViewModel,
    double previousX,
    double previousY,
    double currentX,
    double currentY) : ISymbolCommand
{
    public void Execute()
    {
        baseSymbolViewModel.X = currentX;
        baseSymbolViewModel.Y = currentY;
    }

    public void Undo()
    {
        baseSymbolViewModel.X = previousX;
        baseSymbolViewModel.Y = previousY;
    }
}