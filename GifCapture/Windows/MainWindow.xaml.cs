using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AnimatedGif;
using GifCapture.Base;
using GifCapture.Gif;
using GifCapture.Images;
using GifCapture.Screen;
using GifCapture.Services;
using GifCapture.Utils;
using GifCapture.ViewModels;
using Rectangle = System.Drawing.Rectangle;
using Window = System.Windows.Window;

namespace GifCapture.Windows
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow(bool showTaskbarIcon = true)
        {
            Instance = this;
            InitializeComponent();
            if (!showTaskbarIcon)
            {
                TaskbarIcon.Visibility = Visibility.Hidden;
            }
        }


        private async Task DelayAsync(int delayIndex)
        {
            switch (delayIndex)
            {
                case 0:
                    return;
                case 1:
                    await Task.Delay(2 * 1000);
                    break;
                case 2:
                    await Task.Delay(4 * 1000);
                    break;
                case 3:
                    await Task.Delay(6 * 1000);
                    break;
                case 4:
                    await Task.Delay(8 * 1000);
                    break;
            }
        }

        #region 截图

        private async void CaptureWindow_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                await DelayAsync(mainViewModel.DelayIndex);
                IWindow window = VideoSourcePickerWindow.PickWindow();
                if (window != null)
                {
                    var platformServices = ServiceProvider.IPlatformServices;
                    IBitmapImage bitmapImage = window.Handle == platformServices.DesktopWindow.Handle
                        ? ScreenShot.Capture(false)
                        : ScreenShot.Capture(window.Rectangle, mainViewModel.IncludeCursor);

                    if (bitmapImage != null)
                    {
                        await SaveScreenShotAsync(bitmapImage);
                    }
                }
            }
        }

        private async void CaptureRegion_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                await DelayAsync(mainViewModel.DelayIndex);
                Rectangle? region = RegionPickerWindow.PickRegion();
                if (region != null)
                {
                    await SaveScreenShotAsync(ScreenShot.Capture(region.Value, mainViewModel.IncludeCursor));
                }
            }
        }

        private async void CaptureScreen_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                await DelayAsync(mainViewModel.DelayIndex);
                IScreen screen = ScreenPickerWindow.PickScreen();
                if (screen != null)
                {
                    IBitmapImage img = ScreenShot.Capture(screen.Rectangle, mainViewModel.IncludeCursor);
                    await SaveScreenShotAsync(img);
                }
            }
        }

        private async Task SaveScreenShotAsync(IBitmapImage bmp)
        {
            if (bmp != null)
            {
                await Task.Run(() =>
                {
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    string file = Path.Combine(desktop, $"{date}.png");
                    bmp.Save(file, ImageFormat.Png);
                    bmp.Dispose();
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args.Add("imagePath", file);
                    NotificationsService.Notice(Properties.Resources.ScreenshotSaved, $"{Properties.Resources.SavedPath}{file}", file, args);
                });
            }
        }

        #endregion

        #region 录屏

        private RecordBarWindow _barWindow = null;
        private CancellationTokenSource _cts = null;

        public async void GifWindow_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                if (!mainViewModel.Recoding)
                {
                    if (_cts != null)
                    {
                        _cts.Cancel();
                        _cts.Dispose();
                    }

                    await DelayAsync(mainViewModel.DelayIndex);
                    IWindow target = VideoSourcePickerWindow.PickWindow();
                    if (target == null)
                    {
                        return;
                    }

                    Rectangle rectangle = target.Rectangle;
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    string file = Path.Combine(desktop, $"{date}.gif");
                    _cts = new CancellationTokenSource();
                    mainViewModel.Recoding = true;
                    _barWindow?.Close();
                    _barWindow = new RecordBarWindow(mainViewModel, rectangle);
                    _barWindow.Show();
                    await Task.Run(() => { ToRecordFaster(rectangle, file, 1000 / mainViewModel.Fps, _cts.Token, mainViewModel); });
                }
            }
        }

        public async void GifRegion_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                if (!mainViewModel.Recoding)
                {
                    if (_cts != null)
                    {
                        _cts.Cancel();
                        _cts.Dispose();
                    }

                    await DelayAsync(mainViewModel.DelayIndex);
                    Rectangle? rectangle = RegionPickerWindow.PickRegion();
                    if (rectangle == null)
                        return;
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    string file = Path.Combine(desktop, $"{date}.gif");
                    _cts = new CancellationTokenSource();
                    mainViewModel.Recoding = true;
                    _barWindow?.Close();
                    _barWindow = new RecordBarWindow(mainViewModel, rectangle.Value);
                    _barWindow.Show();
                    await Task.Run(() => ToRecordFaster(rectangle.Value, file, 1000 / mainViewModel.Fps, _cts.Token, mainViewModel));
                }
            }
        }

        public async void GifScreen_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                if (!mainViewModel.Recoding)
                {
                    if (_cts != null)
                    {
                        _cts.Cancel();
                        _cts.Dispose();
                    }

                    await DelayAsync(mainViewModel.DelayIndex);
                    IScreen target = ScreenPickerWindow.PickScreen();
                    if (target == null)
                    {
                        return;
                    }

                    Rectangle rectangle = target.Rectangle;
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    string file = Path.Combine(desktop, $"{date}.gif");
                    _cts = new CancellationTokenSource();
                    mainViewModel.Recoding = true;
                    _barWindow?.Close();
                    _barWindow = new RecordBarWindow(mainViewModel, rectangle);
                    _barWindow.Show();
                    await Task.Run(() => ToRecordFaster(rectangle, file, 1000 / mainViewModel.Fps, _cts.Token, mainViewModel));
                }
            }
        }

        static void Sleep(int ms)
        {
            var sw = Stopwatch.StartNew();
            var sleepMs = ms - 16;
            if (sleepMs > 0)
            {
                Thread.Sleep(sleepMs);
            }

            while (sw.ElapsedMilliseconds < ms)
            {
                Thread.SpinWait(1);
            }
        }

        /// <summary>
        /// 占用大内存，帧率保证
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="gifPath"></param>
        /// <param name="DELAY"></param>
        /// <param name="ct"></param>
        /// <param name="mainViewModel"></param>
        void ToRecordSlower(Rectangle rectangle, string gifPath, int DELAY, CancellationToken ct, MainViewModel mainViewModel)
        {
            IBitmapImage bitmapImage = ScreenShot.Capture(rectangle);
            string tempPath = System.IO.Path.GetTempPath();
            string date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string imagePath = Path.Combine(tempPath, $"{date}.png");
            bitmapImage.Save(imagePath, ImageFormat.Png);
            bitmapImage.Dispose();

            BlockingCollection<GifFrame> bitmaps = new BlockingCollection<GifFrame>(1000);
            Task task1 = Task.Run(() =>
            {
                int delay = DELAY;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                using (RectangleProvider provider = new RectangleProvider(rectangle))
                {
                    TimeSpan prev = sw.Elapsed;
                    while (!ct.IsCancellationRequested)
                    {
                        delay = DELAY;
                        var curr = sw.Elapsed;
                        int usedt = (int) (curr - prev).TotalMilliseconds;
                        if (usedt < delay)
                        {
                            int a = delay - usedt;
                            Sleep(a);
                        }
                        else
                        {
                            delay = usedt;
                        }

                        prev = curr;
                        Bitmap img = provider.Capture(mainViewModel.IncludeCursor);

                        bitmaps.Add(new GifFrame()
                        {
                            Bitmap = img, Delay = delay
                        });

                        Dispatcher.Invoke(() => { mainViewModel.ElapsedSeconds = (int) sw.Elapsed.TotalSeconds; });
                    }

                    // Let consumer know we are done.
                    bitmaps.CompleteAdding();
                }

                sw.Stop();
            }, ct);

            Task task2 = Task.Run(() =>
            {
                using (var gifCreator = AnimatedGif.AnimatedGif.Create(gifPath, DELAY))
                {
                    while (!bitmaps.IsCompleted)
                    {
                        GifFrame frame = null;
                        try
                        {
                            frame = bitmaps.Take();
                        }
                        catch (InvalidOperationException)
                        {
                            //ignore
                        }

                        if (frame != null)
                        {
                            Debug.WriteLine($"DELAY={DELAY},delay={frame.Delay}ms");
                            gifCreator.AddFrame(frame.Bitmap, delay: frame.Delay, quality: GifQuality.Bit8);
                            frame.Bitmap.Dispose();
                        }
                    }
                }

                mainViewModel.Recoding = false;
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("imagePath", gifPath);
                NotificationsService.Notice(Properties.Resources.RecordingSaved, $"{Properties.Resources.SavedPath}{gifPath}", imagePath, args);
            }, ct);

            Task.WaitAll(task1, task2);
        }

        /// <summary>
        /// 不占用内存，但是帧率无法保证
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="gifPath"></param>
        /// <param name="DELAY"></param>
        /// <param name="ct"></param>
        /// <param name="mainViewModel"></param>
        void ToRecordFaster(Rectangle rectangle, string gifPath, int DELAY, CancellationToken ct, MainViewModel mainViewModel)
        {
            IBitmapImage bitmapImage = ScreenShot.Capture(rectangle);
            string tempPath = System.IO.Path.GetTempPath();
            string date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string imagePath = Path.Combine(tempPath, $"{date}.png");
            bitmapImage.Save(imagePath, ImageFormat.Png);
            bitmapImage.Dispose();


            Task task1 = Task.Run(() =>
            {
                int delay = DELAY;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                using (var gifCreator = AnimatedGif.AnimatedGif.Create(gifPath, DELAY))
                {
                    using (var provider = new RectangleProvider(rectangle))
                    {
                        TimeSpan prev = sw.Elapsed;
                        while (!ct.IsCancellationRequested)
                        {
                            delay = DELAY;
                            var curr = sw.Elapsed;
                            int usedt = (int) (curr - prev).TotalMilliseconds;
                            if (usedt < delay)
                            {
                                int a = delay - usedt;
                                Sleep(a);
                            }
                            else
                            {
                                delay = usedt;
                            }

                            prev = curr;
                            Bitmap img = provider.Capture(mainViewModel.IncludeCursor);

                            Debug.WriteLine($"DELAY={DELAY},delay={delay}ms");
                            gifCreator.AddFrame(img, delay: delay, quality: GifQuality.Bit8);
                            img.Dispose();
                            Dispatcher.Invoke(() => { mainViewModel.ElapsedSeconds = (int) sw.Elapsed.TotalSeconds; });
                        }
                    }
                }

                sw.Stop();
                mainViewModel.Recoding = false;
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("imagePath", gifPath);
                NotificationsService.Notice("录屏已保存", $"保存位置：{gifPath}", imagePath, args);
            }, ct);


            Task.WaitAll(task1);
        }


        public void StopRecord_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                if (mainViewModel.Recoding && _cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                    _barWindow?.Close();
                    _barWindow = null;
                }
            }
        }

        #endregion

        void SystemTray_TrayMouseDoubleClick(object sender, RoutedEventArgs args)
        {
            if (Visibility == Visibility.Visible)
            {
                Hide();
            }
            else
            {
                this.ShowAndFocus();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_toExit)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private bool _toExit = false;

        public void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            _toExit = true;
            Close();
        }

        private void GitHubHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(@"https://github.com/chenjing1294/GifCapture"));
        }

        private void WebSiteHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(Properties.Resources.WebSite));
        }
    }
}