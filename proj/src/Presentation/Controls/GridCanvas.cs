using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Presentation.Controls;

/// <summary>
/// Custom control do renderowania i edycji Grid2D
/// </summary>
public class GridCanvas : Canvas
{
    private const int CellSize = 30;
    private Workspace? _workspace;
    private int[,]? _skeletonMatrix;
    private bool _isSkeletonOverlayVisible = true;
    private List<(int x, int y)>? _endpoints;
    private List<(int x, int y)>? _bifurcations;
    private List<(int x, int y)>? _crossings;
    private bool _showEndpoints = true;
    private bool _showBifurcations = true;
    private bool _showCrossings = true;
    private bool _showBranchAnnotations = false;

    public static readonly DependencyProperty WorkspaceProperty =
        DependencyProperty.Register(
            nameof(Workspace),
            typeof(Workspace),
            typeof(GridCanvas),
            new PropertyMetadata(null, OnWorkspaceChanged));

    public static readonly DependencyProperty SkeletonMatrixProperty =
        DependencyProperty.Register(
            nameof(SkeletonMatrix),
            typeof(int[,]),
            typeof(GridCanvas),
            new PropertyMetadata(null, OnSkeletonMatrixChanged));

    public static readonly DependencyProperty IsSkeletonOverlayVisibleProperty =
        DependencyProperty.Register(
            nameof(IsSkeletonOverlayVisible),
            typeof(bool),
            typeof(GridCanvas),
            new PropertyMetadata(true, OnIsSkeletonOverlayVisibleChanged));

    public static readonly DependencyProperty EndpointsProperty =
        DependencyProperty.Register(nameof(Endpoints), typeof(List<(int, int)>), typeof(GridCanvas),
            new PropertyMetadata(null, OnBranchDataChanged));

    public static readonly DependencyProperty BifurcationsProperty =
        DependencyProperty.Register(nameof(Bifurcations), typeof(List<(int, int)>), typeof(GridCanvas),
            new PropertyMetadata(null, OnBranchDataChanged));

    public static readonly DependencyProperty CrossingsProperty =
        DependencyProperty.Register(nameof(Crossings), typeof(List<(int, int)>), typeof(GridCanvas),
            new PropertyMetadata(null, OnBranchDataChanged));

    public static readonly DependencyProperty ShowEndpointsProperty =
        DependencyProperty.Register(nameof(ShowEndpoints), typeof(bool), typeof(GridCanvas),
            new PropertyMetadata(true, OnBranchDataChanged));

    public static readonly DependencyProperty ShowBifurcationsProperty =
        DependencyProperty.Register(nameof(ShowBifurcations), typeof(bool), typeof(GridCanvas),
            new PropertyMetadata(true, OnBranchDataChanged));

    public static readonly DependencyProperty ShowCrossingsProperty =
        DependencyProperty.Register(nameof(ShowCrossings), typeof(bool), typeof(GridCanvas),
            new PropertyMetadata(true, OnBranchDataChanged));

    public static readonly DependencyProperty ShowBranchAnnotationsProperty =
        DependencyProperty.Register(nameof(ShowBranchAnnotations), typeof(bool), typeof(GridCanvas),
            new PropertyMetadata(false, OnBranchDataChanged));

    public Workspace? Workspace
    {
        get => (Workspace?)GetValue(WorkspaceProperty);
        set => SetValue(WorkspaceProperty, value);
    }

    public int[,]? SkeletonMatrix
    {
        get => (int[,]?)GetValue(SkeletonMatrixProperty);
        set => SetValue(SkeletonMatrixProperty, value);
    }

    public bool IsSkeletonOverlayVisible
    {
        get => (bool)GetValue(IsSkeletonOverlayVisibleProperty);
        set => SetValue(IsSkeletonOverlayVisibleProperty, value);
    }

    public List<(int x, int y)>? Endpoints
    {
        get => (List<(int, int)>?)GetValue(EndpointsProperty);
        set => SetValue(EndpointsProperty, value);
    }

    public List<(int x, int y)>? Bifurcations
    {
        get => (List<(int, int)>?)GetValue(BifurcationsProperty);
        set => SetValue(BifurcationsProperty, value);
    }

    public List<(int x, int y)>? Crossings
    {
        get => (List<(int, int)>?)GetValue(CrossingsProperty);
        set => SetValue(CrossingsProperty, value);
    }

    public bool ShowEndpoints
    {
        get => (bool)GetValue(ShowEndpointsProperty);
        set => SetValue(ShowEndpointsProperty, value);
    }

    public bool ShowBifurcations
    {
        get => (bool)GetValue(ShowBifurcationsProperty);
        set => SetValue(ShowBifurcationsProperty, value);
    }

    public bool ShowCrossings
    {
        get => (bool)GetValue(ShowCrossingsProperty);
        set => SetValue(ShowCrossingsProperty, value);
    }

    public bool ShowBranchAnnotations
    {
        get => (bool)GetValue(ShowBranchAnnotationsProperty);
        set => SetValue(ShowBranchAnnotationsProperty, value);
    }

    public event EventHandler<CellClickedEventArgs>? CellLeftClicked;
    public event EventHandler<CellClickedEventArgs>? CellRightClicked;

    public GridCanvas()
    {
        Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseRightButtonDown += OnMouseRightButtonDown;
    }

    private static void OnWorkspaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridCanvas canvas)
        {
            canvas._workspace = e.NewValue as Workspace;
            canvas.RenderGrid();
        }
    }

    private static void OnSkeletonMatrixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridCanvas canvas)
        {
            canvas._skeletonMatrix = e.NewValue as int[,];
            canvas.RenderGrid();
        }
    }

    private static void OnIsSkeletonOverlayVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridCanvas canvas)
        {
            canvas._isSkeletonOverlayVisible = (bool)e.NewValue;
            canvas.RenderGrid();
        }
    }

    private static void OnBranchDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridCanvas canvas)
        {
            canvas._endpoints = canvas.Endpoints;
            canvas._bifurcations = canvas.Bifurcations;
            canvas._crossings = canvas.Crossings;
            canvas._showEndpoints = canvas.ShowEndpoints;
            canvas._showBifurcations = canvas.ShowBifurcations;
            canvas._showCrossings = canvas.ShowCrossings;
            canvas._showBranchAnnotations = canvas.ShowBranchAnnotations;
            canvas.RenderGrid();
        }
    }

    private void RenderGrid()
    {
        Children.Clear();

        if (_workspace == null)
            return;

        var grid = _workspace.Grid;
        Width = grid.Size.Width * CellSize;
        Height = grid.Size.Height * CellSize;

        // Rysuj linie siatki
        for (int x = 0; x <= grid.Size.Width; x++)
        {
            var line = new Line
            {
                X1 = x * CellSize,
                Y1 = 0,
                X2 = x * CellSize,
                Y2 = Height,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1
            };
            Children.Add(line);
        }

        for (int y = 0; y <= grid.Size.Height; y++)
        {
            var line = new Line
            {
                X1 = 0,
                Y1 = y * CellSize,
                X2 = Width,
                Y2 = y * CellSize,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1
            };
            Children.Add(line);
        }

        // Rysuj squares from active group
        foreach (var kvp in _workspace.ActiveGroup.Elements)
        {
            var position = kvp.Key;
            var square = kvp.Value;

            // Draw the square
            var rect = new Rectangle
            {
                Width = CellSize - 2,
                Height = CellSize - 2,
                Fill = GetBrushForSquareType(square.Type),
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            SetLeft(rect, position.X * CellSize + 1);
            SetTop(rect, position.Y * CellSize + 1);

            Children.Add(rect);
        }

        // Rysuj entities tylko z aktywnej grupy (spójne z kwadratami)
        foreach (var entity in _workspace.ActiveGroup.Entities.Values)
        {
            DrawEntity(entity);
        }

        // Rysuj nakładkę szkieletu
        if (_skeletonMatrix != null && _isSkeletonOverlayVisible)
        {
            DrawSkeletonOverlay(_skeletonMatrix);
        }

        // Rysuj branch points
        DrawBranchPointOverlays();
    }

    private Brush GetBrushForSquareType(SquareType type)
    {
        return type switch
        {
            SquareType.Grass => new SolidColorBrush(Color.FromRgb(34, 139, 34)),
            SquareType.Stone => new SolidColorBrush(Color.FromRgb(128, 128, 128)),
            SquareType.Water => new SolidColorBrush(Color.FromRgb(64, 164, 223)),
            SquareType.Sand => new SolidColorBrush(Color.FromRgb(238, 214, 175)),
            SquareType.Wood => new SolidColorBrush(Color.FromRgb(139, 69, 19)),
            SquareType.Metal => new SolidColorBrush(Color.FromRgb(192, 192, 192)),
            SquareType.Lava => new SolidColorBrush(Color.FromRgb(255, 69, 0)),
            _ => Brushes.White
        };
    }

    private void DrawEntity(Entity entity)
    {
        // Draw entity as circle
        var ellipse = new System.Windows.Shapes.Ellipse
        {
            Width = CellSize - 10,
            Height = CellSize - 10,
            Fill = GetBrushForEntityType(entity.Type),
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };

        SetLeft(ellipse, entity.Position.X * CellSize + 5);
        SetTop(ellipse, entity.Position.Y * CellSize + 5);

        Children.Add(ellipse);

        // Add entity label (first letter of type)
        var textBlock = new System.Windows.Controls.TextBlock
        {
            Text = entity.Type.ToString()[0].ToString(),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        SetLeft(textBlock, entity.Position.X * CellSize + CellSize / 2 - 5);
        SetTop(textBlock, entity.Position.Y * CellSize + CellSize / 2 - 10);

        Children.Add(textBlock);
    }

    private Brush GetBrushForEntityType(Domain.Shared.Enums.EntityType type)
    {
        return type switch
        {
            Domain.Shared.Enums.EntityType.Player => new SolidColorBrush(Color.FromRgb(0, 128, 255)),
            Domain.Shared.Enums.EntityType.Enemy => new SolidColorBrush(Color.FromRgb(255, 0, 0)),
            Domain.Shared.Enums.EntityType.StartPoint => new SolidColorBrush(Color.FromRgb(0, 255, 0)),
            Domain.Shared.Enums.EntityType.EndPoint => new SolidColorBrush(Color.FromRgb(255, 215, 0)),
            Domain.Shared.Enums.EntityType.Checkpoint => new SolidColorBrush(Color.FromRgb(255, 165, 0)),
            Domain.Shared.Enums.EntityType.Collectible => new SolidColorBrush(Color.FromRgb(255, 255, 0)),
            _ => Brushes.Gray
        };
    }

    private void DrawSkeletonOverlay(int[,] skeleton)
    {
        int rows = skeleton.GetLength(0);
        int cols = skeleton.GetLength(1);
        var overlayBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30));

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (skeleton[y, x] == 1)
                {
                    var rect = new Rectangle
                    {
                        Width = CellSize - 14,
                        Height = CellSize - 14,
                        Fill = overlayBrush,
                        Stroke = Brushes.Transparent
                    };
                    SetLeft(rect, x * CellSize + 7);
                    SetTop(rect, y * CellSize + 7);
                    Children.Add(rect);
                }
            }
        }
    }

    private void DrawBranchPointOverlays()
    {
        // Draw endpoints (green circles)
        if (_showEndpoints && _endpoints != null)
        {
            foreach (var point in _endpoints)
            {
                DrawBranchPoint(point.x, point.y, Color.FromRgb(0, 255, 0), "E");
            }
        }

        // Draw bifurcations (yellow circles)
        if (_showBifurcations && _bifurcations != null)
        {
            foreach (var point in _bifurcations)
            {
                DrawBranchPoint(point.x, point.y, Color.FromRgb(255, 255, 0), "B");
            }
        }

        // Draw crossings (red circles)
        if (_showCrossings && _crossings != null)
        {
            foreach (var point in _crossings)
            {
                DrawBranchPoint(point.x, point.y, Color.FromRgb(255, 0, 0), "C");
            }
        }
    }

    private void DrawBranchPoint(int x, int y, Color color, string label)
    {
        // Draw colored circle
        var ellipse = new System.Windows.Shapes.Ellipse
        {
            Width = CellSize - 8,
            Height = CellSize - 8,
            Fill = new SolidColorBrush(Color.FromArgb(128, color.R, color.G, color.B)),
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2
        };
        SetLeft(ellipse, x * CellSize + 4);
        SetTop(ellipse, y * CellSize + 4);
        Children.Add(ellipse);

        // Draw annotation text if enabled
        if (_showBranchAnnotations)
        {
            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = label,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            SetLeft(textBlock, x * CellSize + CellSize / 2 - 3);
            SetTop(textBlock, y * CellSize + CellSize / 2 - 6);
            Children.Add(textBlock);
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_workspace == null)
            return;

        var pos = e.GetPosition(this);
        int x = (int)(pos.X / CellSize);
        int y = (int)(pos.Y / CellSize);

        if (_workspace.Grid.IsValidPosition(x, y))
        {
            CellLeftClicked?.Invoke(this, new CellClickedEventArgs(x, y));
            RenderGrid(); // Re-render after change
        }
    }

    private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_workspace == null)
            return;

        var pos = e.GetPosition(this);
        int x = (int)(pos.X / CellSize);
        int y = (int)(pos.Y / CellSize);

        if (_workspace.Grid.IsValidPosition(x, y))
        {
            CellRightClicked?.Invoke(this, new CellClickedEventArgs(x, y));
            RenderGrid(); // Re-render after change
        }
    }

    // Metoda do odświeżania po zmianach zewnętrznych
    public void Refresh()
    {
        RenderGrid();
    }
}

public class CellClickedEventArgs : EventArgs
{
    public int X { get; }
    public int Y { get; }

    public CellClickedEventArgs(int x, int y)
    {
        X = x;
        Y = y;
    }
}
