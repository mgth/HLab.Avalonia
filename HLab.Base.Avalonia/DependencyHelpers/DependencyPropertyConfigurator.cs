using System.Diagnostics;
using Avalonia;
using Avalonia.Data;

namespace HLab.Base.Avalonia.DependencyHelpers;
public class DependencyPropertyConfigurator<TClass, TValue>(string name) :
   IDependencyPropertyConfigurator<TClass, TValue>,
   IDependencyPropertyConfiguratorDirect<TClass, TValue>,
   IDependencyPropertyConfiguratorNotDirect<TClass, TValue>,
   IDependencyPropertyConfiguratorAttached<TClass, TValue>
   where TClass : AvaloniaObject
{
   TValue? _defaultValue;
   BindingMode _bindingMode;
   Func<TValue, bool>? _validate;
   Func<AvaloniaObject, TValue, TValue>? _coerce;
   bool _inherits = false;
   Func<TClass, TValue>? _getter;
   Action<TClass, TValue>? _setter;
   bool _enableDataValidation = false;

   Action<AvaloniaProperty<TValue>>? _postAction;


   //readonly FrameworkPropertyMetadata _propertyMetadata = new();

   //ValidateValueCallback _validateValueCallback = null;
   //private readonly TValue _default = default;

   /// <summary>
   ///    Registers a dependency property with the specified property name, property type,
   ///    owner type, property metadata, and a value validation callback for the property.
   /// </summary>
   /// <returns></returns>
   StyledProperty<TValue> IDependencyPropertyConfigurator<TClass, TValue>.Register()
   {
      var property =
          AvaloniaProperty.Register<TClass, TValue>(
              name,
              _defaultValue,
              _inherits,
              _bindingMode,
              _validate,
              _coerce,
              _enableDataValidation
          );

      _postAction?.Invoke(property);

      return property;
   }

   StyledProperty<TValue> IDependencyPropertyConfiguratorNotDirect<TClass, TValue>.Register()
   {
      var property = AvaloniaProperty.Register<TClass, TValue>(
          name,
          _defaultValue,
          _inherits,
          _bindingMode,
          _validate,
          _coerce
      );

      _postAction?.Invoke(property);

      return property;
   }

   /// <summary>
   ///     Registers an attached property with the specified property type, owner type,
   ///     property metadata, and value validation callback for the property.
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <returns></returns>
   AttachedProperty<TValue> RegisterAttached<T>()
   {
      var property = AvaloniaProperty.RegisterAttached<T, TClass, TValue>(
          name,
          _defaultValue,
          _inherits,
          _bindingMode,
          _validate,
          _coerce
      );

      _postAction?.Invoke(property);

      return property;
   }

   DirectProperty<TClass, TValue> IDependencyPropertyConfiguratorDirect<TClass, TValue>.Register()
   {
      Debug.Assert(_getter != null, "Getter must be set");

      var property = AvaloniaProperty.RegisterDirect(
            name,
            _getter,
            _setter,
            _defaultValue,
            _bindingMode,
            _enableDataValidation
        );

      _postAction?.Invoke(property);

      return property;
   }

   /// <summary>
   ///     Registers an attached property with the specified property type, owner type,
   ///     property metadata, and value validation callback for the property.
   /// </summary>
   /// <returns></returns>
   AttachedProperty<TValue> IDependencyPropertyConfiguratorAttached<TClass, TValue>.Register() => RegisterAttached<TClass>();

   DependencyPropertyConfigurator<TClass, TValue> Do(Action action)
   {
      action();
      return this;
   }

   /// <summary>
   ///     The dependency property is inheritable.
   /// </summary>
   public IDependencyPropertyConfiguratorNotDirect<TClass, TValue> Inherits => Do(() => _inherits = true);

   IDependencyPropertyConfiguratorAttached<TClass, TValue> IDependencyPropertyConfiguratorNotDirect<TClass, TValue>.Attached => this;
   IDependencyPropertyConfiguratorAttached<TClass, TValue> IDependencyPropertyConfigurator<TClass, TValue>.Attached => this;


   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> IDependencyPropertyConfiguratorNotDirect<TClass, TValue>
       .BindModeDefault(BindingMode mode) => Do(() => _bindingMode = mode);

   IDependencyPropertyConfiguratorAttached<TClass, TValue> IDependencyPropertyConfiguratorAttached<TClass, TValue>
       .Default(TValue value) => Do(() => _defaultValue = value);

   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> IDependencyPropertyConfiguratorNotDirect<TClass, TValue>
       .Default(TValue value) => Do(() => _defaultValue = value);

   /// <summary>
   ///     The property binds two-way by default.
   /// </summary>
   public IDependencyPropertyConfigurator<TClass, TValue> BindModeDefault(BindingMode mode) => Do(() => _bindingMode = mode);
   IDependencyPropertyConfiguratorAttached<TClass, TValue> IDependencyPropertyConfiguratorAttached<TClass, TValue>.BindsTwoWayByDefault => Do(() => _bindingMode = BindingMode.TwoWay);
   IDependencyPropertyConfiguratorDirect<TClass, TValue> IDependencyPropertyConfiguratorDirect<TClass, TValue>.BindsTwoWayByDefault => Do(() => _bindingMode = BindingMode.TwoWay);


   /*

   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> IDependencyPropertyConfigurator<TClass, TValue>.OnChange<TSender>(Action<TSender, bool> action)
       => Do(() => OnChange(action));
   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> IDependencyPropertyConfiguratorNotDirect<TClass, TValue>.OnChange<TSender>(Action<TSender, bool> action)
       => Do(() => OnChange(action));
   IDependencyPropertyConfiguratorAttached<TClass, TValue> IDependencyPropertyConfiguratorAttached<TClass, TValue>.OnChange<TSender>(Action<TSender, bool> action)
       => Do(() => OnChange(action));

   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> IDependencyPropertyConfigurator<TClass, TValue>.OnChangeBeforeNotification(Action<TClass> action)
       => Do(() => OnChangeBeforeNotification(action));
   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> IDependencyPropertyConfiguratorNotDirect<TClass, TValue>.OnChangeBeforeNotification(Action<TClass> action)
       => Do(() => OnChangeBeforeNotification(action));
   IDependencyPropertyConfiguratorAttached<TClass, TValue> IDependencyPropertyConfiguratorAttached<TClass, TValue>.OnChangeBeforeNotification(Action<TClass> action)
       => Do(() => OnChangeBeforeNotification(action));

   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> IDependencyPropertyConfigurator<TClass, TValue>.OnChangeAfterNotification(Action<TClass> action)
       => Do(() => OnChangeAfterNotification(action));

   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> IDependencyPropertyConfiguratorNotDirect<TClass, TValue>.OnChangeAfterNotification(Action<TClass> action)
       => Do(() => OnChangeAfterNotification(action));
   IDependencyPropertyConfiguratorAttached<TClass, TValue> IDependencyPropertyConfiguratorAttached<TClass, TValue>.OnChangeAfterNotification(Action<TClass> action)
       => Do(() => OnChangeAfterNotification(action));
   /// <summary>
   ///     Represents the callback that is invoked when the effective property value of
   ///     a dependency property changes.
   ///
   /// </summary>
   /// <param name="action"></param>
   /// <returns></returns>
   public IDependencyPropertyConfiguratorNotDirect<TClass, TValue> OnChange(Action<TClass,bool> action)
       => Do(()=>OnChange<TClass>(action));
   */

   IDependencyPropertyConfigurator<TClass, TValue> IDependencyPropertyConfigurator<TClass, TValue>.OnChanged(Action<TClass, AvaloniaPropertyChangedEventArgs<TValue>> action)
       => Do(() => _OnChanged(action));

   IDependencyPropertyConfiguratorNotDirect<TClass, TValue> IDependencyPropertyConfiguratorNotDirect<TClass, TValue>.OnChanged(Action<TClass, AvaloniaPropertyChangedEventArgs<TValue>> action)
       => Do(() => _OnChanged(action));

   IDependencyPropertyConfiguratorDirect<TClass, TValue> IDependencyPropertyConfiguratorDirect<TClass, TValue>.OnChanged(Action<TClass, AvaloniaPropertyChangedEventArgs<TValue>> action)
       => Do(() => _OnChanged(action));

   IDependencyPropertyConfiguratorAttached<TClass, TValue> IDependencyPropertyConfiguratorAttached<TClass, TValue>.OnChanged(Action<TClass, AvaloniaPropertyChangedEventArgs<TValue>> action)
       => Do(() => _OnChanged(action));

   void _OnChanged(Action<TClass, AvaloniaPropertyChangedEventArgs<TValue>> action)
   {
      _postAction = p => p.Changed.AddClassHandler(action);
   }

   //void OnChange<TSender>(Action<TSender, bool> action) where TSender : AvaloniaObject
   //{
   //    _notifying += (s, b) => action((TSender)s, b);
   //}

   //void OnChangeBeforeNotification(Action<TClass> action)
   //{
   //    _notifying += (s,before) =>
   //    {
   //        if(before && s is TClass cls) action(cls);
   //    };
   //}
   //void OnChangeAfterNotification(Action<TClass> action)
   //{
   //    _notifying += (s,before) =>
   //    {
   //        if(!before && s is TClass cls) action(cls);
   //    };
   //}

   /// <summary>
   ///     Represents a method used as a callback that validates the effective value of
   ///     a dependency property.
   /// </summary>
   /// <param name="func"></param>
   /// <returns></returns>
   public IDependencyPropertyConfigurator<TClass, TValue> Validate(Func<TValue, bool> func)
   {
      _validate = func;
      return this;
   }

   /// <summary>
   /// A value coercion callback.
   /// </summary>
   /// <param name="func"></param>
   /// <returns></returns>
   public IDependencyPropertyConfigurator<TClass, TValue> Coerce(Func<AvaloniaObject, TValue, TValue> func)
   {
      _coerce = func;
      return this;
   }
   public IDependencyPropertyConfigurator<TClass, TValue> Coerce(Func<TValue, TValue> func)
   {
      _coerce = (a, v) => func(v);
      return this;
   }

   /// <summary>
   ///     Sets the default value of the dependency property.
   /// </summary>
   /// <param name="value"></param>
   /// <returns></returns>
   public IDependencyPropertyConfigurator<TClass, TValue> Default(TValue value)
   {
      _defaultValue = value;
      return this;
   }

   IDependencyPropertyConfiguratorDirect<TClass, TValue> IDependencyPropertyConfiguratorDirect<TClass, TValue>.Default(TValue value)
   {
      _defaultValue = value;
      return this;
   }

   public IDependencyPropertyConfiguratorDirect<TClass, TValue> Getter(Func<TClass, TValue> getter)
       => Do(() => _getter = getter);

   IDependencyPropertyConfiguratorDirect<TClass, TValue> IDependencyPropertyConfiguratorDirect<TClass, TValue>.Setter(Action<TClass, TValue> setter)
       => Do(() => _setter = setter);

   public IDependencyPropertyConfiguratorDirect<TClass, TValue> EnableDataValidation =>
       Do(() => _enableDataValidation = true);
}