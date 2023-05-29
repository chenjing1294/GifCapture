using System.Drawing;

namespace GifCapture.Base
{
    public interface IScreen
    {
        Rectangle Rectangle { get; }

        string DeviceName { get; }
    }
}