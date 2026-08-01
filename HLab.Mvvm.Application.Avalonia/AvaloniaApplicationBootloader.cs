using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HLab.Core.Annotations;
using HLab.Mvvm.Annotations;
using HLab.Mvvm.Avalonia;

namespace HLab.Mvvm.Application.Avalonia;

public class AvaloniaApplicationBootloader(ApplicationBootloader.Injector injector) : ApplicationBootloader(injector)
{
    public override async Task<BootState> LoadAsync()
    {
        await base.LoadAsync();

        var view = injector.Mvvm.MainContext.GetView(ViewModel, MainViewMode, typeof(IDefaultViewClass));
        var window = view?.AsWindow();

        if (window is not null
            && global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Déclarer la fenêtre principale à la lifetime : l'app suit désormais
            // sa fermeture (et survit à la fermeture de la fenêtre de login).
            desktop.MainWindow = window;
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        window?.Show();

        return BootState.Completed;
    }
}
