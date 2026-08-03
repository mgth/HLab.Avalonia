using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using HLab.Mvvm.Annotations;
using HLab.Options;

namespace HLab.Mvvm.Application.Avalonia;

/// <summary>
/// Adaptateur entre le DocumentPresenterViewModel (collections de VUES,
/// contrat hérité d'AvalonDock) et Dock.Avalonia (dockables). Le présentateur
/// reste la source de vérité : ouverture/fermeture par le DocumentService,
/// dédoublonnage par modèle... Dock n'est que la couche de présentation.
/// </summary>
public partial class DocumentPresenterView : UserControl, IView<DocumentPresenterViewModel>
{
    readonly IOptionsService _options;

    DocumentPresenterDockFactory? _factory;
    DocumentPresenterViewModel? _presenter;
    readonly List<ViewDocument> _documents = new();
    bool _syncing;

    public DocumentPresenterView(IOptionsService options)
    {
        _options = options;
        InitializeComponent();

        DataContextChanged += (_, _) => AttachPresenter();
    }

    void AttachPresenter()
    {
        DetachPresenter();

        if (DataContext is not DocumentPresenterViewModel presenter) return;
        _presenter = presenter;

        _factory = new DocumentPresenterDockFactory();
        var layout = _factory.CreateLayout();
        _factory.InitLayout(layout);
        PART_Dock.Layout = layout;

        foreach (var view in presenter.Documents.OfType<Control>())
            AddDockable(view);

        if (presenter.ActiveDocument is Control active) Activate(active);

        presenter.Documents.CollectionChanged += OnDocumentsChanged;
        presenter.PropertyChanged += OnPresenterPropertyChanged;
        _factory.DockableClosed += OnDockableClosed;
        _factory.FocusedDockableChanged += OnFocusedDockableChanged;

        // Source de vérité de « l'onglet existe » : la collection du dock. Les
        // événements du factory ne couvrent pas tous les chemins de fermeture.
        if (_factory.DocumentDock?.VisibleDockables is INotifyCollectionChanged dockables)
        {
            _dockables = dockables;
            _dockables.CollectionChanged += OnDockablesChanged;
        }
    }

    INotifyCollectionChanged? _dockables;

    void DetachPresenter()
    {
        if (_presenter is not null)
        {
            _presenter.Documents.CollectionChanged -= OnDocumentsChanged;
            _presenter.PropertyChanged -= OnPresenterPropertyChanged;
        }

        if (_factory is not null)
        {
            _factory.DockableClosed -= OnDockableClosed;
            _factory.FocusedDockableChanged -= OnFocusedDockableChanged;
        }

        if (_dockables is not null)
        {
            _dockables.CollectionChanged -= OnDockablesChanged;
            _dockables = null;
        }

        _documents.Clear();
        _presenter = null;
        _factory = null;
    }

    ViewDocument? FindDocument(object? view)
        => _documents.FirstOrDefault(d => ReferenceEquals(d.View, view));

    void AddDockable(Control view)
    {
        if (_factory?.DocumentDock is not { } dock) return;
        if (FindDocument(view) is not null) { Activate(view); return; }

        var document = new ViewDocument(view);
        _documents.Add(document);
        _factory.AddDockable(dock, document);
        Focus(document);

        Log($"add '{document.Title}'");
    }

    void Activate(Control view)
    {
        if (_factory?.DocumentDock is not { } dock) return;

        if (FindDocument(view) is not { } document)
        {
            AddDockable(view);
            return;
        }

        // Le dockable a pu quitter le dock sans passer par nous : le remettre,
        // sinon le document reste dans le présentateur mais n'est plus ouvrable.
        if (!IsLive(document))
        {
            _factory.AddDockable(dock, document);
            Log($"re-add '{document.Title}' (orphelin)");
        }

        Focus(document);
    }

    /// <summary>
    /// Le dockable est-il réellement présent dans un dock (onglet ou fenêtre
    /// flottante) ? Test positif : après certaines fermetures, Owner reste
    /// renseigné alors que le dockable n'est plus visible nulle part.
    /// </summary>
    static bool IsLive(ViewDocument document)
        => document.Owner is IDock owner && owner.VisibleDockables?.Contains(document) == true;

    /// <summary>
    /// Activation SANS prise de focus clavier : les fiches embarquent leur
    /// propre dock (tests d'un échantillon), un SetFocusedDockable y déplaçait
    /// le focus et le premier clic sur la bande d'onglets extérieure était
    /// consommé à le ramener (onglet qui ne réagit qu'au deuxième clic).
    /// </summary>
    void Focus(ViewDocument document) => _factory?.SetActiveDockable(document);

    static void Log(string message) => Console.Error.WriteLine($"[Dock] {message}");

    void OnDocumentsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_syncing) return;
        _syncing = true;
        try
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var document in _documents.ToList())
                    _factory?.CloseDockable(document);
                _documents.Clear();
                return;
            }

            if (e.NewItems is not null)
                foreach (var view in e.NewItems.OfType<Control>())
                    AddDockable(view);

            if (e.OldItems is not null)
                foreach (var view in e.OldItems.OfType<Control>())
                {
                    if (FindDocument(view) is not { } document) continue;
                    _documents.Remove(document);
                    _factory?.CloseDockable(document);
                }
        }
        finally { _syncing = false; }
    }

    void OnPresenterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_syncing) return;
        if (e.PropertyName != nameof(DocumentPresenterViewModel.ActiveDocument)) return;
        if (_presenter?.ActiveDocument is not Control view) return;

        _syncing = true;
        try { Activate(view); }
        finally { _syncing = false; }
    }

    void OnDockableClosed(object? sender, DockableClosedEventArgs e)
    {
        if (e.Dockable is not ViewDocument document) return;

        Log($"closed '{document.Title}'");
        Forget(document);
    }

    /// <summary>
    /// L'onglet a disparu du dock : quel que soit le chemin (croix, commande,
    /// fermeture programmée), le présentateur doit oublier le document, sinon
    /// sa réouverture est avalée par le dédoublonnage par modèle.
    /// </summary>
    void OnDockablesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is null) return;

        foreach (var document in e.OldItems.OfType<ViewDocument>())
        {
            // Un déplacement (fenêtre flottante, autre dock) n'est pas une
            // fermeture : le dockable est alors toujours vivant ailleurs.
            if (IsLive(document)) continue;

            Log($"removed '{document.Title}'");
            Forget(document);
        }
    }

    void Forget(ViewDocument document)
    {
        _documents.Remove(document);

        if (_syncing || _presenter is null) return;
        _syncing = true;
        try
        {
            _presenter.RemoveDocument(document.View);
        }
        finally { _syncing = false; }
    }

    void OnFocusedDockableChanged(object? sender, FocusedDockableChangedEventArgs e)
    {
        if (_syncing || _presenter is null) return;
        if (e.Dockable is not ViewDocument document) return;

        _syncing = true;
        try { _presenter.ActiveDocument = document.View; }
        finally { _syncing = false; }
    }
}
