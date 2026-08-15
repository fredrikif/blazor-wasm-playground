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
        private DotNetObjectReference<FirebaseService>? _todoDotNetRef;
        private string? _todoSubscriptionId;

        public event Action<List<HandlelisteItem>>? HandlelisteChanged;
        public event Action<List<HandlelisteItem>>? TodolistChanged;

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

        public ValueTask<List<HandlelisteItem>> GetTodolistAsync()
            => _jsRuntime.InvokeAsync<List<HandlelisteItem>>("firebaseInterop.getTodolist");

        public ValueTask<HandlelisteItem> AddHandlelisteItemAsync(HandlelisteItemCreateRequest request)
            => _jsRuntime.InvokeAsync<HandlelisteItem>("firebaseInterop.addHandlelisteItem", request);

        public ValueTask<HandlelisteItem> AddTodolistItemAsync(HandlelisteItemCreateRequest request)
            => _jsRuntime.InvokeAsync<HandlelisteItem>("firebaseInterop.addTodolistItem", request);

        public ValueTask<HandlelisteItem> UpdateHandlelisteItemPinnedAsync(string id, bool pinned)
            => _jsRuntime.InvokeAsync<HandlelisteItem>("firebaseInterop.updateHandlelisteItemPinned", id, pinned);

        public ValueTask<HandlelisteItem> UpdateTodolistItemPinnedAsync(string id, bool pinned)
            => _jsRuntime.InvokeAsync<HandlelisteItem>("firebaseInterop.updateTodolistItemPinned", id, pinned);

        public ValueTask DeleteHandlelisteItemAsync(string id)
            => _jsRuntime.InvokeVoidAsync("firebaseInterop.deleteHandlelisteItem", id);

        public ValueTask DeleteTodolistItemAsync(string id)
            => _jsRuntime.InvokeVoidAsync("firebaseInterop.deleteTodolistItem", id);

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

        public async ValueTask StartTodolistSubscriptionAsync()
        {
            if (_todoSubscriptionId != null)
            {
                return;
            }

            _todoDotNetRef = DotNetObjectReference.Create(this);
            try
            {
                _todoSubscriptionId = await _jsRuntime.InvokeAsync<string>("firebaseInterop.subscribeTodolist", _todoDotNetRef!);
            }
            catch
            {
                _todoSubscriptionId = null;
            }
        }

        public async ValueTask StopTodolistSubscriptionAsync()
        {
            if (string.IsNullOrEmpty(_todoSubscriptionId))
                return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("firebaseInterop.unsubscribeTodolist", _todoSubscriptionId);
            }
            catch
            {
                // ignore
            }

            _todoSubscriptionId = null;
            _todoDotNetRef?.Dispose();
            _todoDotNetRef = null;
        }

        [JSInvokable]
        public Task HandlelisteSnapshot(List<HandlelisteItem> updatedItems)
        {
            HandlelisteChanged?.Invoke(updatedItems);
            return Task.CompletedTask;
        }

        [JSInvokable]
        public Task TodolistSnapshot(List<HandlelisteItem> updatedItems)
        {
            TodolistChanged?.Invoke(updatedItems);
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
