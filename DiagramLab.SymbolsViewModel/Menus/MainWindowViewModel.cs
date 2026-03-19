using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DiagramLab.SymbolsViewModel.Commands;
using DiagramLab.SymbolsViewModel.Commands.Interfaces;
using DiagramLab.SymbolsViewModel.Interfaces;

namespace DiagramLab.SymbolsViewModel.Menus;

public class MainWindowViewModel
{
    #region Constants

    /// <summary>
    /// Размер ячейки сетки (в пикселях). Используется для привязки элементов к сетке.
    /// </summary>
    private const int GridCellSize = 15;
    
    #endregion

    #region Properties

    /// <summary>
    /// Возвращает массив параметров для отображения сетки на холсте.
    /// </summary>
    public static double[] GridRect => [0, 0, GridCellSize, GridCellSize]; 
    
    /// <summary>
    /// Команда для удаления выделенных символов.
    /// </summary>
    public ICommand RemoveSymbolsCommand { get; }
    
    /// <summary>
    /// Команда для отмены последнего действия.
    /// </summary>
    public ICommand UndoSymbolsCommand { get; }
    
    #endregion

    #region Collections

    /// <summary>
    /// Коллекция всех символов, отображаемых на холсте.
    /// </summary>
    public ObservableCollection<BaseSymbolViewModel> SymbolsVm { get; } = [];
    
    /// <summary>
    /// Коллекция выделенных символов на холсте.
    /// </summary>
    private ObservableCollection<BaseSymbolViewModel> SelectedSymbolsVm { get; } = [];

    /// <summary>
    /// Коллекция символов, поддерживающих редактирование текстового поля.
    /// </summary>
    private ObservableCollection<IHasTextFieldViewModel> SymbolsHasTextFieldVm { get; } = [];
    
    #endregion

    #region Fields

    /// <summary>
    /// Символ, который находится в режиме перемещения (перетаскивания).
    /// </summary>
    private BaseSymbolViewModel? _movingSymbolVm;
    
    /// <summary>
    /// Стек команд для реализации функционала Undo/Redo.
    /// </summary>
    private readonly Stack<ISymbolCommand> _commandHistory = [];

    /// <summary>
    /// Начальная X-координата перемещаемого символа (до начала перетаскивания).
    /// </summary>
    private double? _previousXCoordinateMovingSymbol;
    
    /// <summary>
    /// Начальная Y-координата перемещаемого символа (до начала перетаскивания).
    /// </summary>
    private double? _previousYCoordinateMovingSymbol;
    
    #endregion
    
    public MainWindowViewModel()
    {
        InitializeDefaultSymbols();

        RemoveSymbolsCommand = new RelayCommand(RemoveSymbols);
        UndoSymbolsCommand = new RelayCommand(UndoSymbolsCommandAction);
    }

    #region Initialization

    /// <summary>
    /// Инициализирует набор демонстрационных символов при запуске приложения.
    /// </summary>
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
    
    #endregion

    #region Symbol Selection

    /// <summary>
    /// Активирует режим редактирования текста для указанного символа.
    /// </summary>
    /// <param name="baseSymbolViewModel">Символ, для которого нужно включить редактирование текста.</param>
    /// <remarks>
    /// Вызывается при двойном клике по символу на холсте.
    /// Если символ не поддерживает текстовое поле, метод ничего не делает.
    /// </remarks>
    public static void SetEditableStatus(BaseSymbolViewModel baseSymbolViewModel)
    {
        if (baseSymbolViewModel is not IHasTextFieldViewModel iHasTextFieldViewModel)
        {
            return;
        }

        iHasTextFieldViewModel.TextFieldViewModel.IsEnabled = true;
    }
    
    /// <summary>
    /// Снимает выделение со всех символов и деактивирует режим редактирования текста.
    /// </summary>
    /// <remarks>
    /// Вызывается при клике на свободную область холста.
    /// </remarks>
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
    
    #endregion

    #region Moving Symbol Events Handlers
    
    /// <summary>
    /// Устанавливает символ как перемещаемый и вычисляет смещение точки захвата.
    /// </summary>
    /// <param name="symbolVm">Символ, который будет перемещаться.</param>
    /// <param name="xCoordinate">X-координата курсора в момент захвата (относительно холста).</param>
    /// <param name="yCoordinate">Y-координата курсора в момент захвата (относительно холста).</param>
    /// <remarks>
    /// Смещение (Offset) необходимо для плавного перетаскивания без "скачков" символа.
    /// </remarks>
    public void SetMovingSymbol(BaseSymbolViewModel symbolVm, double xCoordinate, double yCoordinate)
    {
        _movingSymbolVm = symbolVm;
        
        _previousXCoordinateMovingSymbol = _movingSymbolVm.X;
        _previousYCoordinateMovingSymbol = _movingSymbolVm.Y;
        
        _movingSymbolVm.IsSelect = true;
        
        SelectedSymbolsVm.Add(symbolVm);
        
        // Вычисляем смещение от точки захвата до левого верхнего угла символа
        _movingSymbolVm.OffsetX = xCoordinate - _movingSymbolVm.X;
        _movingSymbolVm.OffsetY = yCoordinate - _movingSymbolVm.Y;
    }
    
    /// <summary>
    /// Обновляет позицию перемещаемого символа в соответствии с движением курсора.
    /// </summary>
    /// <param name="xCoordinate">Текущая X-координата курсора на холсте.</param>
    /// <param name="yCoordinate">Текущая Y-координата курсора на холсте.</param>
    /// <remarks>
    /// Координаты автоматически выравниваются по сетке (привязка к GridCellSize).
    /// </remarks>
    public void UpdateMovingSymbolPosition(double xCoordinate, double yCoordinate)
    {
        if (_movingSymbolVm == null)
        {
            return;
        }
        
        // Вычисляем желаемую позицию символа с учётом смещения от точки захвата
        var desiredX = xCoordinate - _movingSymbolVm.OffsetX;
        var desiredY = yCoordinate - _movingSymbolVm.OffsetY;
        
        // Применяем привязку к сетке для аккуратного выравнивания
        _movingSymbolVm.X = desiredX - desiredX % GridCellSize;
        _movingSymbolVm.Y = desiredY - desiredY % GridCellSize;
    }
    
    /// <summary>
    /// Завершает режим перемещения символа и создаёт команду для истории изменений.
    /// </summary>
    /// <param name="xCoordinate">Финальная X-координата курсора на холсте.</param>
    /// <param name="yCoordinate">Финальная Y-координата курсора на холсте.</param>
    public void StopMovingSymbol(double xCoordinate, double yCoordinate)
    {
        if (_movingSymbolVm == null || 
            !_previousXCoordinateMovingSymbol.HasValue || 
            !_previousYCoordinateMovingSymbol.HasValue)
        {
            return;
        }
        
        var desiredX = xCoordinate - _movingSymbolVm.OffsetX;
        var desiredY = yCoordinate - _movingSymbolVm.OffsetY;
        
        // Применяем привязку к сетке для аккуратного выравнивания
        var currentXCoordinateMovingSymbol = desiredX - desiredX % GridCellSize;
        var currentYCoordinateMovingSymbol = desiredY - desiredY % GridCellSize;
        
        var changeCoordinateCommand = new ChangeCoordinateCommand(
            _movingSymbolVm, 
            _previousXCoordinateMovingSymbol.Value, 
            _previousYCoordinateMovingSymbol.Value, 
            currentXCoordinateMovingSymbol, 
            currentYCoordinateMovingSymbol);
        
        changeCoordinateCommand.Execute();
        
        _commandHistory.Push(changeCoordinateCommand);
        
        _movingSymbolVm = null;
        _previousXCoordinateMovingSymbol = null;
        _previousYCoordinateMovingSymbol = null;
    }

    #endregion

    #region Add Remove Symbols Events Handlers

    /// <summary>
    /// Добавляет новый символ на холст и сразу активирует режим его перемещения.
    /// </summary>
    /// <param name="symbolViewModel">Созданный символ для добавления на холст.</param>
    /// <param name="xCoordinate">Текущая X-координата курсора на холсте.</param>
    /// <param name="yCoordinate">Текущая Y-координата курсора на холсте.</param>
    public void AddSymbol(BaseSymbolViewModel symbolViewModel, double xCoordinate, double yCoordinate)
    {
        var addSymbolCommand = new AddSymbolCommand(symbolViewModel, SymbolsVm);
        addSymbolCommand.Execute();
        
        _commandHistory.Push(addSymbolCommand);
        
        symbolViewModel.X = xCoordinate - xCoordinate % GridCellSize;
        symbolViewModel.Y = yCoordinate - yCoordinate % GridCellSize;
        
        _previousXCoordinateMovingSymbol = xCoordinate;
        _previousYCoordinateMovingSymbol = yCoordinate;
        
        SelectedSymbolsVm.Add(symbolViewModel);
    }
    
    /// <summary>
    /// Удаляет выделенные символы с холста.
    /// </summary>
    private void RemoveSymbols()
    {
        var removeSymbolCommand = new RemoveSymbolCommand(SymbolsVm, SelectedSymbolsVm); 
        removeSymbolCommand.Execute();
        
        _commandHistory.Push(removeSymbolCommand);
    }
    
    #endregion

    #region Undo Operations

    /// <summary>
    /// Отменяет последнее выполненное действие.
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
    
    #endregion
}