namespace WpfPropertyGrid.Utilities;

[Flags]
public enum DictionaryObjectPropertySetOptions
{
    None = 0x0,
    DontRaiseOnPropertyChanging = 0x1,
    DontRaiseOnPropertyChanged = 0x2,
    DontTestValuesForEquality = 0x4,
    DontRaiseOnErrorsChanged = 0x8,
    ForceRaiseOnPropertyChanged = 0x10,
    RollbackChangeOnError = 0x20,
}
