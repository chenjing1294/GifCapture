using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Foundation.Collections;
using GifCapture.Services;
using GifCapture.Windows;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GifCapture
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            RegisterEvents();
            // Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
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
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            //unload notifications
            ToastNotificationManagerCompat.Uninstall();
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

        private MainWindow _mainWindow;

        private void OnAppStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                _mainWindow = new MainWindow(true);
                _mainWindow.Show();
            }
            else if (e.Args.Length == 1 && e.Args[0].Equals("RedisantToolbox"))
            {
                _mainWindow = new MainWindow(false);
                Task.Run(Pipe);
            }
            else
            {
                Shutdown();
            }
        }

        #region 与 ToolBox 通信

        private NamedPipeClientStream _namedPipeClientStream;
        private readonly byte[] _receivedData = new byte[4];

        private void Pipe()
        {
            //我们定义了一个Client的对象，.代表是当前计算机，以及和服务端一样的管道名称，同样定义为开启异步，以及是输入输出类型的。然后异步的去链接服务端
            _namedPipeClientStream = new NamedPipeClientStream(".", "RedisantToolBoxPipe", PipeDirection.InOut);
            //链接服务端
            _namedPipeClientStream.Connect();

            //发送消息到服务器
            var data = Encoding.UTF8.GetBytes("hell");
            _namedPipeClientStream.Write(data, 0, data.Length);
            _namedPipeClientStream.Flush();

            //异步等待收到服务端发送的消息
            while (true)
            {
                int read = _namedPipeClientStream.Read(_receivedData, 0, _receivedData.Length);
                if (read == 0)
                {
                    Dispatcher.Invoke(() => _mainWindow.StopRecord_OnClick(null, null));
                    Thread.Sleep(1000);
                    _namedPipeClientStream.Dispose();
                    Dispatcher.Invoke(() => _mainWindow.MenuExit_Click(null, null));
                    break;
                }

                string action = Encoding.UTF8.GetString(_receivedData);
                if (action.Equals("stop")) //停止捕获
                {
                    Dispatcher.Invoke(() => _mainWindow.StopRecord_OnClick(null, null));
                }
                else if (action.Equals("4___")) //捕获窗口
                {
                    Dispatcher.Invoke(() => _mainWindow.GifWindow_OnClick(null, null));
                }
                else if (action.Equals("5___")) //捕获区域
                {
                    Dispatcher.Invoke(() => _mainWindow.GifRegion_OnClick(null, null));
                }
                else if (action.Equals("6___")) //捕获屏幕
                {
                    Dispatcher.Invoke(() => _mainWindow.GifScreen_OnClick(null, null));
                }
            }
        }

        #endregion
    }
}