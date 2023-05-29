using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GifCapture.ViewModels
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged(propertyName);
        }

        protected void RaiseAllChanged()
        {
            RaisePropertyChanged("");
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;

            RaisePropertyChanged(propertyName);

            return true;
        }
    }
}