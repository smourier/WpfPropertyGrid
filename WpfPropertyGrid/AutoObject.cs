using System.Xml.Serialization;

namespace WpfPropertyGrid
{
    /// <summary>
    /// Defines a utility class to implement objects with typed properties without private fields.
    /// This class supports automatically property change notifications and error validations.
    /// </summary>
    public abstract class AutoObject : IDataErrorInfo, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private IDictionary<string, object> _properties;
        private IDictionary<string, object> _changedProperties;
        private readonly Dictionary<string, object> _defaultValues = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoObject"/> class.
        /// </summary>
        protected AutoObject()
        {
            RaisePropertyChanged = true;
            ThrowOnInvalidProperty = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to throw errors if invalid properties are deserialized.
        /// </summary>
        /// <value>
        /// <c>true</c> if errors must be thrown when invalid properties are deserialized; otherwise, <c>false</c>.
        /// </value>
        [XmlIgnore]
        [Browsable(false)]
        public virtual bool ThrowOnInvalidProperty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether property changed events are raised.
        /// </summary>
        /// <value>
        /// <c>true</c> if property changed events are raised; otherwise, <c>false</c>.
        /// </value>
        [XmlIgnore]
        [Browsable(false)]
        public virtual bool RaisePropertyChanged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether property changed events are raised.
        /// </summary>
        /// <value>
        /// <c>true</c> if property changed events are raised; otherwise, <c>false</c>.
        /// </value>
        [XmlIgnore]
        [Browsable(false)]
        public virtual bool TrackChangedProperties { get; set; }

        /// <summary>
        /// Gets a value indicating whether at least a property has changed since the last time this property has been set to false.
        /// </summary>
        /// <value>
        /// <c>true</c> if at least a property has changed; otherwise, <c>false</c>.
        /// </value>
        [XmlIgnore]
        [Browsable(false)]
        public virtual bool HasChanged { get; set; }

        /// <summary>
        /// Gets the properties default values.
        /// </summary>
        /// <value>The default values.</value>
        protected IDictionary<string, object> DefaultValues => _defaultValues;

        /// <summary>
        /// Gets the changed properties and their original values.
        /// </summary>
        /// <value>The changed properties and their original values.</value>
        protected IDictionary<string, object> ChangedProperties
        {
            get
            {
                _changedProperties ??= CreatePropertiesDictionary();
                return _changedProperties;
            }
        }

        /// <summary>
        /// Gets the properties and their values.
        /// </summary>
        /// <value>The properties and their values.</value>
        protected IDictionary<string, object> Properties
        {
            get
            {
                _properties ??= CreatePropertiesDictionary();
                return _properties;
            }
        }

        /// <summary>
        /// Creates the properties dictionary.
        /// </summary>
        /// <returns></returns>
        protected virtual IDictionary<string, object> CreatePropertiesDictionary() => new Dictionary<string, object>();

        /// <summary>
        /// Validates this instances.
        /// </summary>
        /// <param name="errors">The errors.</param>
        /// <param name="memberName">The member to validate or null to validate all members.</param>
        protected virtual void Validate(IList<string> errors, string memberName)
        {
        }

        /// <summary>
        /// Validates the specified member name.
        /// </summary>
        /// <param name="memberName">The member to validate or null to validate all members.</param>
        /// <returns>A string if an error occured; null otherwise.</returns>
        protected virtual string Validate(string memberName)
        {
            List<string> errors = [];
            Validate(errors, memberName);
            if (errors.Count == 0)
                return null;

            return string.Join(Environment.NewLine, errors);
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        string IDataErrorInfo.Error => Validate(null);

        /// <summary>
        /// Gets the <see cref="string"/> with the specified column name.
        /// </summary>
        /// <value></value>
        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                return Validate(columnName);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        [XmlIgnore]
        [Browsable(false)]
        public virtual bool IsValid => Validate(null) == null;

        /// <summary>
        /// Sets a property value.
        /// </summary>
        /// <typeparam name="T">The property type</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="setChanged">if set to <c>true</c> set the HasChanged property to true.</param>
        /// <param name="trackChanged">if set to <c>true</c> the property is tracked in the changed properties.</param>
        /// <returns>true if the value has changed; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected bool SetProperty(object value, bool setChanged, bool trackChanged, [CallerMemberName] string name = null) => SetProperty(name, value, setChanged, false, trackChanged);

        /// <summary>
        /// Sets a property value.
        /// </summary>
        /// <typeparam name="T">The property type</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="setChanged">if set to <c>true</c> set the HasChanged property to true.</param>
        /// <returns>true if the value has changed; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected bool SetProperty(object value, bool setChanged, [CallerMemberName] string name = null) => SetProperty(name, value, setChanged, false, true);

        /// <summary>
        /// Sets a property value.
        /// </summary>
        /// <typeparam name="T">The property type</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>true if the value has changed; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected bool SetProperty(object value, [CallerMemberName] string name = null) => SetProperty(name, value);

        /// <summary>
        /// Gets a property value.
        /// </summary>
        /// <typeparam name="T">The property type</typeparam>
        /// <returns>The value automatically converted into the requested type.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected virtual T GetProperty<T>([CallerMemberName] string name = null)
        {
            T defaultValue;
            if (_defaultValues.TryGetValue(name, out object obj))
            {
                defaultValue = ConversionService.ChangeType<T>(obj);
            }
            else
            {
                // runtime methodbase has no custom atts
                DefaultValueAttribute att = null;

                PropertyInfo pi = GetType().GetProperty(name);
                if (pi != null)
                {
                    att = Attribute.GetCustomAttribute(pi, typeof(DefaultValueAttribute), true) as DefaultValueAttribute;
                }

                defaultValue = att != null ? ConversionService.ChangeType(att.Value, default(T)) : default(T);
                _defaultValues[name] = defaultValue;
            }

            return GetProperty(defaultValue, name);
        }

        /// <summary>
        /// Gets the default value for a given property.
        /// </summary>
        /// <param name="propertyName">The property name. May not be null.</param>
        /// <returns>The default value. May be null.</returns>
        protected virtual object GetDefaultValue(string propertyName)
        {
            ArgumentNullException.ThrowIfNull(propertyName);

            PropertyInfo pi = GetType().GetProperty(propertyName);
            if (pi == null)
            {
                if (ThrowOnInvalidProperty)
                    throw new InvalidOperationException($"Cannot find a '{propertyName}' property in '{GetType().FullName}' type.");

                return null;
            }

            object defaultValue = pi.PropertyType.IsValueType ? Activator.CreateInstance(pi.PropertyType) : null;
            if (Attribute.GetCustomAttribute(pi, typeof(DefaultValueAttribute), true) is DefaultValueAttribute att)
                return ConversionService.ChangeType(att.Value, defaultValue);

            return defaultValue;
        }

        /// <summary>
        /// Gets the type of the specified property.
        /// </summary>
        /// <param name="name">The property name. May not be null.</param>
        /// <returns>An instance of the Type class</returns>
        protected virtual Type GetPropertyType(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            PropertyInfo pi = GetType().GetProperty(name);
            if (pi == null)
                throw new ArgumentException(null, nameof(name));

            return pi.PropertyType;
        }

        /// <summary>
        /// Determines if two property values are equal.
        /// </summary>
        /// <param name="value1">The value1. May be null.</param>
        /// <param name="value2">The value2. May be null.</param>
        /// <returns>true if the values are equal.</returns>
        protected virtual bool ArePropertiesEqual(object value1, object value2)
        {
            if (value1 == null)
                return value2 == null;

            return value1.Equals(value2);
        }

        /// <summary>
        /// Copies the properties to a target object.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>true if at least one property has been changed; false otherwise.</returns>
        public virtual bool CopyProperties(AutoObject target) => CopyProperties(target, false);

        /// <summary>
        /// Copies the properties to a target object.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="raisePropertyChanged">if set to <c>true</c> the PropertyChanged event may be raised.</param>
        /// <returns>true if at least one property has been changed; false otherwise.</returns>
        public virtual bool CopyProperties(AutoObject target, bool raisePropertyChanged)
        {
            ArgumentNullException.ThrowIfNull(target);

            bool b = target.RaisePropertyChanged;
            target.RaisePropertyChanged = raisePropertyChanged;
            try
            {
                bool changed = false;
                foreach (KeyValuePair<string, object> kv in Properties)
                {
                    if (target.SetProperty(kv.Key, kv.Value))
                    {
                        changed = true;
                    }
                }
                return changed;
            }
            finally
            {
                target.RaisePropertyChanged = b;
            }
        }

        protected bool SetProperty(string name, object value) => SetProperty(name, value, true, false, true);

        protected virtual bool SetProperty(string name, object? value, bool setChanged, bool forceRaise, bool trackChanged)
        {
            ArgumentNullException.ThrowIfNull(name);

            var hasOldValue = Properties.TryGetValue(name, out object oldValue);
            if (hasOldValue && ArePropertiesEqual(value, oldValue))
                return false;

            if (!hasOldValue)
            {
                oldValue = GetDefaultValue(name);
                if (ArePropertiesEqual(value, oldValue))
                    return false;
            }

            if (!OnPropertyChanging(name))
                return false;

            if (trackChanged && TrackChangedProperties)
            {
                if (!ChangedProperties.ContainsKey(name))
                {
                    ChangedProperties[name] = oldValue;
                }
            }

            bool oldValid = IsValid;
            Properties[name] = value;
            OnPropertyChanged(name, setChanged, forceRaise);
            if (oldValid != IsValid)
            {
                OnPropertyChanged("IsValid");
            }
            return true;
        }

        /// <summary>
        /// Rollbacks to the original properties.
        /// </summary>
        /// <param name="raisePropertyChanged">if set to <c>true</c> the PropertyChanged event may be raised.</param>
        /// <param name="setChanged">if set to <c>true</c> set the HasChanged property to true.</param>
        /// <returns>true if at least one property has been changed; false otherwise.</returns>
        protected virtual bool RollbackChangedProperties(bool raisePropertyChanged, bool setChanged)
        {
            bool b = RaisePropertyChanged;
            RaisePropertyChanged = raisePropertyChanged;
            try
            {
                bool changed = false;
                foreach (KeyValuePair<string, object> kv in ChangedProperties)
                {
                    if (SetProperty(kv.Key, kv.Value, setChanged, false, true))
                    {
                        changed = true;
                    }
                }
                return changed;
            }
            finally
            {
                RaisePropertyChanged = b;
            }
        }

        /// <summary>
        /// Gets a property value.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="name">The property name. May not be null.</param>
        /// <returns></returns>
        protected virtual T GetProperty<T>(T defaultValue, [CallerMemberName] string name = null)
        {
            ArgumentNullException.ThrowIfNull(name);

            if (!Properties.TryGetValue(name, out object obj))
                return defaultValue;

            return ConversionService.ChangeType(obj, defaultValue);
        }

        /// <summary>
        /// Called when a property is about to change.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>true if the property can be changed; otherwise false.</returns>
        protected virtual bool OnPropertyChanging(string name) => true;

        /// <summary>
        /// Called when a property changed.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>true if the event has been raised; otherwise false.</returns>
        protected bool OnPropertyChanged(string name) => OnPropertyChanged(name, true, false);

        /// <summary>
        /// Called when a property changed.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="setChanged">if set to <c>true</c> set the HasChanged property to true.</param>
        /// <param name="forceRaise">if set to <c>true</c> force the raise, even if RaisePropertyChanged is set to false.</param>
        /// <returns>
        /// true if the event has been raised; otherwise false.
        /// </returns>
        protected virtual bool OnPropertyChanged(string name, bool setChanged, bool forceRaise)
        {
            ArgumentNullException.ThrowIfNull(name);

            if (setChanged)
            {
                HasChanged = true;
            }

            if (!RaisePropertyChanged && !forceRaise)
                return false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            return true;
        }
    }
}
