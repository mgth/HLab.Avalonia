using Avalonia;
using Avalonia.Data;

namespace HLab.Base.Avalonia.DependencyHelpers;

public interface IDependencyPropertyConfigurator<TClass, TValue>
   where TClass : AvaloniaObject
{
   StyledProperty<TValue> Register();
   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> Inherits { get; }

   IDependencyPropertyConfiguratorAttached<TClass, TValue> Attached { get; }


   IDependencyPropertyConfigurator<TClass, TValue> BindModeDefault(BindingMode mode = BindingMode.TwoWay);
   IDependencyPropertyConfigurator<TClass, TValue> Default(TValue value);
   IDependencyPropertyConfigurator<TClass, TValue> Coerce(Func<AvaloniaObject, TValue, TValue> func);
   IDependencyPropertyConfigurator<TClass, TValue> Coerce(Func<TValue, TValue> func);
   IDependencyPropertyConfigurator<TClass, TValue> Validate(Func<TValue, bool> func);
   IDependencyPropertyConfigurator<TClass, TValue> OnChanged(Action<TClass, AvaloniaPropertyChangedEventArgs<TValue>> action);

   IDependencyPropertyConfiguratorDirect<TClass, TValue> Getter(Func<TClass, TValue> getter);
}