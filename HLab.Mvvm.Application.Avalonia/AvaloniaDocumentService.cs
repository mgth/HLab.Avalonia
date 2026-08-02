using Avalonia.Controls;
using HLab.Core.Annotations;
using HLab.Mvvm.Annotations;
using HLab.Mvvm.Application.Documents;
using HLab.Mvvm.Application.Messages;

namespace HLab.Mvvm.Application.Avalonia;

public class AvaloniaDocumentService(
   IMvvmService mvvm,
   Func<Type, object> getter,
   IMessagesService messageBus,
   Func<object, ISelectedMessage> getMessage)
   : DocumentService(mvvm, getter)
{
    public IMessagesService MessageBus { get; } = messageBus;
    Func<object, ISelectedMessage> GetMessage { get; } = getMessage;

    object GetModel(object view)
    {
        var o = view;
        while (true)
        {
            var linked = o switch
            {
                Control c => c.DataContext,
                IViewModel vm => vm.Model,
                _ => null
            };

            if (linked is null) return o;
            o = linked;
        }
    }

    public override async Task OpenDocumentAsync(IView view, IDocumentPresenter presenter)
    {
        // Chercher un document existant pour le même modèle AVANT d'ajouter : les
        // vues ne sont pas cachées (IView NotCacheable), chaque GetView en crée
        // une nouvelle — ajouter d'abord laissait un doublon dans le présenteur.
        var model = GetModel(view);

        if (view is IAnchorableViewClass)
        {
            if (presenter.Anchorables.Contains(view)) return;

            foreach (var anchorable in presenter.Anchorables.ToList())
            {
                if (ReferenceEquals(model, GetModel(anchorable)))
                    return;
            }

            presenter.Anchorables.Add(view);
            return;
        }

        foreach (var document in presenter.Documents.ToList())
        {
            if (!ReferenceEquals(model, GetModel(document))) continue;

            presenter.ActiveDocument = document;
            return;
        }

        presenter.Documents.Add(view);
        MessageBus.Publish(GetMessage(view));
        presenter.ActiveDocument = view as Control;
    }


    public override async Task CloseDocumentAsync(object content, IDocumentPresenter presenter)
    {
        if (content is IView view)
        {
            if (presenter.Documents.Contains(view))
            {
                presenter.RemoveDocument((Control)view);
                return;
            }

            if (presenter.Anchorables.Contains(view))
            {
                presenter.Anchorables.Remove(view);
                return;
            }
        }

        var documents = presenter.Documents.OfType<Control>().ToList();
        foreach (var document in documents)
        {
            if (ReferenceEquals(document.DataContext, content))
            {
                presenter.RemoveDocument(document);
            }

            else if (document.DataContext is IViewModel mvm && ReferenceEquals(mvm.Model, content))
            {
                presenter.RemoveDocument(document);
            }
        }

        var anchorables = presenter.Anchorables.OfType<Control>().ToList();
        foreach (var anchorable in anchorables)
        {
            if (ReferenceEquals(anchorable.DataContext, content))
            {
                presenter.Anchorables.Remove(anchorable);
            }
            else if (anchorable.DataContext is IViewModel mvm && ReferenceEquals(mvm.Model, content))
            {
                presenter.Anchorables.Remove(anchorable);
            }
        }

    }
}