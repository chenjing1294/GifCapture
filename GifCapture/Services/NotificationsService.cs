using System;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GifCapture.Services
{
    public static class NotificationsService
    {
        public static void Notice(string title, string subTitle, string imageUri = null, Dictionary<string, string> args = null)
        {
            ToastContentBuilder builder = new ToastContentBuilder();
            if (args != null)
            {
                foreach (KeyValuePair<string, string> pair in args)
                {
                    builder.AddArgument(pair.Key, pair.Value);
                }
            }

            builder.AddText(title);
            builder.AddText(subTitle);
            if (!string.IsNullOrWhiteSpace(imageUri))
            {
                builder.AddInlineImage(new Uri(imageUri));
            }

            builder.Show();
        }

        public static void OpenFile(string fullNamePath)
        {
            System.Diagnostics.Process.Start(fullNamePath);
        }
    }
}