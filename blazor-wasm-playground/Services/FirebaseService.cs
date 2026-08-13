using System.Text.Json.Serialization;
using blazor_wasm_playground.Models;
using Microsoft.JSInterop;

namespace blazor_wasm_playground.Services
{
    public class FirebaseService
    {
        private readonly IJSRuntime _jsRuntime;

        private DotNetObjectReference<FirebaseService>? _dotNetRef;
        private string? _subscriptionId;

        public event Action<List<HandlelisteItem>>? HandlelisteChanged;

        public FirebaseService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public ValueTask InitializeAsync()
            => _jsRuntime.InvokeVoidAsync("firebaseInterop.initialize");

        public ValueTask<FirebaseUserInfo?> GetCurrentUserAsync()
            => _jsRuntime.InvokeAsync<FirebaseUserInfo?>("firebaseInterop.getCurrentUser");

        public ValueTask<FirebaseUserInfo> SignInWithEmailPasswordAsync(string email, string password)
            => _jsRuntime.InvokeAsync<FirebaseUserInfo>("firebaseInterop.signInWithEmailPassword", email, password);
        public ValueTask<FirebaseUserInfo> RegisterWithEmailPasswordAsync(string email, string password)
            => _jsRuntime.InvokeAsync<FirebaseUserInfo>("firebaseInterop.createUserWithEmailPassword", email, password);
        public ValueTask SignOutAsync()
            => _jsRuntime.InvokeVoidAsync("firebaseInterop.signOut");

        public ValueTask<List<HandlelisteItem>> GetHandlelisteAsync()
            => _jsRuntime.InvokeAsync<List<HandlelisteItem>>("firebaseInterop.getHandleliste");

        public ValueTask<HandlelisteItem> AddHandlelisteItemAsync(HandlelisteItemCreateRequest request)
            => _jsRuntime.InvokeAsync<HandlelisteItem>("firebaseInterop.addHandlelisteItem", request);

        public ValueTask<HandlelisteItem> UpdateHandlelisteItemPinnedAsync(string id, bool pinned)
            => _jsRuntime.InvokeAsync<HandlelisteItem>("firebaseInterop.updateHandlelisteItemPinned", id, pinned);

        public ValueTask DeleteHandlelisteItemAsync(string id)
            => _jsRuntime.InvokeVoidAsync("firebaseInterop.deleteHandlelisteItem", id);

        // Manage realtime subscription from the service layer.
        public async ValueTask StartHandlelisteSubscriptionAsync()
        {
            if (_subscriptionId != null)
            {
                return;
            }

            _dotNetRef = DotNetObjectReference.Create(this);
            try
            {
                _subscriptionId = await _jsRuntime.InvokeAsync<string>("firebaseInterop.subscribeHandleliste", _dotNetRef!);
            }
            catch
            {
                // swallow subscription errors
                _subscriptionId = null;
            }
        }

        public async ValueTask StopHandlelisteSubscriptionAsync()
        {
            if (string.IsNullOrEmpty(_subscriptionId))
                return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("firebaseInterop.unsubscribeHandleliste", _subscriptionId);
            }
            catch
            {
                // ignore
            }

            _subscriptionId = null;
            _dotNetRef?.Dispose();
            _dotNetRef = null;
        }

        [JSInvokable]
        public Task HandlelisteSnapshot(List<HandlelisteItem> updatedItems)
        {
            HandlelisteChanged?.Invoke(updatedItems);
            return Task.CompletedTask;
        }
    }

    public class FirebaseUserInfo
    {
        [JsonPropertyName("uid")]
        public string? Uid { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }
    }

    public class HandlelisteItemCreateRequest
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("pinned")]
        public bool Pinned { get; set; }
    }
}
