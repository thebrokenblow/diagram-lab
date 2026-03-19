using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using DiagramLab.Desktop.Core;
using DiagramLab.Desktop.DTOs;
using DiagramLab.SymbolsUi.DragGhostSymbols;
using DiagramLab.SymbolsUi.ToolboxSymbols;
using DiagramLab.SymbolsView;
using DiagramLab.SymbolsViewModel;
using DiagramLab.SymbolsViewModel.Menus;

namespace DiagramLab.Desktop.Views;

public partial class MainWindow : Window
{
    #region Constants

    /// <summary>
    /// Количество нажатий для активации режима перемещения символа, который уже добавлен на холст
    /// </summary>
    private const int SingleClick = 1;

    /// <summary>
    /// Количество нажатий для перевода символа в режим редактирования текста.
    /// </summary>
    private const int DoubleClick = 2;

    /// <summary>
    /// Уникальный идентификатор формата данных для операции Drag-and-Drop.
    /// Используется для идентификации перетаскиваемых символов.
    /// </summary>
    private const string IdentifierDragObject = "DiagramLab.SymbolGhost";

    #endregion

    #region Fields

    /// <summary>
    /// Холст (Canvas), на котором отображаются и редактируются символы.
    /// </summary>
    private Canvas? _drawingCanvas;

    /// <summary>
    /// ViewModel главного окна, содержащая основную логику по обработке символов.
    /// </summary>
    private readonly MainWindowViewModel _mainWindowViewModel = new();

    /// <summary>
    /// Фабрика для создания ViewModel символов на основе их UI-представлений.
    /// </summary>
    private readonly FactorySymbolViewModel _factorySymbolViewModel = new();

    /// <summary>
    /// Текущий перетаскиваемый символ-призрак.
    /// Отображается во время операции Drag-and-Drop из панели символов.
    /// </summary>
    private BaseSymbolGhost? _draggingGhostSymbol;

    /// <summary>
    /// Словарь, сопоставляющий символы из панели инструментов с их символами-призраками.
    /// </summary>
    private readonly Dictionary<BaseSymbolToolbox, BaseSymbolGhost> _ghostSymbolBySymbolToolbox = [];

    #endregion
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = _mainWindowViewModel;
        
        ConfigureGhostSymbolMapping();
    }
    
    #region Drag and Drop Events Handlers

    /// <summary>
    /// Обрабатывает нажатие на символ в панели инструментов.
    /// Инициирует операцию Drag-and-Drop для создания нового символа.
    /// </summary>
    /// <param name="sender">Исходный символ панели инструментов</param>
    /// <param name="e">Данные события PointerPressed</param>
    private async void SymbolUi_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            _mainWindowViewModel.DeselectAllAndDisableEditing();

            if (sender is not BaseSymbolToolbox toolboxSymbol)
            {
                return;
            }

            _draggingGhostSymbol = _ghostSymbolBySymbolToolbox[toolboxSymbol];

            var symbolInfoDto = new SymbolInfoDto
            {
                Name = toolboxSymbol.GetType().ToString()
            };
            
            var serializedData = SerializeSymbolInfo(symbolInfoDto);
            var dragData = CreateDragDropData(serializedData);

            var startPosition = e.GetPosition(this);
            UpdateGhostSymbolPosition(_draggingGhostSymbol, startPosition);

            _draggingGhostSymbol.IsVisible = true;
            await DragDrop.DoDragDropAsync(e, dragData, DragDropEffects.Move);
            _draggingGhostSymbol.IsVisible = false;
        }
        catch (Exception ex)
        {
            // В продакшене буду использовать нормальное логирование
            Console.WriteLine($"Ошибка при перетаскивании символа: {ex.Message}");
        }
        finally
        {
            e.Handled = true; //Прерывает всплытие события для родительского элемента
        }
    }

    /// <summary>
    /// Обрабатывает событие DragOver для управления отображением символа-призрака.
    /// </summary>
    /// <param name="sender">Целевой элемент</param>
    /// <param name="e">Данные события DragEventArgs</param>
    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (_drawingCanvas == null || _draggingGhostSymbol == null)
        {
            return;
        }

        var dragFormat = DataFormat.CreateBytesApplicationFormat(IdentifierDragObject);
        var isValidDragElement = e.DataTransfer.Formats.Contains(dragFormat);
        
        var cursorPosition = e.GetPosition(this);
        UpdateGhostSymbolPosition(_draggingGhostSymbol, cursorPosition);
        
        var windowBounds = new Rect(0, 0, MainGrid.Bounds.Width, MainGrid.Bounds.Height);
        var isCursorInWindow = windowBounds.Contains(cursorPosition);
        var canvasPosition = e.GetPosition(_drawingCanvas);
        
        if (isValidDragElement && IsMouseOverCanvas(canvasPosition) && isCursorInWindow)
        {
            e.DragEffects = DragDropEffects.Move;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
        
        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }

    /// <summary>
    /// Обрабатывает событие Drop (остановки перемещения символа призрака) для создания нового символа на холсте.
    /// </summary>
    /// <param name="sender">Целевой элемент</param>
    /// <param name="e">Данные события DragEventArgs</param>
    #pragma warning disable CA1826
    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (_drawingCanvas == null)
        {
            return;
        }

        try
        {
            var dataItem = e.DataTransfer.Items.FirstOrDefault();
            if (dataItem == null)
            {
                return;
            }

            var symbolInfoFormat = DataFormat.CreateBytesApplicationFormat(IdentifierDragObject);
            if (!dataItem.Formats.Contains(symbolInfoFormat))
            {
                return;
            }

            if (dataItem.TryGetRaw(symbolInfoFormat) is not byte[] serializedSymbolData ||
                serializedSymbolData.Length == 0)
            {
                return;
            }

            var jsonSymbolData = Encoding.UTF8.GetString(serializedSymbolData);
            var symbolInfoDto = JsonSerializer.Deserialize<SymbolInfoDto>(jsonSymbolData);

            if (symbolInfoDto == null)
            {
                return;
            }

            var canvasPosition = e.GetPosition(_drawingCanvas);
            var windowPosition = e.GetPosition(MainGrid);
            var windowBounds = new Rect(0, 0, MainGrid.Bounds.Width, MainGrid.Bounds.Height);

            if (!IsMouseOverCanvas(canvasPosition) || !windowBounds.Contains(windowPosition))
            {
                return;
            }

            var symbolViewModel = _factorySymbolViewModel.Create(symbolInfoDto.Name);

            var xCoordinate = canvasPosition.X - symbolViewModel.Width / 2;
            var yCoordinate = canvasPosition.Y - symbolViewModel.Height / 2;

            _mainWindowViewModel.AddSymbol(symbolViewModel, xCoordinate, yCoordinate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении символа: {ex.Message}");
        }
        finally
        {
            e.Handled = true; //Прерывает всплытие события для родительского элемента
        }
    }
    #pragma warning restore CA1826

    #endregion

    #region Symbol View Events Handlers

    /// <summary>
    /// Обрабатывает нажатие на символ, размещенный на холсте.
    /// Поддерживает одиночный клик для перемещения и двойной клик для редактирования текста.
    /// </summary>
    /// <param name="sender">Визуальный элемент символа</param>
    /// <param name="e">Данные события PointerPressed</param>
    public void SymbolView_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is BaseSymbolView { DataContext: BaseSymbolViewModel symbolViewModel })
        {
            switch (e.ClickCount)
            {
                case SingleClick:
                    var pointerPosition = e.GetPosition(_drawingCanvas);
                    _mainWindowViewModel.SetMovingSymbol(symbolViewModel, pointerPosition.X, pointerPosition.Y);
                    break;
                    
                case DoubleClick:
                    MainWindowViewModel.SetEditableStatus(symbolViewModel);
                    break;
            }
        }

        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }

    #endregion

    #region Drawing Canvas Events Handlers

    /// <summary>
    /// Обрабатывает нажатие на пустую область холста.
    /// Снимает выделение со всех символов и отключает режим редактирования.
    /// </summary>
    /// <param name="sender">Холст</param>
    /// <param name="e">Данные события PointerPressed</param>
    private void DrawingCanvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _mainWindowViewModel.DeselectAllAndDisableEditing();
        
        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }

    /// <summary>
    /// Обрабатывает перемещение мыши над холстом.
    /// Обновляет позицию перемещаемого символа.
    /// </summary>
    /// <param name="sender">Холст</param>
    /// <param name="e">Данные события PointerEventArgs</param>
    public void DrawingCanvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_drawingCanvas == null)
        {
            return;
        }

        var pointerPosition = e.GetPosition(_drawingCanvas);
        _mainWindowViewModel.UpdateMovingSymbolPosition(pointerPosition.X, pointerPosition.Y);
        
        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }

    /// <summary>
    /// Обрабатывает отпускание кнопки мыши над холстом.
    /// Завершает операцию перемещения символа.
    /// </summary>
    /// <param name="sender">Холст</param>
    /// <param name="e">Данные события PointerReleasedEventArgs</param>
    private void DrawingCanvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var pointerPosition = e.GetPosition(_drawingCanvas);
        _mainWindowViewModel.StopMovingSymbol(pointerPosition.X, pointerPosition.Y);
        
        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }

    #endregion
    
    #region Configurations
    
    /// <summary>
    /// Выполняет начальную настройку сопоставления символов панели инструментов с символами-призраками.
    /// </summary>
    private void ConfigureGhostSymbolMapping()
    {
        _ghostSymbolBySymbolToolbox.Add(ActionSymbolToolbox, ActionSymbolGhost);
    }
    
    /// <summary>
    /// Обрабатывает загрузку холста и сохраняет ссылку на него.
    /// </summary>
    /// <param name="sender">Загруженный холст</param>
    /// <param name="e">Данные события RoutedEventArgs</param>
    private void DrawingCanvas_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is Canvas canvas)
        {
            _drawingCanvas = canvas;
        }

        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }
    
    #endregion
    
    #region Help Methods

    /// <summary>
    /// Сериализует SymbolInfoDto символа в массив байтов для передачи через Drag-and-Drop.
    /// </summary>
    /// <param name="symbolInfoDto">SymbolInfoDto объект для информации какой символ сейчас перемещается</param>
    /// <returns>Массив байтов, представляющий сериализованный объект</returns>
    private static byte[] SerializeSymbolInfo(SymbolInfoDto symbolInfoDto)
    {
        var jsonSymbolViewModel = JsonSerializer.Serialize(symbolInfoDto);
        
        return Encoding.UTF8.GetBytes(jsonSymbolViewModel);
    }

    /// <summary>
    /// Создает объект DataTransfer для операции Drag-and-Drop.
    /// </summary>
    /// <param name="serializedSymbolData">Сериализованные данные символа</param>
    /// <returns>Объект DataTransfer, содержащий данные символа</returns>
    private static DataTransfer CreateDragDropData(byte[] serializedSymbolData)
    {
        var dragData = new DataTransfer();
        var dataTransferItem = new DataTransferItem();
        
        var dragFormat = DataFormat.CreateBytesApplicationFormat(IdentifierDragObject);
        
        dataTransferItem.Set(dragFormat, serializedSymbolData);
        dragData.Add(dataTransferItem);

        return dragData;
    }

    /// <summary>
    /// Проверяет, находится ли курсор мыши в пределах области холста.
    /// </summary>
    /// <param name="canvasPosition">Позиция курсора относительно холста</param>
    /// <returns>True, если курсор находится в пределах холста; иначе False</returns>
    private bool IsMouseOverCanvas(Point canvasPosition)
    {
        if (_drawingCanvas == null)
        {
            return false;
        }

        return canvasPosition.X >= 0 && 
               canvasPosition.X <= _drawingCanvas.Bounds.Width &&
               canvasPosition.Y >= 0 && 
               canvasPosition.Y <= _drawingCanvas.Bounds.Height;
    }

    /// <summary>
    /// Обновляет позицию символа-призрака при перетаскивании.
    /// </summary>
    /// <param name="ghostSymbol">Символ-призрак для перемещения</param>
    /// <param name="position">Текущая позиция курсора</param>
    private static void UpdateGhostSymbolPosition(BaseSymbolGhost ghostSymbol, Point position)
    {
        //Координаты устанавливаются посередине относительно размера символа 
        var xCoordinate = position.X - ghostSymbol.Width / 2;
        var yCoordinate = position.Y - ghostSymbol.Height / 2;
        
        ghostSymbol.RenderTransform = new TranslateTransform(xCoordinate, yCoordinate);
    }

    #endregion
}