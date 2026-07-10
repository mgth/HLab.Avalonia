using HLab.Core;
using HLab.Mvvm.Annotations;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;

namespace HLab.Mvvm.Avalonia;

public class DialogService : Service, IDialogService
{
    static async Task<bool?> ShowMessageAsync(string text, string caption, ButtonEnum button, string icon)
    {
        if (!Enum.TryParse<Icon>(icon, true, out var boxIcon))
            boxIcon = Icon.None;

        var box = MessageBoxManager
            .GetMessageBoxStandard(caption, text, button, boxIcon);

        var result = await box.ShowAsync();

        return result switch
        {
            ButtonResult.Ok => true,
            ButtonResult.Yes => true,
            ButtonResult.No => false,
            _ => null,
        };
    }

    public async Task ShowMessageOkAsync(string text, string caption, string icon)
        => await ShowMessageAsync(text, caption, ButtonEnum.Ok, icon);

    public async Task<bool> ShowMessageOkCancelAsync(string text, string caption, string icon)
        => await ShowMessageAsync(text, caption, ButtonEnum.OkCancel, icon)??false;

    public async Task<bool> ShowMessageYesNoAsync(string text, string caption, string icon)
        => await ShowMessageAsync(text, caption, ButtonEnum.YesNo, icon) ?? false;

    public async Task<bool?> ShowMessageYesNoCancelAsync(string text, string caption, string icon)
        => await ShowMessageAsync(text, caption, ButtonEnum.YesNoCancel, icon);
}
