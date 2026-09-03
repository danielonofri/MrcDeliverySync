using System.Threading.Tasks;
using Microsoft.JSInterop;
using MrcDeliverySync.Services;

namespace MrcDeliverySync.Services
{
    public class AudioService : IAudioService
    {
        private readonly IJSRuntime _jsRuntime;

        public AudioService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async ValueTask PlayNewOrderSoundAsync()
        {
            await _jsRuntime.InvokeVoidAsync("playNewOrderAlert");
        }

        public async ValueTask StartNewOrderLoopAsync(int intervalSeconds = 10)
        {
            await _jsRuntime.InvokeVoidAsync("startNewOrderLoop", intervalSeconds);
        }

        public async ValueTask StopNewOrderLoopAsync()
        {
            await _jsRuntime.InvokeVoidAsync("stopNewOrderLoop");
        }

        public async ValueTask PlayCriticalDelaySoundAsync()
        {
            await _jsRuntime.InvokeVoidAsync("playCriticalDelayAlert");
        }
    }
}