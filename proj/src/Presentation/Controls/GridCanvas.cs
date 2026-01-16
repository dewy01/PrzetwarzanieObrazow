using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MapEditor.Domain.Editing.Entities;
using GridPoint = MapEditor.Domain.Editing.ValueObjects.Point;
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
    private bool _isSelecting = false;
    private GridPoint? _selectionStart;
    private List<Infrastructure.Algorithms.Canvas>? _canvases;
    private bool _showCanvases = false;

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

    public static readonly DependencyProperty CanvasesProperty =
        DependencyProperty.Register(nameof(Canvases), typeof(List<Infrastructure.Algorithms.Canvas>), typeof(GridCanvas),
            new PropertyMetadata(null, OnCanvasesChanged));

    public static readonly DependencyProperty ShowCanvasesProperty =
        DependencyProperty.Register(nameof(ShowCanvases), typeof(bool), typeof(GridCanvas),
            new PropertyMetadata(false, OnCanvasesChanged));

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

    public List<Infrastructure.Algorithms.Canvas>? Canvases
    {
        get => (List<Infrastructure.Algorithms.Canvas>?)GetValue(CanvasesProperty);
        set => SetValue(CanvasesProperty, value);
    }

    public bool ShowCanvases
    {
        get => (bool)GetValue(ShowCanvasesProperty);
        set => SetValue(ShowCanvasesProperty, value);
    }

    public event EventHandler<CellClickedEventArgs>? CellLeftClicked;
    public event EventHandler<CellClickedEventArgs>? CellRightClicked;
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    public GridCanvas()
    {
        Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseRightButtonDown += OnMouseRightButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
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

    private static void OnCanvasesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridCanvas canvas)
        {
            canvas._canvases = canvas.Canvases;
            canvas._showCanvases = canvas.ShowCanvases;
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

        // Rysuj squares from all groups (inactive with 50% opacity)
        foreach (var group in _workspace.Groups)
        {
            bool isActiveGroup = group == _workspace.ActiveGroup;
            double opacity = isActiveGroup ? 1.0 : 0.5;

            foreach (var kvp in group.Elements)
            {
                var position = kvp.Key;
                var square = kvp.Value;

                // Draw the square
                var rect = new Rectangle
                {
                    Width = CellSize - 2,
                    Height = CellSize - 2,
                    Fill = GetBrushForSquareType(square.Type),
                    Stroke = isActiveGroup ? Brushes.Black : Brushes.Gray,
                    StrokeThickness = 1,
                    Opacity = opacity
                };

                SetLeft(rect, position.X * CellSize + 1);
                SetTop(rect, position.Y * CellSize + 1);

                // Apply rotation if not 0
                if (square.Rotation != 0)
                {
                    var rotateTransform = new RotateTransform(square.Rotation);
                    // Set rotation center to middle of the square
                    rotateTransform.CenterX = (CellSize - 2) / 2.0;
                    rotateTransform.CenterY = (CellSize - 2) / 2.0;
                    rect.RenderTransform = rotateTransform;
                }

                Children.Add(rect);
            }

            // Rysuj entities from this group
            foreach (var entity in group.Entities.Values)
            {
                DrawEntity(entity, opacity, isActiveGroup);
            }
        }

        // Rysuj nakładkę szkieletu
        if (_skeletonMatrix != null && _isSkeletonOverlayVisible)
        {
            DrawSkeletonOverlay(_skeletonMatrix);
        }

        // Rysuj canvases
        if (_showCanvases && _canvases != null)
        {
            DrawCanvasOverlays(_canvases);
        }

        // Rysuj branch points
        DrawBranchPointOverlays();

        // Rysuj zaznaczenie (Selection)
        DrawSelectionOverlay();
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

    private void DrawEntity(Entity entity, double opacity = 1.0, bool isActive = true)
    {
        // Draw entity as circle
        var ellipse = new System.Windows.Shapes.Ellipse
        {
            Width = CellSize - 10,
            Height = CellSize - 10,
            Fill = GetBrushForEntityType(entity.Type),
            Opacity = opacity,
            Stroke = isActive ? Brushes.Black : Brushes.Gray,
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
            // Draw label letter (E/B/C)
            var labelBlock = new System.Windows.Controls.TextBlock
            {
                Text = label,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            SetLeft(labelBlock, x * CellSize + CellSize / 2 - 4);
            SetTop(labelBlock, y * CellSize + CellSize / 2 - 8);
            Children.Add(labelBlock);

            // Draw coordinate label below
            var coordBlock = new System.Windows.Controls.TextBlock
            {
                Text = $"({x},{y})",
                FontSize = 8,
                Foreground = new SolidColorBrush(color),
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                Padding = new Thickness(2, 0, 2, 0)
            };
            SetLeft(coordBlock, x * CellSize + 2);
            SetTop(coordBlock, (y + 1) * CellSize - 12);
            Children.Add(coordBlock);
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
            // Hold Shift to start selection rectangle
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                _isSelecting = true;
                _selectionStart = new GridPoint(x, y);
                _workspace.SetSelection(_selectionStart, _selectionStart);
                SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(_workspace.Selection.Width, _workspace.Selection.Height, _workspace.Selection.Area));
                CaptureMouse();
                RenderGrid();
            }
            else
            {
                CellLeftClicked?.Invoke(this, new CellClickedEventArgs(x, y));
                RenderGrid(); // Re-render after change
            }
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

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (_workspace == null || !_isSelecting || _selectionStart == null)
            return;

        var pos = e.GetPosition(this);
        int x = Math.Max(0, Math.Min((int)(pos.X / CellSize), _workspace.Grid.Size.Width - 1));
        int y = Math.Max(0, Math.Min((int)(pos.Y / CellSize), _workspace.Grid.Size.Height - 1));

        var current = new GridPoint(x, y);
        _workspace.SetSelection(_selectionStart, current);
        SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(_workspace.Selection.Width, _workspace.Selection.Height, _workspace.Selection.Area));
        RenderGrid();
    }

    private void OnMouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        if (_isSelecting)
        {
            _isSelecting = false;
            _selectionStart = null;
            ReleaseMouseCapture();
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(_workspace!.Selection.Width, _workspace.Selection.Height, _workspace.Selection.Area));
            RenderGrid();
        }
    }

    // Metoda do odświeżania po zmianach zewnętrznych
    public void Refresh()
    {
        RenderGrid();
    }

    private void DrawSelectionOverlay()
    {
        if (_workspace == null || _workspace.Selection == null || !_workspace.Selection.IsActive)
            return;

        var sel = _workspace.Selection;
        var start = sel.StartPoint;
        var end = sel.EndPoint;

        int x = Math.Min(start.X, end.X);
        int y = Math.Min(start.Y, end.Y);
        int w = Math.Abs(end.X - start.X) + 1;
        int h = Math.Abs(end.Y - start.Y) + 1;

        // Overlay background
        var overlay = new Rectangle
        {
            Width = w * CellSize,
            Height = h * CellSize,
            Fill = new SolidColorBrush(Color.FromArgb(60, 30, 144, 255)), // semi-transparent DodgerBlue
            Stroke = Brushes.DodgerBlue,
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 4, 2 }
        };
        SetLeft(overlay, x * CellSize);
        SetTop(overlay, y * CellSize);
        Children.Add(overlay);
    }

    private void DrawCanvasOverlays(List<Infrastructure.Algorithms.Canvas> canvases)
    {
        if (canvases == null || canvases.Count == 0)
            return;

        // Use different colors for each canvas (cycling through a palette)
        var colors = new[]
        {
            Color.FromArgb(80, 255, 0, 0),     // Red
            Color.FromArgb(80, 0, 0, 255),     // Blue
            Color.FromArgb(80, 0, 255, 0),     // Green
            Color.FromArgb(80, 255, 255, 0),   // Yellow
            Color.FromArgb(80, 255, 0, 255),   // Magenta
            Color.FromArgb(80, 0, 255, 255),   // Cyan
        };

        for (int i = 0; i < canvases.Count; i++)
        {
            var canvas = canvases[i];
            var color = colors[i % colors.Length];

            foreach (var cell in canvas.Cells)
            {
                var rect = new Rectangle
                {
                    Width = CellSize - 2,
                    Height = CellSize - 2,
                    Fill = new SolidColorBrush(color),
                    Stroke = Brushes.Transparent
                };
                SetLeft(rect, cell.X * CellSize + 1);
                SetTop(rect, cell.Y * CellSize + 1);
                Children.Add(rect);
            }

            // Draw canvas ID label at center
            var bbox = canvas.GetBoundingBox();
            int centerX = (bbox.minX + bbox.maxX) / 2;
            int centerY = (bbox.minY + bbox.maxY) / 2;

            var label = new System.Windows.Controls.TextBlock
            {
                Text = $"#{canvas.Id}\n{canvas.Size} cells",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                Padding = new Thickness(3),
                TextAlignment = System.Windows.TextAlignment.Center
            };
            SetLeft(label, centerX * CellSize);
            SetTop(label, centerY * CellSize);
            Children.Add(label);
        }
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

public class SelectionChangedEventArgs : EventArgs
{
    public int Width { get; }
    public int Height { get; }
    public int Area { get; }

    public SelectionChangedEventArgs(int width, int height, int area)
    {
        Width = width;
        Height = height;
        Area = area;
    }
}
