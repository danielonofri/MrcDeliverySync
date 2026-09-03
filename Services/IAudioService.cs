using System.Threading.Tasks;

namespace MrcDeliverySync.Services
{
    public interface IAudioService
    {
        ValueTask PlayNewOrderSoundAsync();
        ValueTask StartNewOrderLoopAsync(int intervalSeconds = 10);
        ValueTask StopNewOrderLoopAsync();
        ValueTask PlayCriticalDelaySoundAsync();
    }
}