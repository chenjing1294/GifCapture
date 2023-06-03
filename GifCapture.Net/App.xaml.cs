using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Foundation.Collections;
using GifCapture.Services;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GifCapture.Net
{
    public partial class App : Application
    {
        private bool _registered = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            #region terminate if there's any existing instance

            Mutex procMutex = new System.Threading.Mutex(true, "_REDISANT_TOOLBOX", out bool result);
            if (!result)
            {
                Application.Current.Shutdown(0);
                return;
            }

            procMutex.ReleaseMutex();

            #endregion

            RegisterEvents();

            // click on the notification tip
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // Obtain any user input (text boxes, menu selections) from the notification
                ValueSet userInput = toastArgs.UserInput;

                // Need to dispatch to UI thread if performing UI operations
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (args.Contains("imagePath"))
                    {
                        string s = args.Get("imagePath");
                        NotificationsService.OpenFile(s);
                    }
                });
            };
            _registered = true;
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (_registered)
            {
                //unload notifications
                ToastNotificationManagerCompat.Uninstall();
            }
        }

        #region 异常处理

        private void RegisterEvents()
        {
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            //UI线程未捕获异常处理事件（UI主线程）
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                if (e.Exception is Exception exception)
                {
                    HandleException(exception);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                e.SetObserved();
            }
        }

        //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)      
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception exception)
                {
                    HandleException(exception);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        //UI线程未捕获异常处理事件（UI主线程）
        private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                e.Handled = true;
            }
        }

        private static void HandleException(Exception ex)
        {
            MessageBox.Show(ex.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}