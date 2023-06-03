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

        private bool _includeCursor = false;

        public bool IncludeCursor
        {
            get => _includeCursor;
            set => Set(ref _includeCursor, value);
        }

        private int _delayIndex = 0;

        public int DelayIndex
        {
            get => _delayIndex;
            set => Set(ref _delayIndex, value);
        }

        private int _quality = 0;

        public int Quality
        {
            get => _quality;
            set => Set(ref _quality, value);
        }
    }
}