using System.Windows;
using MapEditor.Presentation.ViewModels;

namespace MapEditor.Presentation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private System.Windows.Threading.DispatcherTimer? _refreshTimer;
    private bool _refreshScheduled;

    public MainWindow()
    {
        InitializeComponent();

        // Wire up GridCanvas events
        var gridCanvas = (Controls.GridCanvas)FindName("GridCanvas");
        if (gridCanvas != null)
        {
            gridCanvas.CellLeftClicked += OnCellLeftClicked;
            gridCanvas.CellRightClicked += OnCellRightClicked;
            gridCanvas.SelectionChanged += (s, e) =>
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.SelectionWidth = e.Width;
                    vm.SelectionHeight = e.Height;
                    vm.SelectionArea = e.Area;
                    vm.StatusMessage = $"Selection: {e.Width}×{e.Height} ({e.Area})";
                }
            };
        }

        // Wire up ViewModel events
        this.DataContextChanged += (s, e) =>
        {
            if (e.NewValue is MainViewModel viewModel)
            {
                viewModel.GridRefreshRequested += (_, _) =>
                {
                    if (viewModel.IsLivePreviewEnabled)
                        DebouncedRefresh(gridCanvas);
                };
                SetupRefreshTimer(gridCanvas, viewModel);
            }
        };
    }

    private void SetupRefreshTimer(Controls.GridCanvas? gridCanvas, MainViewModel viewModel)
    {
        _refreshTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // ~20 FPS to balance smoothness and CPU
        };
        _refreshTimer.Tick += (s, e) =>
        {
            _refreshTimer?.Stop();
            _refreshScheduled = false;
            gridCanvas?.Refresh();
        };
    }

    private void DebouncedRefresh(Controls.GridCanvas? gridCanvas)
    {
        if (_refreshTimer == null)
        {
            gridCanvas?.Refresh();
            return;
        }
        if (_refreshScheduled)
            return;
        _refreshScheduled = true;
        _refreshTimer.Start();
    }

    private void OnCellLeftClicked(object? sender, Controls.CellClickedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.PlaceSquareAt(e.X, e.Y);
        }
    }

    private void OnCellRightClicked(object? sender, Controls.CellClickedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.RemoveSquareAt(e.X, e.Y);
        }
    }
}
