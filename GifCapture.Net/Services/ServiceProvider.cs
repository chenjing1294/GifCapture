namespace GifCapture.Services
{
    public static class ServiceProvider
    {
        public static readonly IPlatformServices IPlatformServices;

        static ServiceProvider()
        {
            IPlatformServices = new WindowsPlatformServices();
        }
    }
}