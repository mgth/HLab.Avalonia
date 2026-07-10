using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using HLab.Core.Annotations;
using HLab.Mvvm.Annotations;
using ReactiveUI;

namespace HLab.Mvvm.Avalonia;

public class MvvmAvaloniaImpl : IMvvmPlatformImpl
{
   public class Bootloader(IMvvmService mvvm) : Core.Annotations.Bootloader
   {
      protected override BootState Load()
      {
         mvvm.RegisterPlatform<MvvmAvaloniaImpl>();
         return base.Load();
      }
   }

   readonly ResourceDictionary _dictionary = new();

   public void Register(IMvvmService mvvm)
   {
      Application.Current.Resources.MergedDictionaries.Add(_dictionary);
      mvvm.ViewHelperFactory.Register<IView>(v => new ViewHelperAvalonia((StyledElement)v));
   }

   public Task PrepareViewAsync(IView view, CancellationToken token)
   {
      if (view is not AvaloniaObject obj) throw new InvalidCastException("IView objects should be AvaloniaObject in Avalonia implementation");

      if (Dispatcher.UIThread.CheckAccess())
      {
         Prepare();
         return Task.CompletedTask;
      }

      return Dispatcher.UIThread.InvokeAsync(Prepare, DispatcherPriority.Default, token).GetTask();

      void Prepare()
      {
         ViewLocator.SetViewClass(obj, typeof(IDefaultViewClass));
         ViewLocator.SetViewMode(obj, typeof(DefaultViewMode));
         LinkDispose(view);
      }

      // View models are not tracked by the DI container (ExternallyOwned),
      // ownership follows the view tree : dispose them when the view leaves it.
      static void LinkDispose(IView v)
      {
         if (v is not StyledElement element) return;
         element.DetachedFromLogicalTree += (a, o) =>
         {
            if (element.DataContext is IDisposable vm)
            {
               vm.Dispose();
            }
         };
      }
   }

   public void Register(Type t)
   {
      if (t.IsInterface) return;

      var template = new FuncDataTemplate(t, (value, namescope) =>
          new ViewLocator());

      _dictionary.Add(t, template);
   }

   public async Task<IView> GetNotFoundViewAsync(Type viewModelType, Type viewMode, Type viewClass, CancellationToken token = default)
   {
      return await Dispatcher.UIThread.InvokeAsync(() => new NotFoundView
      {
         Title = { Text = "View not found" },
         Message = { Text = (viewModelType?.ToString() ?? "??")
                                      + "\n" + (viewMode?.FullName ?? "??")
                                      + "\n" + (viewClass?.FullName ?? "??") }
      }
          , DispatcherPriority.Normal
          , token
      );
   }

   public object Activate(IView obj)
   {
      if (obj is IActivatableViewModel a) a.Activator.Activate();

      return obj;
   }

   public object Deactivate(IView obj)
   {
      if (obj is IActivatableViewModel a) a.Activator.Deactivate();

      return obj;
   }

   public IWindow ViewAsWindow<T>(IView? view) where T : IWindow, new()
   {
      switch (view)
      {
         case IWindow win:
            return win;
         case Control c:
            {
               var w = new T();
               if (w is Window window)
               {
                  window.DataContext = c.DataContext;
                  window.Content = view;
               }

               return w;
            }
         default:
            throw new ArgumentException("view should be FrameworkElement");
      }
   }

   public IWindow ViewAsWindow(IView? view)
   {
      var w = new DefaultWindow()
      {
         DataContext = (view as Control)?.DataContext,
         View = view,
      };

      return w;
   }
}
