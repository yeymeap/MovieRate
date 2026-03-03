using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace MovieRate.Services;

public static class DialogService
{
    public static async Task<bool> ConfirmAsync(string title, string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams
            {
                ContentTitle = title,
                ContentMessage = message,
                ButtonDefinitions = ButtonEnum.YesNo,
                Icon = Icon.Warning,
                MinWidth = 500,
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowInCenter = true
            });

        var result = await box.ShowAsync();
        return result == ButtonResult.Yes;
    }
}