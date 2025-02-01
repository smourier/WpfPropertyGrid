using System.Xml.Serialization;

namespace WpfPropertyGrid
{
    public abstract class AutoObject : IDataErrorInfo, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly Dictionary<string, object?> _properties = [];
        private readonly Dictionary<string, object?> _changedProperties = [];
        private readonly Dictionary<string, object?> _defaultValues = [];

        protected AutoObject()
        {
            RaisePropertyChanged = true;
            ThrowOnInvalidProperty = true;
        }

        [XmlIgnore]
        [Browsable(false)]
        public virtual bool ThrowOnInvalidProperty { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public virtual bool RaisePropertyChanged { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public virtual bool TrackChangedProperties { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public virtual bool HasChanged { get; set; }

        protected IDictionary<string, object?> DefaultValues => _defaultValues;
        protected IDictionary<string, object?> ChangedProperties => _changedProperties;
        protected IDictionary<string, object?> Properties => _properties;

        protected virtual void Validate(IList<string> errors, string? memberName)
        {
        }

        protected virtual string? Validate(string? memberName)
        {
            List<string> errors = [];
            Validate(errors, memberName);
            if (errors.Count == 0)
                return null;

            return string.Join(Environment.NewLine, errors);
        }

        string IDataErrorInfo.Error => Validate(null) ?? string.Empty;
        string IDataErrorInfo.this[string? columnName] => Validate(columnName) ?? string.Empty;

        [XmlIgnore]
        [Browsable(false)]
        public virtual bool IsValid => Validate(null) == null;

        protected bool SetProperty(object? value, bool setChanged, bool trackChanged, [CallerMemberName] string? name = null) => SetProperty(name!, value, setChanged, false, trackChanged);

        protected bool SetProperty(object? value, bool setChanged, [CallerMemberName] string? name = null) => SetProperty(name!, value, setChanged, false, true);

        protected bool SetProperty(object? value, [CallerMemberName] string? name = null) => SetProperty(name!, value);

        protected virtual T? GetProperty<T>([CallerMemberName] string? name = null)
        {
            ArgumentNullException.ThrowIfNull(name);

            T? defaultValue;
            if (_defaultValues.TryGetValue(name, out var obj))
            {
                defaultValue = ConversionService.ChangeType<T>(obj);
            }
            else
            {
                // runtime methodbase has no custom atts
                DefaultValueAttribute? att = null;

                var pi = GetType().GetProperty(name);
                if (pi != null)
                {
                    att = Attribute.GetCustomAttribute(pi, typeof(DefaultValueAttribute), true) as DefaultValueAttribute;
                }

                defaultValue = att != null ? ConversionService.ChangeType(att.Value, default(T)) : default;
                _defaultValues[name] = defaultValue;
            }

            return GetProperty(defaultValue, name);
        }

        protected virtual object? GetDefaultValue(string propertyName)
        {
            ArgumentNullException.ThrowIfNull(propertyName);

            var pi = GetType().GetProperty(propertyName);
            if (pi == null)
            {
                if (ThrowOnInvalidProperty)
                    throw new InvalidOperationException($"Cannot find a '{propertyName}' property in '{GetType().FullName}' type.");

                return null;
            }

            var defaultValue = pi.PropertyType.IsValueType ? Activator.CreateInstance(pi.PropertyType) : null;
            if (Attribute.GetCustomAttribute(pi, typeof(DefaultValueAttribute), true) is DefaultValueAttribute att)
                return ConversionService.ChangeType(att.Value, defaultValue);

            return defaultValue;
        }

        protected virtual Type GetPropertyType(string name)
        {
            ArgumentNullException.ThrowIfNull(name);
            var pi = GetType().GetProperty(name) ?? throw new ArgumentException(null, nameof(name));
            return pi.PropertyType;
        }

        protected virtual bool ArePropertiesEqual(object? value1, object? value2)
        {
            if (value1 == null)
                return value2 == null;

            return value1.Equals(value2);
        }

        public virtual bool CopyProperties(AutoObject target, bool raisePropertyChanged = false)
        {
            ArgumentNullException.ThrowIfNull(target);

            var b = target.RaisePropertyChanged;
            target.RaisePropertyChanged = raisePropertyChanged;
            try
            {
                var changed = false;
                foreach (var kv in Properties)
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

        protected virtual bool SetProperty(string name, object? value, bool setChanged = true, bool forceRaise = false, bool trackChanged = true)
        {
            ArgumentNullException.ThrowIfNull(name);

            var hasOldValue = Properties.TryGetValue(name, out var oldValue);
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

            var oldValid = IsValid;
            Properties[name] = value;
            OnPropertyChanged(name, setChanged, forceRaise);
            if (oldValid != IsValid)
            {
                OnPropertyChanged("IsValid");
            }
            return true;
        }

        protected virtual bool RollbackChangedProperties(bool raisePropertyChanged, bool setChanged)
        {
            var b = RaisePropertyChanged;
            RaisePropertyChanged = raisePropertyChanged;
            try
            {
                var changed = false;
                foreach (var kv in ChangedProperties)
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

        protected virtual T? GetProperty<T>(T defaultValue, [CallerMemberName] string? name = null)
        {
            ArgumentNullException.ThrowIfNull(name);

            if (!Properties.TryGetValue(name, out var obj))
                return defaultValue;

            return ConversionService.ChangeType(obj, defaultValue);
        }

        protected virtual bool OnPropertyChanging(string name) => true;

        protected bool OnPropertyChanged(string name) => OnPropertyChanged(name, true, false);

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
