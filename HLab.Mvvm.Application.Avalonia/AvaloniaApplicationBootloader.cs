using HLab.Core.Annotations;
using HLab.Mvvm.Annotations;
using HLab.Mvvm.Avalonia;
using ReactiveUI;

namespace HLab.Mvvm.Application.Avalonia;

public class AvaloniaApplicationBootloader(ApplicationBootloader.Injector injector) : ApplicationBootloader(injector)
{
    // TODO : should not be needed anymore
    //public Window MainWindow { get; protected set; }

    public override async Task LoadAsync(IBootContext b)
    {
        await base.LoadAsync(b);

        var view = await injector.Mvvm.MainContext.GetViewAsync(ViewModel,MainViewMode,typeof(IDefaultViewClass));
        var window = view?.AsWindow();
        // TODO 
        //MainWindow.Closing += (sender, args) => System.Windows.Application.Current.Shutdown();
        window?.Show();
    }
}