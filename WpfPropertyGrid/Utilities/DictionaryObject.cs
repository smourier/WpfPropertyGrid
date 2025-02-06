namespace WpfPropertyGrid.Utilities;

public abstract class DictionaryObject : INotifyPropertyChanged, INotifyPropertyChanging, IDataErrorInfo, INotifyDataErrorInfo
{
    protected DictionaryObject()
    {
        DictionaryObjectRaiseOnPropertyChanging = true;
        DictionaryObjectRaiseOnPropertyChanged = true;
        DictionaryObjectRaiseOnErrorsChanged = true;
    }

    protected virtual ConcurrentDictionary<string, DictionaryObjectProperty?> DictionaryObjectProperties { get; } = new(StringComparer.Ordinal); // properties are case sensitive in .NET

    public event PropertyChangingEventHandler? PropertyChanging;
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    public event EventHandler<DictionaryObjectPropertyRollbackEventArgs>? PropertyRollback;

    protected virtual bool DictionaryObjectRaiseOnPropertyChanging { get; set; }
    protected virtual bool DictionaryObjectRaiseOnPropertyChanged { get; set; }
    protected virtual bool DictionaryObjectRaiseOnErrorsChanged { get; set; }

    protected string? DictionaryObjectError => DictionaryObjectGetError(null);
    public bool HasErrors => DictionaryObjectGetErrors(null)?.OfType<object>().Any() == true;
    public bool HasNoError => !HasErrors;

    protected virtual string? DictionaryObjectGetError(string? propertyName)
    {
        var errors = DictionaryObjectGetErrors(propertyName);
        if (errors == null)
            return null;

        var error = string.Join(Environment.NewLine, errors.Cast<object>().Select(e => string.Format("{0}", e)));
        return !string.IsNullOrEmpty(error) ? error : null;
    }

    protected virtual IEnumerable DictionaryObjectGetErrors(string? propertyName) => Enumerable.Empty<object>();

    protected void OnErrorsChanged(string? name)
    {
        OnErrorsChanged(this, new DataErrorsChangedEventArgs(name));
        OnPropertyChanged(nameof(HasErrors));
        OnPropertyChanged(nameof(HasNoError));
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) => OnPropertyChanged(this, new PropertyChangedEventArgs(name));

    protected virtual void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e) => ErrorsChanged?.Invoke(sender, e);
    protected virtual void OnPropertyRollback(object sender, DictionaryObjectPropertyRollbackEventArgs e) => PropertyRollback?.Invoke(sender, e);
    protected virtual void OnPropertyChanging(object sender, PropertyChangingEventArgs e) => PropertyChanging?.Invoke(sender, e);
    protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(sender, e);

    protected virtual DictionaryObject DictionaryObjectCreateInstance() => (DictionaryObject)Activator.CreateInstance(GetType())!;

    public virtual object Clone()
    {
        var clone = DictionaryObjectCreateInstance();
        clone.CopyFrom(this);
        return clone;
    }

    public virtual void CopyFrom(DictionaryObject from)
    {
        ArgumentNullException.ThrowIfNull(from);
        foreach (var kv in from.DictionaryObjectProperties)
        {
            DictionaryObjectSetPropertyValue(kv.Value?.Value, kv.Key);
        }
    }

    protected string? DictionaryObjectGetNullifiedPropertyValue(string? defaultValue = null, [CallerMemberName] string? name = null) => DictionaryObjectGetPropertyValue(defaultValue, name)?.Nullify();
    protected T? DictionaryObjectGetPropertyValue<T>([CallerMemberName] string? name = null) => DictionaryObjectGetPropertyValue<T>(default, name);
    protected virtual T? DictionaryObjectGetPropertyValue<T>(T? defaultValue, [CallerMemberName] string? name = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        DictionaryObjectProperties.TryGetValue(name, out var property);
        if (property == null)
            return defaultValue;

        if (!ConversionService.TryChangeType<T>(property.Value, out var value))
            return defaultValue;

        return value;
    }

    protected virtual bool DictionaryObjectAreValuesEqual(object? value1, object? value2)
    {
        if (value1 == null)
            return value2 == null;

        if (value2 == null)
            return false;

        return value1.Equals(value2);
    }

    private class ObjectComparer(DictionaryObject obj) : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y) => obj.DictionaryObjectAreValuesEqual(x, y);
        public int GetHashCode(object obj) => (obj?.GetHashCode()).GetValueOrDefault();
    }

    protected virtual bool DictionaryObjectAreErrorsEqual(IEnumerable? errors1, IEnumerable? errors2)
    {
        if (errors1 == null && errors2 == null)
            return true;

        var dic = new Dictionary<object, int>(new ObjectComparer(this));
        var left = errors1 != null ? errors1.Cast<object>() : [];
        foreach (var obj in left)
        {
            if (dic.TryGetValue(obj, out var value))
            {
                dic[obj] = ++value;
            }
            else
            {
                dic.Add(obj, 1);
            }
        }

        if (errors2 == null)
            return dic.Count == 0;

        foreach (var obj in errors2)
        {
            if (dic.TryGetValue(obj, out var value))
            {
                dic[obj] = --value;
            }
            else
                return false;
        }
        return dic.Values.All(c => c == 0);
    }

    protected virtual DictionaryObjectProperty? DictionaryObjectUpdatingProperty(DictionaryObjectPropertySetOptions options, string name, DictionaryObjectProperty? oldProperty, DictionaryObjectProperty newProperty) => null;
    protected virtual DictionaryObjectProperty? DictionaryObjectUpdatedProperty(DictionaryObjectPropertySetOptions options, string name, DictionaryObjectProperty? oldProperty, DictionaryObjectProperty newProperty) => null;
    protected virtual DictionaryObjectProperty? DictionaryObjectRollbackProperty(DictionaryObjectPropertySetOptions options, string name, DictionaryObjectProperty? oldProperty, DictionaryObjectProperty newProperty) => null;
    protected virtual DictionaryObjectProperty? DictionaryObjectCreateProperty() => new();

    protected bool DictionaryObjectSetPropertyValue(object? value, [CallerMemberName] string? name = null) => DictionaryObjectSetPropertyValue(value, DictionaryObjectPropertySetOptions.None, name);
    protected virtual bool DictionaryObjectSetPropertyValue(object? value, DictionaryObjectPropertySetOptions options, [CallerMemberName] string? name = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        object[]? oldErrors = null;
        var rollbackOnError = options.HasFlag(DictionaryObjectPropertySetOptions.RollbackChangeOnError);
        var onErrorsChanged = !options.HasFlag(DictionaryObjectPropertySetOptions.DontRaiseOnErrorsChanged);
        if (!DictionaryObjectRaiseOnErrorsChanged)
        {
            onErrorsChanged = false;
        }

        if (onErrorsChanged || rollbackOnError)
        {
            oldErrors = DictionaryObjectGetErrors(name)?.OfType<object>()?.ToArray();
        }

        var forceChanged = options.HasFlag(DictionaryObjectPropertySetOptions.ForceRaiseOnPropertyChanged);
        var onChanged = !options.HasFlag(DictionaryObjectPropertySetOptions.DontRaiseOnPropertyChanged);
        if (!DictionaryObjectRaiseOnPropertyChanged)
        {
            onChanged = false;
            forceChanged = false;
        }

        var newProp = DictionaryObjectCreateProperty() ?? throw new InvalidOperationException();
        newProp.Value = value;
        DictionaryObjectProperty? oldProp = null;
        var finalProp = DictionaryObjectProperties.AddOrUpdate(name, newProp, (k, o) =>
        {
            if (name == "IsChecked")
            {
            }


            oldProp = o;
            var updating = DictionaryObjectUpdatingProperty(options, k, o, newProp);
            if (updating != null)
                return updating;

            var testEquality = !options.HasFlag(DictionaryObjectPropertySetOptions.DontTestValuesForEquality);
            if (testEquality && o != null && DictionaryObjectAreValuesEqual(value, o.Value))
                return o;

            var onChanging = !options.HasFlag(DictionaryObjectPropertySetOptions.DontRaiseOnPropertyChanging);
            if (!DictionaryObjectRaiseOnPropertyChanging)
            {
                onChanging = false;
            }

            if (onChanging)
            {
                var e = new DictionaryObjectPropertyChangingEventArgs(name, oldProp, newProp);
                OnPropertyChanging(this, e);
                if (e.Cancel)
                    return o;
            }

            var updated = DictionaryObjectUpdatedProperty(options, k, o, newProp);
            if (updated != null)
                return updated;

            return newProp;
        });

        if (forceChanged || onChanged && ReferenceEquals(finalProp, newProp))
        {
            var rollbacked = false;
            if (rollbackOnError)
            {
                if ((DictionaryObjectGetErrors(name)?.Cast<object>().Any()) == true)
                {
                    var rolled = DictionaryObjectRollbackProperty(options, name, oldProp, newProp) ?? oldProp;
                    if (rolled == null)
                    {
                        DictionaryObjectProperties.TryRemove(name, out var dop);
                    }
                    else
                    {
                        DictionaryObjectProperties.AddOrUpdate(name, rolled, (k, o) => rolled);
                    }

                    var e = new DictionaryObjectPropertyRollbackEventArgs(name, rolled, value);
                    OnPropertyRollback(this, e);
                    rollbacked = true;
                }
            }

            if (!rollbacked)
            {
                var e = new DictionaryObjectPropertyChangedEventArgs(name, oldProp, newProp);
                OnPropertyChanged(this, e);

                if (onErrorsChanged)
                {
                    var newErrors = DictionaryObjectGetErrors(name);
                    if (!DictionaryObjectAreErrorsEqual(oldErrors, newErrors))
                    {
                        OnErrorsChanged(name);
                    }
                }
                return true;
            }
        }

        return false;
    }

    string IDataErrorInfo.Error => DictionaryObjectError!;
    string IDataErrorInfo.this[string columnName] => DictionaryObjectGetError(columnName)!;
    bool INotifyDataErrorInfo.HasErrors => !HasErrors;
    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName) => DictionaryObjectGetErrors(propertyName);
}
