namespace WpfPropertyGrid.Samples;

public class Customer : DictionaryObject
{
    public Customer()
    {
        Id = Guid.NewGuid();
        ListOfStrings = ["string 1", "string 2"];
        ArrayOfStrings = [.. ListOfStrings];
        CreationDateAndTime = DateTime.Now;
        Description = "press button to edit...";
        ByteArray1 = [1, 2, 3, 4, 5, 6, 7, 8];
        WebSite = "https://www.simonmourier.com";
        Status = Status.Valid;
        Addresses = [new Address { Line1 = "1 Microsoft Way", City = "Redmond", State = "WA", ZipCode = 98052, Country = "USA" }];
        DaysOfWeek = DaysOfWeek.WeekDays;
        PercentageOfSatisfaction = 50;
        PreferredColorName = "DodgerBlue";
        PreferredFont = Fonts.SystemFontFamilies.FirstOrDefault(f => string.Equals(f.Source, "Consolas", StringComparison.OrdinalIgnoreCase));
        SampleNullableBooleanDropDownList = false;
        SampleBooleanDropDownList = true;
        MultiEnumString = "First, Second";
        SubObject = Address.Parse("1600 Amphitheatre Parkway Mountain View, CA 94043, USA");
        Color = Colors.Red;
    }

    [DisplayName("System In Dark Mode")]
    [Category("Theming")]
#pragma warning disable CA1822 // Mark members as static
    public bool IsSystemDarkMode => WpfUtilities.IsDarkMode() == true;
#pragma warning restore CA1822 // Mark members as static

    [DisplayName("Window Theme Mode")]
    [Category("Theming")]
    [PropertyGridOptions(GetValueMethodName = nameof(GetCurrentThemeMode))]
#pragma warning disable CA1822 // Mark members as static
    public WpfTheme? CurrentThemeMode => null;
#pragma warning restore CA1822 // Mark members as static

    private static WpfTheme GetCurrentThemeMode(PropertyGridProperty property) => WpfUtilities.GetTheme(property.DataProvider.Grid);

    [DisplayName("Guid (see menu on right-click)")]
    public Guid Id { get => DictionaryObjectGetPropertyValue<Guid>(); set => DictionaryObjectSetPropertyValue(value); }

    //[ReadOnly(true)]
    [Category("Dates and Times")]
    [PropertyGridOptions(EditorDataTemplateResourceKey = "DateTimePicker")]
    public DateTime CreationDateAndTime { get => DictionaryObjectGetPropertyValue<DateTime>(); set => DictionaryObjectSetPropertyValue(value); }

    [DisplayName("Sub Object (Address)")]
    [PropertyGridOptions(ForcePropertyChanged = true)]
    public Address? SubObject
    {
        get => DictionaryObjectGetPropertyValue<Address>();
        set
        {
            var so = SubObject;
            if (so != null)
            {
                so.PropertyChanged -= OnSubObjectPropertyChanged;
            }

            if (DictionaryObjectSetPropertyValue(value))
            {
                so = SubObject;
                if (so != null)
                {
                    so.PropertyChanged += OnSubObjectPropertyChanged;
                }

                OnPropertyChanged(nameof(SubObjectAsObject));
            }
        }
    }

    private void OnSubObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SubObject));
        OnPropertyChanged(nameof(SubObjectAsObject));
    }

    [DisplayName("Sub Object (Address as Object)")]
    [PropertyGridOptions(EditorDataTemplateResourceKey = "ObjectEditor", ForcePropertyChanged = true)]
    public Address? SubObjectAsObject { get => SubObject; set => SubObject = value; }

    [PropertyGridOptions(SortOrder = 10)]
    public string? FirstName { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(SortOrder = 20)]
    public string? LastName { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [Category("Dates and Times")]
    [PropertyGridOptions(SortOrder = 40)]
    public DateTime DateOfBirth { get => DictionaryObjectGetPropertyValue<DateTime>(); set => DictionaryObjectSetPropertyValue(value); }

    [Category("Enums")]
    [PropertyGridOptions(SortOrder = 30)]
    public Gender Gender { get => DictionaryObjectGetPropertyValue<Gender>(); set => DictionaryObjectSetPropertyValue(value); }

    public Color Color { get => DictionaryObjectGetPropertyValue<Color>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(EditorDataTemplateResourceKey = "CustomColorEditor")]
    [DisplayName("Color (Custom Editor)")]
    public Color CustomColor { get => DictionaryObjectGetPropertyValue<Color>(); set => DictionaryObjectSetPropertyValue(value); }

    [Category("Enums")]
    public Status Status
    {
        get => DictionaryObjectGetPropertyValue<Status>();
        set
        {
            if (DictionaryObjectSetPropertyValue(value))
            {
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(StatusColorString));
            }
        }
    }

    [PropertyGridOptions(EditorDataTemplateResourceKey = "ColorEnumEditor", PropertyType = typeof(PropertyGridEnumProperty))]
    [DisplayName("Status (colored enum)")]
    [ReadOnly(true)]
    [Category("Enums")]
    public Status StatusColor { get => Status; set => Status = value; }

    [PropertyGridOptions(IsEnum = true, EnumNames = ["1N\\/AL1D", "\\/AL1D", "UNKN0WN"], EnumValues = [Status.Invalid, Status.Valid, Status.Unknown])]
    [DisplayName("Status (enum as string list)")]
    [Category("Enums")]
    public string StatusColorString { get => Status.ToString(); set => Status = Enum.Parse<Status>(value); }

    [PropertyGridOptions(IsEnum = true, IsFlagsEnum = true, EnumNames = ["First", "Second", "Third"])]
    [Category("Enums")]
    public string? MultiEnumString { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(IsEnum = true, IsFlagsEnum = true, EnumNames = ["No,nze", "My First", "My Second", "My Third"], EnumValues = [8, 1, 2, 4])]
    [Category("Enums")]
    public string? MultiEnumStringWithDisplay { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [Category("Dates and Times")]
    [Description("This is the timespan tooltip")]
    public TimeSpan TimeSpan { get => DictionaryObjectGetPropertyValue<TimeSpan>(); set => DictionaryObjectSetPropertyValue(value); }

    [Category("Security")]
    [PropertyGridOptions(EditorDataTemplateResourceKey = "PasswordEditor")]
    [DisplayName("Password (in SecureString)")]
    public SecureString? Password { get => DictionaryObjectGetPropertyValue<SecureString>(); set { if (DictionaryObjectSetPropertyValue(value)) { OnPropertyChanged(nameof(PasswordString)); } } }

    [Category("Security")]
    [DisplayName("Password (readonly clear string)")]
    public string? PasswordString { get => Password?.ConvertToUnsecureString(); }

    [Browsable(false)]
    public string? NotBrowsable { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [DisplayName("Description (multi-line)")]
    [PropertyGrid(Name = "Foreground", Value = "White")]
    [PropertyGrid(Name = "Background", Value = "Black")]
    [PropertyGridOptions(EditorDataTemplateResourceKey = "BigTextEditor")]
    public string? Description { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(EditorDataTemplateResourceKey = "FormatTextEditor")]
    [PropertyGrid(Name = "Format", Value = "0x{0}")]
    [ReadOnly(true)]
    [DisplayName("Byte Array (hex format)")]
    public byte[]? ByteArray1 { get => DictionaryObjectGetPropertyValue<byte[]>(); set => DictionaryObjectSetPropertyValue(value); }

    [ReadOnly(true)]
    [DisplayName("Byte Array (press button for hex dump)")]
    public byte[]? ByteArray2 { get => ByteArray1; set => ByteArray1 = value; }

    [PropertyGridOptions(EditorDataTemplateResourceKey = "CustomEditor", SortOrder = -10)]
    [DisplayName("Web Site (custom sort order)")]
    public string? WebSite { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [Category("Collections")]
    public string[]? ArrayOfStrings { get => DictionaryObjectGetPropertyValue<string[]>(); set => DictionaryObjectSetPropertyValue(value); }

    [Category("Collections")]
    public List<string>? ListOfStrings { get => DictionaryObjectGetPropertyValue<List<string>>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(EditorDataTemplateResourceKey = "AddressListEditor", SortOrder = 10)]
    [DisplayName("Addresses (custom editor)")]
    [Category("Collections")]
    public ObservableCollection<Address>? Addresses { get => DictionaryObjectGetPropertyValue<ObservableCollection<Address>>(); set => DictionaryObjectSetPropertyValue(value); }

    [DisplayName("Days Of Week (multi-valued enum)")]
    [Category("Enums")]
    public DaysOfWeek DaysOfWeek { get => DictionaryObjectGetPropertyValue<DaysOfWeek>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(EditorDataTemplateResourceKey = "PercentEditor")]
    [DisplayName("Percentage Of Satisfaction (int)")]
    public int PercentageOfSatisfactionInt { get => DictionaryObjectGetPropertyValue(0, nameof(PercentageOfSatisfaction)); set => DictionaryObjectSetPropertyValue(value, nameof(PercentageOfSatisfaction)); }

    [PropertyGridOptions(EditorDataTemplateResourceKey = "PercentEditor")]
    [DisplayName("Percentage Of Satisfaction (double)")]
    public double PercentageOfSatisfaction { get => DictionaryObjectGetPropertyValue<double>(); set { if (DictionaryObjectSetPropertyValue(value)) { OnPropertyChanged(nameof(PercentageOfSatisfactionInt)); } } }

    [PropertyGridOptions(EditorDataTemplateResourceKey = "NamedColorEditor")]
    [DisplayName("Preferred Color Name (custom editor)")]
    public string? PreferredColorName { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(EditorDataTemplateResourceKey = "FontEditor")]
    [DisplayName("Preferred Font (custom editor)")]
    public FontFamily? PreferredFont { get => DictionaryObjectGetPropertyValue<FontFamily>(); set => DictionaryObjectSetPropertyValue(value); }

    [DisplayName("Point (auto type converter)")]
    public PointInt32 Point { get => DictionaryObjectGetPropertyValue<PointInt32>(); set => DictionaryObjectSetPropertyValue(value); }

    [DisplayName("Nullable Int32 (supports empty string)")]
    public int? NullableInt32 { get => DictionaryObjectGetPropertyValue<int?>(); set => DictionaryObjectSetPropertyValue(value); }

    [DisplayName("Boolean (Checkbox)")]
    [Category("Booleans")]
    public bool SampleBoolean { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

    [DisplayName("Boolean (Checkbox three states)")]
    [Category("Booleans")]
    public bool? SampleNullableBoolean { get => DictionaryObjectGetPropertyValue<bool?>(); set => DictionaryObjectSetPropertyValue(value); }

    [DisplayName("Boolean (DropDownList)")]
    [Category("Booleans")]
    [PropertyGridOptions(EditorDataTemplateResourceKey = "BooleanDropDownListEditor")]
    public bool SampleBooleanDropDownList { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

    [DisplayName("Boolean (DropDownList 3 states)")]
    [Category("Booleans")]
    [PropertyGridOptions(EditorDataTemplateResourceKey = "NullableBooleanDropDownListEditor")]
    public bool? SampleNullableBooleanDropDownList { get => DictionaryObjectGetPropertyValue<bool?>(); set => DictionaryObjectSetPropertyValue(value); }
}

[TypeConverter(typeof(AddressConverter))]
public class Address : DictionaryObject
{
    [PropertyGridOptions(SortOrder = 10)]
    public string? Line1 { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(SortOrder = 20)]
    public string? Line2 { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(SortOrder = 30)]
    public int? ZipCode { get => DictionaryObjectGetPropertyValue<int?>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(SortOrder = 40)]
    public string? City { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(SortOrder = 50)]
    public string? State { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    [PropertyGridOptions(SortOrder = 60)]
    public string? Country { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

    public static Address Parse(string text)
    {
        var address = new Address();
        if (text != null)
        {
            var split = text.Split([','], StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 0)
            {
                var zip = 0;
                var index = -1;
                string? state = null;
                for (var i = 0; i < split.Length; i++)
                {
                    if (TryFindStateZip(split[i], out state, out zip))
                    {
                        index = i;
                        break;
                    }
                }

                if (index < 0)
                {
                    address.DistributeOverProperties(split, 0, int.MaxValue, nameof(Line1), nameof(Line2), nameof(City), nameof(Country));
                }
                else
                {
                    address.ZipCode = zip;
                    address.State = state;
                    address.DistributeOverProperties(split, 0, index, nameof(Line1), nameof(Line2), nameof(City));
                    if (string.IsNullOrWhiteSpace(address.City) && address.Line2 != null)
                    {
                        address.City = address.Line2;
                        address.Line2 = null;
                    }
                    address.DistributeOverProperties(split, index + 1, int.MaxValue, nameof(Country));
                }
            }
        }
        return address;
    }

    private static bool TryFindStateZip(string text, out string? state, out int zip)
    {
        state = null;
        var zipText = text;
        var pos = text.LastIndexOfAny([' ']);
        if (pos >= 0)
        {
            zipText = text[(pos + 1)..].Trim();
        }

        if (!int.TryParse(zipText, out zip) || zip <= 0)
            return false;

        state = text[..pos].Trim();
        return true;
    }

    private void DistributeOverProperties(string[] split, int offset, int max, params string[] properties)
    {
        for (var i = 0; i < properties.Length; i++)
        {
            if ((offset + i) >= split.Length || (offset + i) >= max)
                return;

            var s = split[offset + i].Trim();
            if (s.Length == 0)
                continue;

            DictionaryObjectSetPropertyValue(s, properties[i]);
        }
    }

    public override string ToString()
    {
        const string sep = ", ";
        var sb = new StringBuilder();
        AppendJoin(sb, Line1, string.Empty);
        AppendJoin(sb, Line2, sep);
        AppendJoin(sb, City, sep);
        AppendJoin(sb, State, sep);
        if (ZipCode.HasValue)
        {
            AppendJoin(sb, ZipCode.Value.ToString(), " ");
        }

        AppendJoin(sb, Country, sep);
        return sb.ToString();
    }

    private static void AppendJoin(StringBuilder sb, string? value, string? sep)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var s = sb.ToString();
        if (!s.EndsWith(' ') && !s.EndsWith(',') && !s.EndsWith(Environment.NewLine))
        {
            sb.Append(sep);
        }
        sb.Append(value);
    }
}

[Flags]
public enum DaysOfWeek
{
    NoDay = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 4,
    Thursday = 8,
    Friday = 16,
    Saturday = 32,
    Sunday = 64,
    WeekDays = Monday | Tuesday | Wednesday | Thursday | Friday
}

public enum Gender
{
    Male,
    Female
}

public enum Status
{
    [PropertyGrid(Name = "Foreground", Value = "Black")]
    [PropertyGrid(Name = "Background", Value = "Orange")]
    Unknown,
    [PropertyGrid(Name = "Foreground", Value = "White")]
    [PropertyGrid(Name = "Background", Value = "Red")]
    Invalid,
    [PropertyGrid(Name = "Foreground", Value = "White")]
    [PropertyGrid(Name = "Background", Value = "Green")]
    Valid
}

public class AddressConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
            return Address.Parse(s);

        return base.ConvertFrom(context, culture, value);
    }
}

public class PointInt32Converter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            var v = s.Split([';']);
            return new PointInt32(int.Parse(v[0]), int.Parse(v[1]));
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string))
            return ((PointInt32)value!).X + ";" + ((PointInt32)value).Y;

        return base.ConvertTo(context, culture, value, destinationType);
    }
}

[TypeConverter(typeof(PointInt32Converter))]
public struct PointInt32(int x, int y)
{
    public int X = x;
    public int Y = y;
}
