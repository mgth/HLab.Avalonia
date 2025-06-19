using Avalonia;

namespace HLab.Base.Avalonia.DependencyHelpers;

public interface IDependencyPropertyConfiguratorAttached<out TClass, TValue>
   where TClass : AvaloniaObject
{
   AttachedProperty<TValue> Register();
   IDependencyPropertyConfiguratorAttached<TClass, TValue> BindsTwoWayByDefault { get; }
   IDependencyPropertyConfiguratorAttached<TClass, TValue> Default(TValue value);

   //IDependencyPropertyConfiguratorAttached<TClass, TValue> OnChange<TSender>(Action<TSender, bool> action)            
   //    where TSender : AvaloniaObject;

   IDependencyPropertyConfiguratorAttached<TClass, TValue> OnChanged(Action<TClass, AvaloniaPropertyChangedEventArgs<TValue>> action);
   //IDependencyPropertyConfiguratorAttached<TClass, TValue> OnChangeBeforeNotification(Action<TClass> action);
   //IDependencyPropertyConfiguratorAttached<TClass, TValue> OnChangeAfterNotification(Action<TClass> action);
}