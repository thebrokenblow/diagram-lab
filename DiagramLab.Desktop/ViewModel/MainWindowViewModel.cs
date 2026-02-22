using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace DiagramLab.Desktop.ViewModel;

public class MainWindowViewModel
{
    public List<Item> Items { get; } =
    [
        new Item { X = 100, Y = 200, Size = 100, Color = Colors.Cyan, Text = "First" },
        new Item { X = 500, Y = 300, Size = 200, Color = Colors.Yellow, Text = "Second" },
        new Item { X = 300, Y = 500, Size = 150, Color = Colors.Red, Text = "Third" }
    ];
  
    public int Width => Items.Max(x => x.X + x.Size);
    public int Height => Items.Max(x => x.Y + x.Size);
}