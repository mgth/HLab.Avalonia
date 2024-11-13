namespace HLab.Icons.Avalonia.Icons.Providers;

public abstract class IconProviderXaml(string sourceXaml) : IconProvider
{
    protected override object? GetIcon(uint foreground = 0) => XamlTools.FromXamlString(sourceXaml);

    protected override async Task<object?> GetIconAsync(uint foreground = 0) => await XamlTools.FromXamlStringAsync(sourceXaml).ConfigureAwait(true);

    public override async Task<string> GetTemplateAsync(uint foreground = 0) => sourceXaml;
}

public abstract class IconProviderXamlParser() : IconProviderXaml("")
{
    bool _parsed = false;

    protected abstract object? ParseIcon();
    protected abstract Task<object?> ParseIconAsync();
        
    protected override object? GetIcon(uint foreground = 0)
    {
        if (_parsed) return base.GetIcon(foreground);

        if (ParseIcon() is not { } icon) return null;

        // TODO : avalonia
        //SetSource(XamlWriter.Save(icon));
        //_parsed = true;

        return icon;
    }

    protected override async Task<object?> GetIconAsync(uint foreground = 0)
    {
        if (_parsed) return await base.GetIconAsync(foreground);

        if (await ParseIconAsync() is not { } icon) return null;
        
        // TODO : avalonia
        //await Dispatcher.UIThread.InvokeAsync(
        //    ()=>SetSource(XamlWriter.Save(icon)),XamlTools.Priority2);
        //_parsed = true;

        return icon;
    }

    public override async Task<string> GetTemplateAsync(uint foreground = 0)
    {
        while (!_parsed)
        {
            await GetIconAsync(foreground);
        }
        return await base.GetTemplateAsync(foreground);
    }

}