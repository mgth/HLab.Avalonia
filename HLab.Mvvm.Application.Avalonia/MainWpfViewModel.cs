using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using HLab.Erp.Acl;
using HLab.Mvvm.Annotations;
using HLab.Mvvm.Application.Documents;
using HLab.Mvvm.ReactiveUI;
using ReactiveUI;

namespace HLab.Mvvm.Application.Avalonia;

public class MainAvaloniaViewModelDesign : MainAvaloniaViewModel
{
    public MainAvaloniaViewModelDesign() 
        : base(null, null, null, null, null, null )
    {
            
    }
}

public class MainAvaloniaViewModel : ViewModel
{
    public IAclService Acl {get; }
    readonly IDocumentService _doc;
    public IApplicationInfoService ApplicationInfo { get; }
    public ILocalizationService LocalizationService { get; }
    public IIconService IconService { get; }

    public MainAvaloniaViewModel(
        IAclService acl, 
        IDocumentService doc, 
        IDocumentPresenter presenter, 
        IApplicationInfoService applicationInfo, 
        ILocalizationService localizationService, 
        IIconService iconService)
    {
        Acl = acl;
        DocumentPresenter = presenter;
        doc.MainPresenter = presenter;
        _doc = doc;
        ApplicationInfo = applicationInfo;
        LocalizationService = localizationService;
        IconService = iconService;

        _title = this
            .WhenAnyValue(e => e.ApplicationInfo.Name)
            .ToProperty(this, e => e.Title);

        OpenUserCommand = ReactiveCommand.Create(OpenUser);
        ExitCommand = ReactiveCommand.Create(Exit);
    }

    public IDocumentPresenter DocumentPresenter { get; }

    public bool IsActive
    {
        get => _isActive;
        set => SetAndRaise(ref _isActive, value);
    }
    bool _isActive = true;




    // TODO
    //public Canvas DragCanvas => _dragCanvas.Get();
    //private Canvas _dragCanvas = H.Property<Canvas>( c => c
    //    .Set( e => {
    //            var canvas = new Canvas();
    //            e._dragDrop.RegisterDragCanvas(canvas);
    //            return canvas;
    //        }
    //    )
    //);

    public Menu Menu { get; } = new Menu {/*IsMainMenu = true, */Background=Brushes.Transparent}; 

    public string Title => _title.Value;
    readonly ObservableAsPropertyHelper<string> _title;

    public ICommand ExitCommand  { get; }

    void Exit() => Dispatcher.UIThread.BeginInvokeShutdown(DispatcherPriority.Normal);

    public ICommand OpenUserCommand { get; }

    public void OpenUser() => _doc.OpenDocumentAsync(Acl.Connection.User);


}