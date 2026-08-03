using Avalonia;
using Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

namespace HLab.Mvvm.Application.Avalonia;

/// <summary>
/// Dockable hébergeant une vue déjà construite : le présentateur de documents
/// manipule des VUES (créées par le ViewLocator), pas des ViewModels à résoudre —
/// contrairement au modèle nominal de Dock. Le Context expose le DataContext de
/// la vue pour le template d'entête (icône/titre/sous-titre).
/// </summary>
public class ViewDocument : Document
{
    public ViewDocument(Control view)
    {
        View = view;
        Context = view.DataContext;
        CanClose = true;
        CanFloat = true;

        // Titre brut pour les fenêtres flottantes ; l'entête d'onglet passe par
        // le HeaderTemplate (localisé), pas par Title.
        Title = view.DataContext?.GetType().GetProperty("Header")?.GetValue(view.DataContext)?.ToString()
                ?? view.GetType().Name;
    }

    public Control View { get; }
}

/// <summary>
/// Hôte du contenu d'un ViewDocument. Dock ré-instancie le template de contenu
/// à chaque activation d'onglet, alors que la vue est UNE instance unique :
/// sans re-parentage explicite, la deuxième activation crashe (« control
/// already has a visual parent » — même famille que le bug AvalonDock WPF).
/// L'hôte vole la vue à son hôte précédent en s'attachant, et la libère en se
/// détachant (l'ordre attach/détach des deux hôtes n'est pas garanti).
/// </summary>
public class ViewDocumentHost : ContentControl
{
    public ViewDocumentHost()
    {
        // Le DataContext peut arriver AVANT ou APRÈS l'attachement selon le
        // chemin de Dock (onglet, fenêtre flottante) : traiter les deux, sinon
        // l'hôte reste vide et l'onglet s'affiche sans contenu.
        DataContextChanged += (_, _) => TakeView();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        TakeView();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Libérer la vue : elle est unique et va être reprise par un autre hôte.
        if (ReferenceEquals(Content, (DataContext as ViewDocument)?.View)) Content = null;
        base.OnDetachedFromVisualTree(e);
    }

    void TakeView()
    {
        if (DataContext is not ViewDocument { View: { } view }) return;
        if (ReferenceEquals(Content, view)) return;

        // Voler la vue à son hôte précédent (l'ordre attach/détach des deux
        // hôtes n'est pas garanti par Dock).
        if (view.Parent is ContentControl previous && !ReferenceEquals(previous, this))
            previous.Content = null;

        Content = view;
    }
}

/// <summary>
/// Layout minimal : un RootDock contenant un seul DocumentDock. Les documents
/// sont ajoutés/retirés au fil de l'eau par l'adaptateur du DocumentPresenterView.
/// </summary>
public class DocumentPresenterDockFactory : Factory
{
    public IDocumentDock? DocumentDock { get; private set; }

    public override IRootDock CreateLayout()
    {
        var documentDock = new DocumentDock
        {
            Id = "Documents",
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(),
            CanCreateDocument = false,
            EnableWindowDrag = true,
        };

        var root = CreateRootDock();
        root.Id = "Root";
        root.IsCollapsable = false;
        root.VisibleDockables = CreateList<IDockable>(documentDock);
        root.ActiveDockable = documentDock;
        root.DefaultDockable = documentDock;

        DocumentDock = documentDock;
        return root;
    }
}
