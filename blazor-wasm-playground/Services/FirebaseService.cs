using System.Text.Json.Serialization;
using blazor_wasm_playground.Models;
using Microsoft.JSInterop;

namespace blazor_wasm_playground.Services
{
    public class FirebaseService
    {
        private readonly IJSRuntime _jsRuntime;

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
