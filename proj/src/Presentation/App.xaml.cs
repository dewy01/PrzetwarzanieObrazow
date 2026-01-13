using System.IO;
using System.Text.Json;
using System.Windows;
using MapEditor.Application.Services;
using MapEditor.Domain.Biometric.Services;
using MapEditor.Domain.Editing.Services;
using MapEditor.Infrastructure.Algorithms;
using MapEditor.Infrastructure.Repositories;
using MapEditor.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MapEditor.Presentation;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure Dependency Injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Create main window and set DataContext
        var mainWindow = new MainWindow();
        var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = viewModel;
        mainWindow.Show();

        // Auto-load default workspace if configured
        TryAutoLoadDefaultWorkspace(viewModel);
    }

    private void ConfigureServices(ServiceCollection services)
    {
        // Domain Services
        services.AddSingleton<IWorkspaceRepository, WorkspaceFileRepository>();
        services.AddSingleton<IPresetRepository, PresetFileRepository>();
        services.AddSingleton<IUL22Converter, UL22Converter>();
        services.AddSingleton<IPreprocessingService, PreprocessingService>();
        services.AddSingleton<IFragmentationService, FragmentationService>();
        services.AddSingleton<ISkeletonizationService, SkeletonizationService>();
        services.AddSingleton<IBranchDetectionService, BranchDetectionService>();

        // Application Services
        services.AddSingleton<UndoRedoService>();
        services.AddSingleton<EditingService>();
        services.AddSingleton<IImageImportService, MapEditor.Infrastructure.Services.ImageImportService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private sealed class AppSettings
    {
        public bool AutoLoadWorkspace { get; set; }
        public string? DefaultWorkspacePath { get; set; }
    }

    private void TryAutoLoadDefaultWorkspace(MainViewModel viewModel)
    {
        try
        {
            var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(settingsPath))
                return;

            var json = File.ReadAllText(settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            if (settings == null)
                return;

            if (settings.AutoLoadWorkspace && !string.IsNullOrWhiteSpace(settings.DefaultWorkspacePath))
            {
                var fullPath = settings.DefaultWorkspacePath!;
                if (!Path.IsPathRooted(fullPath))
                    fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, fullPath));

                _ = viewModel.LoadWorkspaceFromPathAsync(fullPath);
            }
        }
        catch
        {
            // Swallow config errors to avoid blocking startup
        }
    }
}

