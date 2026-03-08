using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DiagramLab.Desktop.Core;
using DiagramLab.SymbolsUi;
using DiagramLab.SymbolsView;
using DiagramLab.SymbolsViewModel;
using DiagramLab.SymbolsViewModel.Interfaces;
using DiagramLab.SymbolsViewModel.Menus;

namespace DiagramLab.Desktop.Views;

public partial class MainWindow : Window
{
    /// <summary>
    /// Холст (Canvas), на котором отображаются символы.
    /// </summary>
    private Canvas? _drawingCanvas;

    /// <summary>
    /// ViewModel главного окна, содержащая основную бизнес-логику приложения.
    /// </summary>
    private readonly MainWindowViewModel _mainWindowViewModel = new();

    /// <summary>
    /// Фабрика для создания ViewModel символов на основе их UI-представлений.
    /// </summary>
    private readonly FactorySymbolViewModel _factorySymbolViewModel = new();
    
    /// <summary>
    /// Количество нажатий для активирования режим перемещения символа.
    /// </summary>
    private const int SingleClick = 1;

    /// <summary>
    /// Количество нажатий для перевода символ в режим редактирования текста.
    /// </summary>
    private const int DoubleClick = 2;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _mainWindowViewModel;
    }
    
    #region SymbolUi Events

    /// <summary>
    /// Обработчик нажатия на элемент символа в боковом меню (панели инструментов).
    /// Создает новый экземпляр ViewModel для соответствующего типа символа.
    /// </summary>
    /// <param name="sender">Объект BaseSymbolUi, по которому было произведено нажатие</param>
    /// <param name="e">Данные события PointerPressed, содержащие информацию о нажатии</param>
    private void SymbolUi_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is BaseSymbolUi baseSymbolUi)
        {
            var symbolViewModel = _factorySymbolViewModel.Create(baseSymbolUi.GetType());
            _mainWindowViewModel.AddSymbol(symbolViewModel);
        }
        
        e.Handled = true; //Прерывает всплытие события для родительского элемента
        e.Pointer.Capture(_drawingCanvas); //Захватывает фокус холста
    }
    
    #endregion

    #region SymbolView Events
    /// <summary>
    /// Обработчик нажатия на символ, уже размещенный на рабочем холсте.
    /// Реализует различное поведение в зависимости от количества нажатий:
    /// - Один клик: активирует режим перемещения символа,
    /// - Двойной клик: переводит символ в режим редактирования текста (если символ поддерживает текстовое поле)
    /// </summary>
    /// <param name="sender">Объект BaseSymbolView, представляющий визуальный элемент символа на холсте</param>
    /// <param name="e">Данные события PointerPressed, включающие количество нажатий и позицию курсора</param>
    public void SymbolView_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is BaseSymbolView baseSymbolView)
        {
            switch (e.ClickCount)
            {
                case SingleClick:
                {
                    if (baseSymbolView.DataContext is BaseSymbolViewModel baseSymbolViewModel)
                    {
                        var pointerPosition = e.GetPosition(_drawingCanvas);
                        _mainWindowViewModel.SetMovingSymbol(baseSymbolViewModel, pointerPosition.X, pointerPosition.Y);
                    }
                    break;
                }
                case DoubleClick:
                {
                    if (baseSymbolView.DataContext is IHasTextFieldViewModel iHasTextFieldViewModel)
                    {
                        _mainWindowViewModel.SetEditableStatus(iHasTextFieldViewModel);
                    }
                    break;
                }
            }
        }

        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }

    /// <summary>
    /// Обработчик отпускания кнопки мыши над символом на холсте.
    /// Завершает режим перемещения для конкретного символа.
    /// </summary>
    /// <param name="sender">Объект BaseSymbolView, с которым взаимодействовал пользователь</param>
    /// <param name="e">Данные события PointerReleased</param>
    public void SymbolView_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is BaseSymbolView { DataContext: BaseSymbolViewModel baseSymbolViewModel })
        {
            _mainWindowViewModel.UnsetMovingSymbol(baseSymbolViewModel);
        }

        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }
    
    #endregion SymbolView Events

    #region DrawingCanvas Events
    
    /// <summary>
    /// Обработчик события загрузки холста (DrawingCanvas).
    /// Сохраняет ссылку на объект Canvas для дальнейшего использования
    /// </summary>
    /// <param name="sender">Объект Canvas, который был загружен</param>
    /// <param name="e">Данные события RoutedEventArgs</param>
    private void DrawingCanvas_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is Canvas canvas)
        {
            _drawingCanvas = canvas;
        }
        
        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }
    
    /// <summary>
    /// Обработчик нажатия на свободное пространство холста (не на символ).
    /// Снимает режим редактирования текста со всех символов, деактивируя текстовые поля.
    /// </summary>
    /// <param name="sender">Объект Canvas, по которому было произведено нажатие</param>
    /// <param name="e">Данные события PointerPressed</param>
    private void DrawingCanvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _mainWindowViewModel.UnsetEditableStatus();
        
        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }
    
    /// <summary>
    /// Обработчик перемещения мыши над холстом.
    /// Обновляет позицию символа, который находится в режиме перемещения.
    /// </summary>
    /// <param name="sender">Объект Canvas, над которым перемещается мышь</param>
    /// <param name="e">Данные события PointerEventArgs с информацией о текущей позиции курсора</param>
    public void DrawingCanvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var pointerPosition = e.GetPosition(_drawingCanvas);
        _mainWindowViewModel.MovingSymbol(pointerPosition.X, pointerPosition.Y);

        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }
    
    /// <summary>
    /// Обработчик отпускания кнопки мыши над холстом, при первом перемещении символа.
    /// Прекращение перемещения символа (при первом создании и перемещении).
    /// </summary>
    /// <param name="sender">Объект Canvas, над которым была отпущена кнопка мыши</param>
    /// <param name="e">Данные события PointerReleasedEventArgs</param>
    private void DrawingCanvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _mainWindowViewModel.StopMovingSymbol();
        
        e.Handled = true; //Прерывает всплытие события для родительского элемента
    }
    
    #endregion
}