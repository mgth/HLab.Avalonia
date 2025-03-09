using Avalonia;

namespace HLab.Base.Avalonia.DependencyHelpers;

public interface IDependencyPropertyConfiguratorDirect<TClass, TValue>
   where TClass : AvaloniaObject
{
   DirectProperty<TClass, TValue> Register();
   IDependencyPropertyConfiguratorDirect<TClass, TValue> BindsTwoWayByDefault { get; }
   IDependencyPropertyConfiguratorDirect<TClass, TValue> Default(TValue value);
   IDependencyPropertyConfiguratorDirect<TClass, TValue> Getter(Func<TClass, TValue> getter);
   IDependencyPropertyConfiguratorDirect<TClass, TValue> Setter(Action<TClass, TValue> setter);
   IDependencyPropertyConfiguratorDirect<TClass, TValue> OnChanged(Action<TClass, AvaloniaPropertyChangedEventArgs<TValue>> action);
}