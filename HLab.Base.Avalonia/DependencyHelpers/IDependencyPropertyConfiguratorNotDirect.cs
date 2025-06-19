using Avalonia;
using Avalonia.Data;

namespace HLab.Base.Avalonia.DependencyHelpers;

public interface IDependencyPropertyConfiguratorNotDirect<TClass, TValue>
   where TClass : AvaloniaObject
{
   StyledProperty<TValue> Register();
   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> Inherits { get; }

   IDependencyPropertyConfiguratorAttached<TClass, TValue> Attached { get; }

   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> BindModeDefault(BindingMode mode = BindingMode.Default);
   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> Default(TValue value);

   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> OnChanged(Action<TClass, AvaloniaPropertyChangedEventArgs<TValue>> action);

}