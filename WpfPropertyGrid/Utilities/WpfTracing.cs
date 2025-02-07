namespace WpfPropertyGrid.Utilities;

public static class WpfTracing
{
    public static void Enable(SourceLevels levels = SourceLevels.Warning, TraceListener? listener = null)
    {
        listener ??= new DefaultTraceListener();
        PresentationTraceSources.Refresh();
        foreach (var pi in typeof(PresentationTraceSources).GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
            if (pi.Name == "FreezableSource")
                continue;

            if (typeof(TraceSource).IsAssignableFrom(pi.PropertyType))
            {
                var ts = (TraceSource)pi.GetValue(null, null)!;
                ts.Listeners.Add(listener);
                ts.Switch.Level = levels;
            }
        }
    }
}
