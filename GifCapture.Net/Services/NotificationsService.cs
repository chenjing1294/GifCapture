using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GifCapture.Services
{
    public static class NotificationsService
    {
        public static void Notice(string title, string subTitle, string imageUri = null, Dictionary<string, string> args = null)
        {
            try
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
            catch (Exception e)
            {
                //ignore
            }
        }

        public static void OpenFile(string fullNamePath)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(fullNamePath)
            {
                UseShellExecute = true
            };
            process.Start();
        }
    }
}