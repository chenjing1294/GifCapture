using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GifCapture.Base;
using GifCapture.Services;
using GifCapture.ViewModels;
using Reactive.Bindings;
using Window = System.Windows.Window;

namespace GifCapture.Net.Windows
{
    public partial class ScreenPickerWindow : Window
    {
        const double Scale = 0.15;
        public ObservableCollection<ScreenPickerViewModel> ScreenPickerViewModels { get; } = new ObservableCollection<ScreenPickerViewModel>();
        public ICommand SelectScreenCommand { get; }


        ScreenPickerWindow()
        {
            this.Deactivated += OnDeactivated;
            SelectScreenCommand = new ReactiveCommand<IScreen>()
                .WithSubscribe(m =>
                {
                    _onClosing = true;
                    SelectedScreen = m;
                    Close();
                });

            InitializeComponent();
            var platformServices = ServiceProvider.IPlatformServices;
            var screens = platformServices.EnumerateScreens().ToArray();

            foreach (var screen in screens)
            {
                ScreenPickerViewModels.Add(new ScreenPickerViewModel(screen, Scale));
            }
        }


        private bool _onClosing = false;

        private void OnDeactivated(object sender, EventArgs e)
        {
            if (!_onClosing)
            {
                CloseClick(null, null);
            }
        }

        public IScreen SelectedScreen { get; private set; }

        void CloseClick(object sender, RoutedEventArgs e)
        {
            _onClosing = true;
            SelectedScreen = null;
            Close();
        }

        public static IScreen PickScreen()
        {
            var picker = new ScreenPickerWindow();
            picker.ShowDialog();
            return picker.SelectedScreen;
        }
    }
}