using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DiagramLab.SymbolsViewModel.Commands;
using DiagramLab.SymbolsViewModel.Commands.Interfaces;
using DiagramLab.SymbolsViewModel.Interfaces;

namespace DiagramLab.SymbolsViewModel.Menus;

public class MainWindowViewModel
{
    #region Properties

    /// <summary>
    /// Возвращает массив параметров для отображения сетки на холсте.
    /// Формат: [offsetX, offsetY, cellWidth, cellHeight]
    /// </summary>
    public static double[] GridRect => [0, 0, GridCellSize, GridCellSize]; 

    /// <summary>
    /// Коллекция всех символов, отображаемых на холсте.
    /// </summary>
    public ObservableCollection<BaseSymbolViewModel> SymbolsVm { get; } = [];
    
    /// <summary>
    /// Команда для удаления выделенных символов
    /// </summary>
    public ICommand RemoveSymbolsCommand { get; }
    
    /// <summary>
    /// Команда для отмены операции
    /// </summary>
    public ICommand UndoSymbolsCommand { get; }
    
    #endregion

    #region Fields
    
    /// <summary>
    /// Символ, который находится в режиме перемещения (перетаскивания).
    /// </summary>
    private BaseSymbolViewModel? _movingSymbolVm;
    
    /// <summary>
    /// Коллекция всех выделенных символов, отображаемых на холсте.
    /// </summary>
    private ObservableCollection<BaseSymbolViewModel> SelectedSymbolsVm { get; } = [];

    /// <summary>
    /// Коллекция всех символов, у которых есть текстовое поле
    /// </summary>
    private ObservableCollection<IHasTextFieldViewModel> SymbolsHasTextFieldVm { get; } = [];
    
    /// <summary>
    /// Стек команд, который пользователь может отменить
    /// </summary>
    private readonly Stack<ISymbolCommand> _commandHistory = [];

    /// <summary>
    /// Начальная (предыдущая) X координата перетаскиваемого символа 
    /// </summary>
    private double? _previousXCoordinateMovingSymbol;
    
    /// <summary>
    /// Начальная (предыдущая) Y координата перетаскиваемого символа 
    /// </summary>
    private double? _previousYCoordinateMovingSymbol;
    
    #endregion

    #region Constants

    /// <summary>
    /// Размер ячейки сетки (в пикселях)
    /// </summary>
    private const int GridCellSize = 15;
    
    #endregion
    
    public MainWindowViewModel()
    {
        InitializeDefaultSymbols();

        RemoveSymbolsCommand = new RelayCommand(RemoveSymbols);
        UndoSymbolsCommand = new RelayCommand(UndoSymbolsCommandAction);
    }

    private void InitializeDefaultSymbols()
    {
        var actionSymbolViewModel1 = new ActionSymbolViewModel
        {
            X = 100,
            Y = 100,
            Height = 60,
            Width = 140
        };

        var actionSymbolViewModel2 = new ActionSymbolViewModel
        {
            X = 200,
            Y = 200,
            Height = 60,
            Width = 140
        };
        
        var actionSymbolViewModel3 = new ActionSymbolViewModel
        {
            X = 400,
            Y = 400,
            Height = 60,
            Width = 140
        };
        
        SymbolsVm.Add(actionSymbolViewModel1);
        SymbolsVm.Add(actionSymbolViewModel2);
        SymbolsVm.Add(actionSymbolViewModel3);
        
        SymbolsHasTextFieldVm.Add(actionSymbolViewModel1);
        SymbolsHasTextFieldVm.Add(actionSymbolViewModel2);
        SymbolsHasTextFieldVm.Add(actionSymbolViewModel3);
    }
    
    
    /// <summary>
    /// Активирует режим редактирования текста для указанного символа.
    /// Если символ не поддерживает текстовое поле, метод ничего не делает.
    /// Вызывается при двойном клике по символу на холсте
    /// </summary>
    /// <param name="baseSymbolViewModel">Символ, для которого нужно включить редактирование текста</param>
    public void SetEditableStatus(BaseSymbolViewModel baseSymbolViewModel)
    {
        if (baseSymbolViewModel is not IHasTextFieldViewModel iHasTextFieldViewModel)
        {
            return;
        }

        iHasTextFieldViewModel.TextFieldViewModel.IsEnabled = true;
    }
    
    /// <summary>
    /// Снимает выделение со всех символов и деактивирует режим редактирования текста.
    /// Вызывается при клике на свободную область холста.
    /// </summary>
    public void DeselectAllAndDisableEditing()
    {
        foreach (var symbolHasTextFieldVm in SymbolsHasTextFieldVm)
        {
            symbolHasTextFieldVm.TextFieldViewModel.IsEnabled = false;
        }
        
        foreach (var symbolVm in SymbolsVm)
        {
            symbolVm.IsSelect = false;
        }
        
        SelectedSymbolsVm.Clear(); 
    }

    #region Moving Symbol Events
    
    /// <summary>
    /// Устанавливает символ как перемещаемый и вычисляет смещение точки захвата.
    /// Смещение необходимо для плавного перетаскивания без "скачков" символа
    /// </summary>
    /// <param name="symbolVm">Символ, который будет перемещаться</param>
    /// <param name="pointerX">X-координата курсора в момент захвата (относительно холста)</param>
    /// <param name="pointerY">Y-координата курсора в момент захвата (относительно холста)</param>
    public void SetMovingSymbol(BaseSymbolViewModel symbolVm, double pointerX, double pointerY)
    {
        _movingSymbolVm = symbolVm;
        
        _previousXCoordinateMovingSymbol = _movingSymbolVm.X;
        _previousYCoordinateMovingSymbol = _movingSymbolVm.Y;
        
        _movingSymbolVm.IsSelect = true;
        
        SelectedSymbolsVm.Add(symbolVm);
        
        // Вычисляем смещение от точки захвата до левого верхнего угла символа
        _movingSymbolVm.OffsetX = pointerX - _movingSymbolVm.X;
        _movingSymbolVm.OffsetY = pointerY - _movingSymbolVm.Y;
    }
    
    /// <summary>
    /// Обновляет позицию перемещаемого символа в соответствии с движением курсора.
    /// Координаты автоматически выравниваются по сетке (привязка к GridCellSize)
    /// </summary>
    /// <param name="cursorX">Текущая X-координата курсора на холсте</param>
    /// <param name="cursorY">Текущая Y-координата курсора на холсте</param>
    public void UpdateMovingSymbolPosition(double cursorX, double cursorY)
    {
        if (_movingSymbolVm == null)
        {
            return;
        }
        
        // Вычисляем желаемую позицию символа с учётом смещения от точки захвата
        var desiredX = cursorX - _movingSymbolVm.OffsetX;
        var desiredY = cursorY - _movingSymbolVm.OffsetY;
        
        // Применяем привязку к сетке для аккуратного выравнивания
        _movingSymbolVm.X = desiredX - desiredX % GridCellSize;
        _movingSymbolVm.Y = desiredY - desiredY % GridCellSize;
    }
    
    /// <summary>
    /// Завершает режим перемещения символа.
    /// </summary>
    public void StopMovingSymbol(double cursorX, double cursorY)
    {
        if (_movingSymbolVm == null || 
            !_previousXCoordinateMovingSymbol.HasValue || 
            !_previousYCoordinateMovingSymbol.HasValue)
        {
            return;
        }
        
        var desiredX = cursorX - _movingSymbolVm.OffsetX;
        var desiredY = cursorY - _movingSymbolVm.OffsetY;
        
        // Применяем привязку к сетке для аккуратного выравнивания
        var x = desiredX - desiredX % GridCellSize;
        var y = desiredY - desiredY % GridCellSize;
        
        var changeCoordinateCommand = new ChangeCoordinateCommand(
            _movingSymbolVm, 
            _previousXCoordinateMovingSymbol.Value, 
            _previousYCoordinateMovingSymbol.Value, 
            x, 
            y);
        
        changeCoordinateCommand.Execute();
        
        _commandHistory.Push(changeCoordinateCommand);
        
        _movingSymbolVm = null;
        _previousXCoordinateMovingSymbol = null;
        _previousYCoordinateMovingSymbol = null;
    }

    #endregion

    #region Add Remove Events
    
    /// <summary>
    /// Добавляет новый символ на холст и сразу активирует режим его перемещения.
    /// </summary>
    /// <param name="symbolViewModel">Созданный символ для добавления на холст</param>
    public void AddSymbol(BaseSymbolViewModel symbolViewModel)
    {
        //Проверка нужна для блокировки создание нового символа,
        //пока другой символ находится в процессе перемещения.
        if (_movingSymbolVm != null)
        {
            return;
        }
        
        var addSymbolCommand = new AddSymbolCommand(symbolViewModel, SymbolsVm);
        addSymbolCommand.Execute();
        
        _commandHistory.Push(addSymbolCommand);
        
        // Временное размещение за пределами видимой области,
        // чтобы символ не "моргал" в углу перед первым перемещением
        symbolViewModel.X = -symbolViewModel.Width - BaseSymbolViewModel.DefaultBorderThickness;
        symbolViewModel.Y = -symbolViewModel.Height - BaseSymbolViewModel.DefaultBorderThickness;
        
        _previousXCoordinateMovingSymbol = symbolViewModel.X;
        _previousYCoordinateMovingSymbol = symbolViewModel.Y;
        
        symbolViewModel.IsSelect = true;
        _movingSymbolVm = symbolViewModel;
        
        SelectedSymbolsVm.Add(symbolViewModel);
    }
    
    /// <summary>
    /// Удаление символа с холста
    /// </summary>
    private void RemoveSymbols()
    {
        var removeSymbolCommand = new RemoveSymbolCommand(SymbolsVm, SelectedSymbolsVm); 
        removeSymbolCommand.Execute();
        
        _commandHistory.Push(removeSymbolCommand);
    }
    
    #endregion

    /// <summary>
    /// Отмена последней команды
    /// </summary>
    private void UndoSymbolsCommandAction()
    {
        DeselectAllAndDisableEditing();
        
        if (_commandHistory.Count == 0)
        {
            return;
        }
        
        var command = _commandHistory.Pop();
        command.Undo();
    }
}