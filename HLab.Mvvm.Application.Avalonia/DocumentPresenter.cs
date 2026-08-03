using System.Collections.ObjectModel;
using System.Collections.Specialized;
using HLab.Base.ReactiveUI;
using HLab.Core.Annotations;
using HLab.Mvvm.Application.Documents;
using HLab.Mvvm.Application.Messages;
using HLab.Mvvm.ReactiveUI;
using ReactiveUI;

namespace HLab.Mvvm.Application.Avalonia
{
    public class DocumentPresenterViewModel : ViewModel, IDocumentPresenter
    {
        readonly IMessagesService _message;
        readonly Func<object, ISelectedMessage> _getSelectedMessage;

        public DocumentPresenterViewModel
        (
            IMessagesService message,             
            Func<object, ISelectedMessage> getSelectedMessage 
        )
        {
            _message = message;
            _getSelectedMessage = getSelectedMessage;

            Documents.CollectionChanged += OnDocumentsChanged;
        }

        /// <summary>
        /// Les vues peuvent être retirées directement de la collection, sans
        /// passer par RemoveDocument (le docking le fait à la fermeture d'un
        /// onglet) : purger l'historique et réélire l'actif, sinon un document
        /// fantôme bloque sa propre réouverture (dédoublonnage par modèle).
        /// </summary>
        void OnDocumentsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Move) return;

            _documentHistory.RemoveAll(d => !Documents.Contains(d));

            if (ActiveDocument is null || Documents.Contains(ActiveDocument)) return;

            ActiveDocument = _documentHistory.FirstOrDefault();
        }

        public ObservableCollection<object> Documents { get; } = new();
        public ObservableCollection<object> Anchorables { get; } = new();

        readonly List<object> _documentHistory = new();

        public object ActiveDocument
        {
            get;
            set
            {
                if (value is not null)
                {
                    _documentHistory.Remove(value);
                    _documentHistory.Insert(0, value);
                }

                // Notifier même sans changement de valeur : l'affectation vaut
                // « activer ce document » (réouverture d'un document déjà ouvert
                // mais dont l'onglet a disparu). Le message n'est publié, lui,
                // que sur un vrai changement.
                var changed = !ReferenceEquals(field, value);
                field = value;
                this.RaisePropertyChanged();

                if (changed && value is not null) _message.Publish(_getSelectedMessage(value));
            }
        }

      public object? Theme { get; set => this.SetAndRaise(ref field, value); }

      /// <summary>
        /// Retire un document, actif ou non. La version précédente refusait tout
        /// document qui n'était pas en tête d'historique : la vue restait alors
        /// dans Documents alors que son onglet était fermé, et sa réouverture
        /// était avalée par le dédoublonnage par modèle (Avalonia : plus rien ne
        /// s'ouvre ; WPF : AvalonDock ré-ajoutait une vue encore présente dans
        /// son layout, d'où le crash).
        /// </summary>
        public bool RemoveDocument(object document)
        {
            if (!Documents.Contains(document)) return false;

            var wasActive = ReferenceEquals(ActiveDocument, document);

            _documentHistory.Remove(document);
            Documents.Remove(document);

            if (wasActive)
            {
                // L'historique peut contenir des documents déjà fermés.
                ActiveDocument = _documentHistory.FirstOrDefault(Documents.Contains);
            }

            return true;
        }

    }
}
