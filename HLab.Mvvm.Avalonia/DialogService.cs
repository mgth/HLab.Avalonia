using HLab.Core;
using HLab.Mvvm.Annotations;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;

namespace HLab.Mvvm.Avalonia;

public class DialogService : Service, IDialogService
{
    static bool? ShowMessage(string text, string caption, ButtonEnum button, string icon)
    {
        var task = Task.Run(async () => await ShowMessageAsync(text, caption, button, icon));
        task.Wait();
        return task.Result;
    }

    static async Task<bool?> ShowMessageAsync(string text, string caption, ButtonEnum button, string icon)
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard(caption, text,
                ButtonEnum.YesNo);

        var result = await box.ShowAsync();


        //if (!Enum.TryParse("Active", out MessageBoxImage img))
        //    img = MessageBoxImage.Information;

        // var result = MessageBox.Show(text,caption,MessageBoxButton.OK, img);
        return result switch
        {
            ButtonResult.None => null,
            ButtonResult.Ok => true,
            ButtonResult.Cancel => null,
            ButtonResult.Yes => true,
            ButtonResult.No => false,
            _ => null,
        };
    }
    public void ShowMessageOk(string text, string caption, string icon)
        => ShowMessage(text, caption, ButtonEnum.Ok, icon);

    public bool ShowMessageOkCancel(string text, string caption, string icon)
        => ShowMessage(text, caption, ButtonEnum.OkCancel, icon)??false;

    public bool ShowMessageYesNo(string text, string caption, string icon)
        => ShowMessage(text, caption, ButtonEnum.YesNo, icon) ?? false;

    public bool? ShowMessageYesNoCancel(string text, string caption, string icon)
        => ShowMessage(text, caption, ButtonEnum.YesNoCancel, icon);

    public async Task ShowMessageOkAsync(string text, string caption, string icon)
        => await ShowMessageAsync(text, caption, ButtonEnum.Ok, icon);

    public async Task<bool> ShowMessageOkCancelAsync(string text, string caption, string icon)
        => await ShowMessageAsync(text, caption, ButtonEnum.OkCancel, icon)??false;

    public async Task<bool> ShowMessageYesNoAsync(string text, string caption, string icon)
        => await ShowMessageAsync(text, caption, ButtonEnum.YesNo, icon) ?? false;

    public async Task<bool?> ShowMessageYesNoCancelAsync(string text, string caption, string icon)
        => await ShowMessageAsync(text, caption, ButtonEnum.YesNoCancel, icon);
}