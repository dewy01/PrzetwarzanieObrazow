using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MapEditor.Application.Services;
using MapEditor.Domain.Biometric.Services;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Shared.Enums;
using MapEditor.Presentation.Commands;
using Microsoft.Win32;
using MapEditor.Domain.Editing.Services;

namespace MapEditor.Presentation.ViewModels;

/// <summary>
/// ViewModel dla głównego okna aplikacji
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly EditingService _editingService;
    private readonly IUL22Converter _ul22Converter;
    private readonly IPreprocessingService _preprocessingService;
    private readonly IFragmentationService _fragmentationService;
    private readonly ISkeletonizationService _skeletonizationService;
    private readonly IBranchDetectionService _branchDetectionService;
    private readonly IImageImportService _imageImportService;
    private readonly IPresetRepository _presetRepository;
    private Workspace? _currentWorkspace;
    private SquareType _selectedSquareType = SquareType.Grass;
    private string _statusMessage = "Ready";
    private bool _isFillModeEnabled = false;
    private bool _isEntityModeEnabled = false;
    private bool _isPresetModeEnabled = false;
    private bool _isLoading = false;
    private bool _isLivePreviewEnabled = true;
    private int[,]? _skeletonMatrix;
    private bool _isSkeletonOverlayVisible = true;
    private List<(int x, int y)>? _endpoints;
    private List<(int x, int y)>? _bifurcations;
    private List<(int x, int y)>? _crossings;
    private bool _showEndpoints = true;
    private bool _showBifurcations = true;
    private bool _showCrossings = true;
    private bool _showBranchAnnotations = false;
    private Domain.Shared.Enums.EntityType _selectedEntityType = Domain.Shared.Enums.EntityType.Player;
    private Group? _selectedGroup;
    private Preset? _selectedPreset;

    public MainViewModel(EditingService editingService, IUL22Converter ul22Converter, IPreprocessingService preprocessingService, IFragmentationService fragmentationService, ISkeletonizationService skeletonizationService, IBranchDetectionService branchDetectionService, IImageImportService imageImportService, IPresetRepository presetRepository)
    {
        _editingService = editingService ?? throw new ArgumentNullException(nameof(editingService));
        _ul22Converter = ul22Converter ?? throw new ArgumentNullException(nameof(ul22Converter));
        _preprocessingService = preprocessingService ?? throw new ArgumentNullException(nameof(preprocessingService));
        _fragmentationService = fragmentationService ?? throw new ArgumentNullException(nameof(fragmentationService));
        _skeletonizationService = skeletonizationService ?? throw new ArgumentNullException(nameof(skeletonizationService));
        _branchDetectionService = branchDetectionService ?? throw new ArgumentNullException(nameof(branchDetectionService));
        _imageImportService = imageImportService ?? throw new ArgumentNullException(nameof(imageImportService));
        _presetRepository = presetRepository ?? throw new ArgumentNullException(nameof(presetRepository));
        _editingService.WorkspaceChanged += OnWorkspaceChanged;

        // Commands
        NewWorkspaceCommand = new RelayCommand(_ => ExecuteNewWorkspace());
        SaveWorkspaceCommand = new RelayCommand(_ => ExecuteSaveWorkspace(), _ => CurrentWorkspace != null);
        LoadWorkspaceCommand = new RelayCommand(_ => ExecuteLoadWorkspace());
        UndoCommand = new RelayCommand(_ => ExecuteUndo(), _ => _editingService.CanUndo);
        RedoCommand = new RelayCommand(_ => ExecuteRedo(), _ => _editingService.CanRedo);
        ShowUL22MatrixCommand = new RelayCommand(_ => ExecuteShowUL22Matrix(), _ => CurrentWorkspace != null);
        PreprocessMatrixCommand = new AsyncRelayCommand(_ => ExecutePreprocessMatrixAsync(), _ => CurrentWorkspace != null);
        RunFragmentationCommand = new AsyncRelayCommand(_ => ExecuteRunFragmentationAsync(), _ => CurrentWorkspace != null);
        RunSkeletonizationCommand = new AsyncRelayCommand(_ => ExecuteRunSkeletonizationAsync(), _ => CurrentWorkspace != null);
        RunK3MSkeletonizationCommand = new AsyncRelayCommand(_ => ExecuteRunK3MSkeletonizationAsync(), _ => CurrentWorkspace != null);
        RunBranchDetectionCommand = new AsyncRelayCommand(_ => ExecuteRunBranchDetectionAsync(), _ => CurrentWorkspace != null);
        ClearSkeletonCommand = new RelayCommand(_ => ExecuteClearSkeleton(), _ => SkeletonMatrix != null);
        ShowGridFeaturesCommand = new RelayCommand(_ => ExecuteShowGridFeatures(), _ => CurrentWorkspace != null);
        ToggleFillModeCommand = new RelayCommand(_ => ExecuteToggleFillMode());
        ToggleEntityModeCommand = new RelayCommand(_ => ExecuteToggleEntityMode());
        TogglePresetModeCommand = new RelayCommand(_ => ExecuteTogglePresetMode());
        LoadPresetsCommand = new RelayCommand(_ => ExecuteLoadPresets());
        ChangeGroupCommand = new RelayCommand(ExecuteChangeGroup, _ => CurrentWorkspace != null);
        AddGroupCommand = new RelayCommand(_ => ExecuteAddGroup(), _ => CurrentWorkspace != null);
        ImportImageCommand = new RelayCommand(_ => ExecuteImportImage());
        ExitCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());

        // Square types dla UI
        AvailableSquareTypes = new ObservableCollection<SquareType>(
            Enum.GetValues<SquareType>().Where(t => t != SquareType.Empty)
        );

        // Entity types dla UI
        AvailableEntityTypes = new ObservableCollection<Domain.Shared.Enums.EntityType>(
            Enum.GetValues<Domain.Shared.Enums.EntityType>().Where(t => t != Domain.Shared.Enums.EntityType.None)
        );
    }

    public Workspace? CurrentWorkspace
    {
        get => _currentWorkspace;
        private set
        {
            if (SetProperty(ref _currentWorkspace, value))
            {
                OnPropertyChanged(nameof(GridWidth));
                OnPropertyChanged(nameof(GridHeight));
                OnPropertyChanged(nameof(WorkspaceName));
            }
        }
    }

    public int GridWidth => CurrentWorkspace?.Grid.Size.Width ?? 0;
    public int GridHeight => CurrentWorkspace?.Grid.Size.Height ?? 0;
    public string WorkspaceName => CurrentWorkspace?.Name ?? "No Workspace";

    public SquareType SelectedSquareType
    {
        get => _selectedSquareType;
        set => SetProperty(ref _selectedSquareType, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsFillModeEnabled
    {
        get => _isFillModeEnabled;
        set => SetProperty(ref _isFillModeEnabled, value);
    }

    public bool IsEntityModeEnabled
    {
        get => _isEntityModeEnabled;
        set => SetProperty(ref _isEntityModeEnabled, value);
    }

    public bool IsPresetModeEnabled
    {
        get => _isPresetModeEnabled;
        set => SetProperty(ref _isPresetModeEnabled, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool IsLivePreviewEnabled
    {
        get => _isLivePreviewEnabled;
        set
        {
            if (SetProperty(ref _isLivePreviewEnabled, value))
            {
                // Trigger a refresh when toggled
                GridRefreshRequested?.Invoke(this, EventArgs.Empty);
                StatusMessage = _isLivePreviewEnabled ? "Real-time Preview: ON" : "Real-time Preview: OFF";
            }
        }
    }

    public int[,]? SkeletonMatrix
    {
        get => _skeletonMatrix;
        private set
        {
            if (SetProperty(ref _skeletonMatrix, value))
            {
                GridRefreshRequested?.Invoke(this, EventArgs.Empty);
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsSkeletonOverlayVisible
    {
        get => _isSkeletonOverlayVisible;
        set
        {
            if (SetProperty(ref _isSkeletonOverlayVisible, value))
            {
                GridRefreshRequested?.Invoke(this, EventArgs.Empty);
                StatusMessage = _isSkeletonOverlayVisible ? "Skeleton Overlay: ON" : "Skeleton Overlay: OFF";
            }
        }
    }

    public List<(int x, int y)>? Endpoints
    {
        get => _endpoints;
        private set
        {
            if (SetProperty(ref _endpoints, value))
            {
                GridRefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public List<(int x, int y)>? Bifurcations
    {
        get => _bifurcations;
        private set
        {
            if (SetProperty(ref _bifurcations, value))
            {
                GridRefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public List<(int x, int y)>? Crossings
    {
        get => _crossings;
        private set
        {
            if (SetProperty(ref _crossings, value))
            {
                GridRefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public bool ShowEndpoints
    {
        get => _showEndpoints;
        set
        {
            if (SetProperty(ref _showEndpoints, value))
            {
                GridRefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public bool ShowBifurcations
    {
        get => _showBifurcations;
        set
        {
            if (SetProperty(ref _showBifurcations, value))
            {
                GridRefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public bool ShowCrossings
    {
        get => _showCrossings;
        set
        {
            if (SetProperty(ref _showCrossings, value))
            {
                GridRefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public bool ShowBranchAnnotations
    {
        get => _showBranchAnnotations;
        set
        {
            if (SetProperty(ref _showBranchAnnotations, value))
            {
                GridRefreshRequested?.Invoke(this, EventArgs.Empty);
                StatusMessage = _showBranchAnnotations ? "Branch Annotations: ON" : "Branch Annotations: OFF";
            }
        }
    }

    public Domain.Shared.Enums.EntityType SelectedEntityType
    {
        get => _selectedEntityType;
        set => SetProperty(ref _selectedEntityType, value);
    }

    public Preset? SelectedPreset
    {
        get => _selectedPreset;
        set => SetProperty(ref _selectedPreset, value);
    }

    public ObservableCollection<SquareType> AvailableSquareTypes { get; }
    public ObservableCollection<Domain.Shared.Enums.EntityType> AvailableEntityTypes { get; }
    public ObservableCollection<Group> AvailableGroups { get; } = new();
    public ObservableCollection<Preset> AvailablePresets { get; } = new();

    public event EventHandler? GridRefreshRequested;

    public Group? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetProperty(ref _selectedGroup, value) && value != null)
            {
                ExecuteChangeGroup(value);
            }
        }
    }

    public string ActiveGroupName => CurrentWorkspace?.ActiveGroup.Name ?? "No Group";

    // Commands
    public ICommand NewWorkspaceCommand { get; }
    public ICommand SaveWorkspaceCommand { get; }
    public ICommand LoadWorkspaceCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }
    public ICommand ShowUL22MatrixCommand { get; }
    public ICommand PreprocessMatrixCommand { get; }
    public ICommand RunFragmentationCommand { get; }
    public ICommand RunSkeletonizationCommand { get; }
    public ICommand RunK3MSkeletonizationCommand { get; }
    public ICommand RunBranchDetectionCommand { get; }
    public ICommand ClearSkeletonCommand { get; }
    public ICommand ShowGridFeaturesCommand { get; }
    public ICommand ToggleFillModeCommand { get; }
    public ICommand ToggleEntityModeCommand { get; }
    public ICommand TogglePresetModeCommand { get; }
    public ICommand LoadPresetsCommand { get; }
    public ICommand ChangeGroupCommand { get; }
    public ICommand AddGroupCommand { get; }
    public ICommand ImportImageCommand { get; }
    public ICommand ExitCommand { get; }

    public void PlaceSquareAt(int x, int y)
    {
        try
        {
            if (IsPresetModeEnabled && SelectedPreset != null)
            {
                _editingService.PlacePreset(x, y, SelectedPreset);
                StatusMessage = $"Placed preset '{SelectedPreset.Name}' at ({x}, {y})";
            }
            else if (IsEntityModeEnabled)
            {
                _editingService.PlaceEntity(x, y, SelectedEntityType);
                StatusMessage = $"Placed {SelectedEntityType} entity at ({x}, {y})";
            }
            else if (IsFillModeEnabled)
            {
                _editingService.FillRegion(x, y, SelectedSquareType);
                StatusMessage = $"Filled region at ({x}, {y}) with {SelectedSquareType}";
            }
            else
            {
                _editingService.PlaceSquare(x, y, SelectedSquareType);
                StatusMessage = $"Placed {SelectedSquareType} at ({x}, {y})";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    public void RemoveSquareAt(int x, int y)
    {
        try
        {
            // If entity mode is on, try to remove entity first
            if (IsEntityModeEnabled)
            {
                var entity = CurrentWorkspace?.GetEntityAt(new Domain.Editing.ValueObjects.Point(x, y));
                if (entity != null)
                {
                    _editingService.RemoveEntity(x, y);
                    StatusMessage = $"Removed entity at ({x}, {y})";
                    return;
                }
            }

            // Otherwise remove square
            _editingService.RemoveSquare(x, y);
            StatusMessage = $"Removed square at ({x}, {y})";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async void ExecuteNewWorkspace()
    {
        try
        {
            // W przyszłości: dialog do wprowadzenia nazwy i rozmiaru
            var workspace = await _editingService.CreateWorkspaceAsync("New Map", 20, 15);
            StatusMessage = "Created new workspace";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create workspace: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ExecuteSaveWorkspace()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Map Workspace (*.workspace)|*.workspace|All Files (*.*)|*.*",
                DefaultExt = ".workspace",
                FileName = CurrentWorkspace?.Name ?? "map"
            };

            if (dialog.ShowDialog() == true)
            {
                await _editingService.SaveWorkspaceAsync(dialog.FileName);
                StatusMessage = $"Saved to {Path.GetFileName(dialog.FileName)}";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save workspace: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ExecuteLoadWorkspace()
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Map Workspace (*.workspace)|*.workspace|All Files (*.*)|*.*",
                DefaultExt = ".workspace"
            };

            if (dialog.ShowDialog() == true)
            {
                await _editingService.LoadWorkspaceAsync(dialog.FileName);
                StatusMessage = $"Loaded {Path.GetFileName(dialog.FileName)}";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load workspace: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task LoadWorkspaceFromPathAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                StatusMessage = "Default workspace path not found";
                return;
            }

            await _editingService.LoadWorkspaceAsync(path);
            StatusMessage = $"Loaded {Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to auto-load workspace: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnWorkspaceChanged(object? sender, EventArgs e)
    {
        CurrentWorkspace = _editingService.CurrentWorkspace;

        // Update available groups
        AvailableGroups.Clear();
        if (CurrentWorkspace != null)
        {
            foreach (var group in CurrentWorkspace.Groups)
            {
                AvailableGroups.Add(group);
            }
            _selectedGroup = CurrentWorkspace.ActiveGroup;
            OnPropertyChanged(nameof(SelectedGroup));
            OnPropertyChanged(nameof(ActiveGroupName));
        }

        // Notify that command availability may have changed
        CommandManager.InvalidateRequerySuggested();

        // Request grid refresh for real-time preview
        GridRefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ExecuteUndo()
    {
        try
        {
            _editingService.Undo();
            StatusMessage = "Undo successful";
            GridRefreshRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Undo failed: {ex.Message}";
        }
    }

    private void ExecuteRedo()
    {
        try
        {
            _editingService.Redo();
            StatusMessage = "Redo successful";
            GridRefreshRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Redo failed: {ex.Message}";
        }
    }

    private void ExecuteShowUL22Matrix()
    {
        try
        {
            if (CurrentWorkspace == null)
                return;

            var matrix = _ul22Converter.ConvertToUL22(CurrentWorkspace);
            var (rows, columns) = _ul22Converter.GetMatrixDimensions(CurrentWorkspace);

            var sb = new StringBuilder();
            sb.AppendLine($"UL22 Binary Matrix ({rows}x{columns})");
            sb.AppendLine(new string('-', 40));

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    sb.Append(matrix[y, x]);
                    if (x < columns - 1)
                        sb.Append(' ');
                }
                sb.AppendLine();
            }

            MessageBox.Show(sb.ToString(), "UL22 Binary Matrix", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = "Displayed UL22 matrix";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to convert to UL22: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void ExecutePreprocessMatrix()
    {
        try
        {
            IsLoading = true;
            if (CurrentWorkspace == null)
                return;

            // Convert to UL22 first
            var matrix = _ul22Converter.ConvertToUL22(CurrentWorkspace);

            // Apply preprocessing (median filter + Otsu)
            var preprocessed = _preprocessingService.Preprocess(matrix, kernelSize: 3);

            var (rows, columns) = _ul22Converter.GetMatrixDimensions(CurrentWorkspace);

            var sb = new StringBuilder();
            sb.AppendLine($"Preprocessed Matrix ({rows}x{columns})");
            sb.AppendLine("Applied: Median Filter (3x3) + Otsu Binarization");
            sb.AppendLine(new string('-', 40));

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    sb.Append(preprocessed[y, x]);
                    if (x < columns - 1)
                        sb.Append(' ');
                }
                sb.AppendLine();
            }

            MessageBox.Show(sb.ToString(), "Preprocessed Matrix", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = "Preprocessing completed (Median + Otsu)";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to preprocess matrix: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecutePreprocessMatrixAsync()
    {
        await Task.Run(() => ExecutePreprocessMatrix());
    }

    private void ExecuteRunFragmentation()
    {
        try
        {
            IsLoading = true;
            if (CurrentWorkspace == null)
                return;

            // Convert to UL22 first
            var matrix = _ul22Converter.ConvertToUL22(CurrentWorkspace);

            // Detect fragments
            var fragments = _fragmentationService.DetectFragments(matrix);
            var stats = _fragmentationService.CalculateStatistics(fragments);

            var sb = new StringBuilder();
            sb.AppendLine("Fragmentation Analysis");
            sb.AppendLine(new string('=', 40));
            sb.AppendLine($"Total Fragments: {stats["FragmentCount"]}");

            if (stats["FragmentCount"] > 0)
            {
                sb.AppendLine($"Total Pixels: {stats["TotalPixels"]}");
                sb.AppendLine($"Average Fragment Size: {stats["AverageSize"]:F2} pixels");
                sb.AppendLine($"Largest Fragment: {stats["LargestSize"]} pixels");
                sb.AppendLine($"Smallest Fragment: {stats["SmallestSize"]} pixels");
                sb.AppendLine();
                sb.AppendLine("Fragment Details:");
                sb.AppendLine(new string('-', 40));

                foreach (var fragment in fragments.OrderByDescending(f => f.PixelCount).Take(10))
                {
                    sb.AppendLine($"Fragment {fragment.Id}: {fragment.PixelCount} px, " +
                                $"BBox: ({fragment.MinX},{fragment.MinY})-({fragment.MaxX},{fragment.MaxY}), " +
                                $"Size: {fragment.Width}x{fragment.Height}");
                }

                if (fragments.Count > 10)
                {
                    sb.AppendLine($"... and {fragments.Count - 10} more fragments");
                }
            }
            else
            {
                sb.AppendLine("No fragments detected (workspace is empty)");
            }

            MessageBox.Show(sb.ToString(), "Fragmentation Results", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = $"Fragmentation completed: {fragments.Count} fragments found";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to run fragmentation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteRunFragmentationAsync()
    {
        await Task.Run(() => ExecuteRunFragmentation());
    }

    private void ExecuteRunSkeletonization()
    {
        try
        {
            IsLoading = true;
            if (CurrentWorkspace == null)
                return;

            // Convert to UL22 first
            var matrix = _ul22Converter.ConvertToUL22(CurrentWorkspace);

            // Apply skeletonization
            var (skeleton, iterations) = _skeletonizationService.ZhangSuenWithIterations(matrix);
            SkeletonMatrix = skeleton;
            var metrics = _skeletonizationService.CalculateSkeletonMetrics(skeleton);

            var sb = new StringBuilder();
            sb.AppendLine("Zhang-Suen Skeletonization");
            sb.AppendLine(new string('=', 40));
            sb.AppendLine($"Algorithm converged in {iterations} iterations");
            sb.AppendLine();
            sb.AppendLine("Skeleton Metrics:");
            sb.AppendLine(new string('-', 40));
            sb.AppendLine($"Skeleton Pixels: {metrics["SkeletonPixels"]}");
            sb.AppendLine($"Endpoints: {metrics["Endpoints"]}");
            sb.AppendLine($"Junctions: {metrics["Junctions"]}");
            sb.AppendLine($"Branches: {metrics["Branches"]}");
            sb.AppendLine();
            sb.AppendLine("Skeleton Matrix:");
            sb.AppendLine(new string('-', 40));

            var (rows, columns) = _ul22Converter.GetMatrixDimensions(CurrentWorkspace);
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    sb.Append(skeleton[y, x]);
                    if (x < columns - 1)
                        sb.Append(' ');
                }
                sb.AppendLine();
            }

            MessageBox.Show(sb.ToString(), "Skeletonization Results", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = $"Skeletonization completed: {metrics["SkeletonPixels"]} skeleton pixels, {metrics["Endpoints"]} endpoints";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to run skeletonization: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteRunSkeletonizationAsync()
    {
        await Task.Run(() => ExecuteRunSkeletonization());
    }

    private void ExecuteRunK3MSkeletonization()
    {
        try
        {
            IsLoading = true;
            if (CurrentWorkspace == null)
                return;

            // Convert to UL22 first
            var matrix = _ul22Converter.ConvertToUL22(CurrentWorkspace);

            // Apply K3M skeletonization
            var (skeleton, iterations) = _skeletonizationService.K3MWithIterations(matrix);
            SkeletonMatrix = skeleton;
            var metrics = _skeletonizationService.CalculateSkeletonMetrics(skeleton);

            var sb = new StringBuilder();
            sb.AppendLine("K3M Skeletonization");
            sb.AppendLine(new string('=', 40));
            sb.AppendLine($"Algorithm converged in {iterations} iterations");
            sb.AppendLine();
            sb.AppendLine("Skeleton Metrics:");
            sb.AppendLine(new string('-', 40));
            sb.AppendLine($"Skeleton Pixels: {metrics["SkeletonPixels"]}");
            sb.AppendLine($"Endpoints: {metrics["Endpoints"]}");
            sb.AppendLine($"Junctions: {metrics["Junctions"]}");
            sb.AppendLine($"Branches: {metrics["Branches"]}");
            sb.AppendLine();
            sb.AppendLine("Skeleton Matrix:");
            sb.AppendLine(new string('-', 40));

            var (rows, columns) = _ul22Converter.GetMatrixDimensions(CurrentWorkspace);
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    sb.Append(skeleton[y, x]);
                    if (x < columns - 1)
                        sb.Append(' ');
                }
                sb.AppendLine();
            }

            MessageBox.Show(sb.ToString(), "K3M Skeletonization Results", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = $"K3M Skeletonization completed: {metrics["SkeletonPixels"]} skeleton pixels, {metrics["Endpoints"]} endpoints";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to run K3M skeletonization: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteRunK3MSkeletonizationAsync()
    {
        await Task.Run(() => ExecuteRunK3MSkeletonization());
    }

    private void ExecuteRunBranchDetection()
    {
        try
        {
            IsLoading = true;
            if (CurrentWorkspace == null)
                return;

            // Convert to UL22
            var matrix = _ul22Converter.ConvertToUL22(CurrentWorkspace);

            // Apply skeletonization first
            var skeleton = _skeletonizationService.ZhangSuenThinning(matrix);
            SkeletonMatrix = skeleton;

            // Detect branches
            var branchAnalysis = _branchDetectionService.AnalyzeBranchStructure(skeleton);

            // Store branch detection results for visualization
            Endpoints = (List<(int x, int y)>)branchAnalysis["Endpoints"];
            Bifurcations = (List<(int x, int y)>)branchAnalysis["Bifurcations"];
            Crossings = (List<(int x, int y)>)branchAnalysis["Crossings"];

            var sb = new StringBuilder();
            sb.AppendLine("Branch Detection Analysis");
            sb.AppendLine(new string('=', 50));
            sb.AppendLine();
            sb.AppendLine("Branch Point Classification:");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"Endpoints:     {branchAnalysis["EndpointCount"],6}");
            sb.AppendLine($"Bifurcations:  {branchAnalysis["BifurcationCount"],6}");
            sb.AppendLine($"Crossings:     {branchAnalysis["CrossingCount"],6}");
            sb.AppendLine($"Total Branch Points: {branchAnalysis["TotalBranchPoints"],6}");
            sb.AppendLine();
            sb.AppendLine("Structural Metrics:");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"Total Skeleton Pixels: {branchAnalysis["TotalSkeletonPixels"]}");
            sb.AppendLine($"Branch Density: {(double)branchAnalysis["BranchDensity"]:F2} per 100 pixels");
            sb.AppendLine($"Branch Complexity: {(double)branchAnalysis["BranchComplexity"]:F3}");
            sb.AppendLine($"Avg Endpoint Distance: {(double)branchAnalysis["AverageEndpointDistance"]:F2} pixels");
            sb.AppendLine($"Avg Bifurcation Distance: {(double)branchAnalysis["AverageBifurcationDistance"]:F2} pixels");
            sb.AppendLine();

            var endpoints = (List<(int x, int y)>)branchAnalysis["Endpoints"];
            var bifurcations = (List<(int x, int y)>)branchAnalysis["Bifurcations"];

            if (endpoints.Count > 0)
            {
                sb.AppendLine($"First {Math.Min(5, endpoints.Count)} Endpoint(s):");
                sb.AppendLine(new string('-', 50));
                for (int i = 0; i < Math.Min(5, endpoints.Count); i++)
                {
                    sb.AppendLine($"  [{i + 1}] Position: ({endpoints[i].x}, {endpoints[i].y})");
                }
                if (endpoints.Count > 5)
                    sb.AppendLine($"  ... and {endpoints.Count - 5} more endpoints");
                sb.AppendLine();
            }

            if (bifurcations.Count > 0)
            {
                sb.AppendLine($"First {Math.Min(5, bifurcations.Count)} Bifurcation(s):");
                sb.AppendLine(new string('-', 50));
                for (int i = 0; i < Math.Min(5, bifurcations.Count); i++)
                {
                    sb.AppendLine($"  [{i + 1}] Position: ({bifurcations[i].x}, {bifurcations[i].y})");
                }
                if (bifurcations.Count > 5)
                    sb.AppendLine($"  ... and {bifurcations.Count - 5} more bifurcations");
            }

            MessageBox.Show(sb.ToString(), "Branch Detection Results", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = $"Branch detection completed: {branchAnalysis["EndpointCount"]} endpoints, {branchAnalysis["BifurcationCount"]} bifurcations";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to run branch detection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteRunBranchDetectionAsync()
    {
        await Task.Run(() => ExecuteRunBranchDetection());
    }

    private void ExecuteClearSkeleton()
    {
        SkeletonMatrix = null;
        StatusMessage = "Skeleton overlay cleared";
    }

    private void ExecuteToggleFillMode()
    {
        IsFillModeEnabled = !IsFillModeEnabled;
        if (IsFillModeEnabled)
        {
            IsEntityModeEnabled = false;
            IsPresetModeEnabled = false;
        }
        StatusMessage = IsFillModeEnabled
            ? "Fill Mode: ON - Click to fill regions"
            : "Fill Mode: OFF - Click to place single squares";
    }

    private void ExecuteToggleEntityMode()
    {
        IsEntityModeEnabled = !IsEntityModeEnabled;
        if (IsEntityModeEnabled)
        {
            IsFillModeEnabled = false;
            IsPresetModeEnabled = false;
        }
        StatusMessage = IsEntityModeEnabled
            ? "Entity Mode: ON - Click to place entities"
            : "Entity Mode: OFF - Click to place squares";
    }

    private void ExecuteTogglePresetMode()
    {
        IsPresetModeEnabled = !IsPresetModeEnabled;
        if (IsPresetModeEnabled)
        {
            IsFillModeEnabled = false;
            IsEntityModeEnabled = false;
        }
        StatusMessage = IsPresetModeEnabled
            ? "Preset Mode: ON - Click to place preset"
            : "Preset Mode: OFF - Click to place squares";
    }

    private async void ExecuteLoadPresets()
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Preset Files (*.preset.json)|*.preset.json|All Files (*.*)|*.*",
                DefaultExt = ".preset.json",
                Multiselect = true,
                Title = "Select Preset Files"
            };

            if (dialog.ShowDialog() == true)
            {
                AvailablePresets.Clear();
                int loadedCount = 0;

                foreach (var filePath in dialog.FileNames)
                {
                    try
                    {
                        var preset = await _presetRepository.LoadAsync(filePath);
                        AvailablePresets.Add(preset);
                        loadedCount++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load {Path.GetFileName(filePath)}: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                StatusMessage = $"Loaded {loadedCount} preset(s)";

                if (AvailablePresets.Count > 0)
                {
                    SelectedPreset = AvailablePresets[0];
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load presets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void ExecuteChangeGroup(object? parameter)
    {
        try
        {
            if (parameter is Group group && CurrentWorkspace != null)
            {
                _editingService.SetActiveGroup(group);
                StatusMessage = $"Switched to group: {group.Name}";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to change group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void ExecuteAddGroup()
    {
        try
        {
            if (CurrentWorkspace == null)
                return;

            // Prompt for group name using simple input dialog
            var inputDialog = new System.Windows.Window
            {
                Title = "Add New Group",
                Width = 350,
                Height = 160,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                ResizeMode = System.Windows.ResizeMode.NoResize,
                ShowInTaskbar = false,
                Owner = System.Windows.Application.Current.MainWindow
            };

            var mainGrid = new System.Windows.Controls.Grid();
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) });
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) });
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) });

            var label = new System.Windows.Controls.Label { Content = "Group Name:", Margin = new System.Windows.Thickness(10, 10, 10, 5) };
            System.Windows.Controls.Grid.SetRow(label, 0);

            var textBox = new System.Windows.Controls.TextBox { Margin = new System.Windows.Thickness(10, 0, 10, 10), Padding = new System.Windows.Thickness(5), Height = 30 };
            System.Windows.Controls.Grid.SetRow(textBox, 1);
            textBox.Focus();

            var buttonPanel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right, Margin = new System.Windows.Thickness(10) };
            var okButton = new System.Windows.Controls.Button { Content = "OK", Width = 80, Padding = new System.Windows.Thickness(5), Margin = new System.Windows.Thickness(5, 0, 5, 0) };
            var cancelButton = new System.Windows.Controls.Button { Content = "Cancel", Width = 80, Padding = new System.Windows.Thickness(5), Margin = new System.Windows.Thickness(5, 0, 0, 0) };

            okButton.Click += (s, e) => inputDialog.DialogResult = true;
            cancelButton.Click += (s, e) => { inputDialog.DialogResult = false; inputDialog.Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            System.Windows.Controls.Grid.SetRow(buttonPanel, 2);

            mainGrid.Children.Add(label);
            mainGrid.Children.Add(textBox);
            mainGrid.Children.Add(buttonPanel);

            inputDialog.Content = mainGrid;

            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                var groupName = textBox.Text.Trim();

                // Check if group already exists
                if (CurrentWorkspace.Groups.Any(g => g.Name == groupName))
                {
                    MessageBox.Show("A group with this name already exists.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create new group
                var newGroup = new Group(groupName);
                CurrentWorkspace.Groups.Add(newGroup);
                AvailableGroups.Add(newGroup);

                StatusMessage = $"Group '{groupName}' created successfully";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to add group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async void ExecuteImportImage()
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Title = "Import Image",
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                StatusMessage = "Importing image...";

                // Ask for preset name
                string presetName = Path.GetFileNameWithoutExtension(dialog.FileName);

                // Create preset from image
                var preset = await _imageImportService.CreatePresetFromImageAsync(
                    dialog.FileName,
                    presetName,
                    threshold: 128);

                // Ask where to save
                var saveDialog = new SaveFileDialog
                {
                    Title = "Save Preset",
                    Filter = "Preset Files (*.preset.json)|*.preset.json|All Files (*.*)|*.*",
                    FileName = $"{presetName}.preset.json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var presetRepo = new MapEditor.Infrastructure.Repositories.PresetFileRepository();
                    await presetRepo.SaveAsync(preset, saveDialog.FileName);
                    MessageBox.Show($"Preset created successfully!\nSize: {preset.Size.Width}x{preset.Size.Height}\nSquares: {preset.Squares.Count}",
                        "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusMessage = $"Preset '{presetName}' created from image";
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to import image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void ExecuteShowGridFeatures()
    {
        try
        {
            if (CurrentWorkspace == null)
                return;

            var grid = CurrentWorkspace.Grid;
            var sb = new StringBuilder();
            sb.AppendLine("Grid Features Summary");
            sb.AppendLine(new string('=', 60));
            sb.AppendLine();

            // Grid Dimensions
            sb.AppendLine("Grid Dimensions:");
            sb.AppendLine(new string('-', 60));
            sb.AppendLine($"Width:  {grid.Size.Width,6} cells");
            sb.AppendLine($"Height: {grid.Size.Height,6} cells");
            sb.AppendLine($"Total:  {grid.Size.Width * grid.Size.Height,6} cells");
            sb.AppendLine();

            // Square Statistics
            var allCells = grid.GetAllCells().ToList();
            var filledCells = allCells.Where(c => !c.IsEmpty).ToList();
            var emptyCells = allCells.Count - filledCells.Count;

            sb.AppendLine("Cell Statistics:");
            sb.AppendLine(new string('-', 60));
            sb.AppendLine($"Filled Cells:    {filledCells.Count,6} ({(double)filledCells.Count / allCells.Count * 100:F1}%)");
            sb.AppendLine($"Empty Cells:     {emptyCells,6} ({(double)emptyCells / allCells.Count * 100:F1}%)");
            sb.AppendLine();

            // Square Type Distribution
            if (filledCells.Any())
            {
                sb.AppendLine("Square Type Distribution:");
                sb.AppendLine(new string('-', 60));
                var squareGroups = filledCells
                    .GroupBy(c => c.Square!.Type)
                    .OrderByDescending(g => g.Count());

                foreach (var group in squareGroups)
                {
                    var percentage = (double)group.Count() / filledCells.Count * 100;
                    sb.AppendLine($"{group.Key,-15} {group.Count(),6} ({percentage:F1}%)");
                }
                sb.AppendLine();
            }

            // Entity Statistics
            var entities = CurrentWorkspace.Entities.ToList();
            if (entities.Any())
            {
                sb.AppendLine("Entity Statistics:");
                sb.AppendLine(new string('-', 60));
                sb.AppendLine($"Total Entities:  {entities.Count,6}");

                var entityGroups = entities
                    .GroupBy(e => e.Type)
                    .OrderByDescending(g => g.Count());

                foreach (var group in entityGroups)
                {
                    sb.AppendLine($"{group.Key,-15} {group.Count(),6}");
                }
                sb.AppendLine();
            }

            // Branch Detection Features (if available)
            if (Endpoints != null || Bifurcations != null || Crossings != null)
            {
                sb.AppendLine("Structural Features (Branch Detection):");
                sb.AppendLine(new string('-', 60));
                if (Endpoints != null)
                    sb.AppendLine($"Endpoints:       {Endpoints.Count,6}");
                if (Bifurcations != null)
                    sb.AppendLine($"Bifurcations:    {Bifurcations.Count,6}");
                if (Crossings != null)
                    sb.AppendLine($"Crossings:       {Crossings.Count,6}");

                var totalBranch = (Endpoints?.Count ?? 0) + (Bifurcations?.Count ?? 0) + (Crossings?.Count ?? 0);
                if (totalBranch > 0)
                {
                    sb.AppendLine($"Total Branch Points: {totalBranch,6}");
                }
                sb.AppendLine();
            }

            // Skeleton Statistics (if available)
            if (SkeletonMatrix != null)
            {
                int skeletonPixels = 0;
                int rows = SkeletonMatrix.GetLength(0);
                int cols = SkeletonMatrix.GetLength(1);

                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        if (SkeletonMatrix[y, x] == 1)
                            skeletonPixels++;
                    }
                }

                sb.AppendLine("Skeleton Statistics:");
                sb.AppendLine(new string('-', 60));
                sb.AppendLine($"Skeleton Pixels: {skeletonPixels,6}");
                sb.AppendLine($"Coverage:        {(double)skeletonPixels / (rows * cols) * 100:F2}%");
                sb.AppendLine();
            }

            // Group Information
            sb.AppendLine("Active Group:");
            sb.AppendLine(new string('-', 60));
            sb.AppendLine($"Name: {CurrentWorkspace.ActiveGroup.Name}");
            sb.AppendLine();

            MessageBox.Show(sb.ToString(), "Grid Features", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = "Grid features displayed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to display grid features: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
