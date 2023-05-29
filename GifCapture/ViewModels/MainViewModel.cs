namespace GifCapture.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private bool _recoding = false;

        public bool Recoding
        {
            get => _recoding;
            set => Set(ref _recoding, value);
        }

        public readonly int Fps = 10;

        private int _elapsedSeconds;

        public int ElapsedSeconds
        {
            get => _elapsedSeconds;
            set => Set(ref _elapsedSeconds, value);
        }
    }
}