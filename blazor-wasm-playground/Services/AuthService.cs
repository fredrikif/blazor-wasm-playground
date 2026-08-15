using blazor_wasm_playground.Models;

namespace blazor_wasm_playground.Services
{
    public class AuthService
    {
        private readonly FirebaseService _firebaseService;

        public AuthService(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        public FirebaseUserInfo? CurrentUser { get; private set; }

        public bool IsSignedIn => CurrentUser != null;

        public event Action? AuthStateChanged;

        public async Task InitializeAsync()
        {
            await _firebaseService.InitializeAsync();
            CurrentUser = await _firebaseService.GetCurrentUserAsync();
            if (IsSignedIn)
            {
                await _firebaseService.StartHandlelisteSubscriptionAsync();
                await _firebaseService.StartTodolistSubscriptionAsync();
            }
            AuthStateChanged?.Invoke();
        }

        public async Task SignInAsync(string email, string password)
        {
            CurrentUser = await _firebaseService.SignInWithEmailPasswordAsync(email, password);
            if (IsSignedIn)
            {
                await _firebaseService.StartHandlelisteSubscriptionAsync();
                await _firebaseService.StartTodolistSubscriptionAsync();
            }
            AuthStateChanged?.Invoke();
        }

        public async Task RegisterAsync(string email, string password)
        {
            CurrentUser = await _firebaseService.RegisterWithEmailPasswordAsync(email, password);
            if (IsSignedIn)
            {
                await _firebaseService.StartHandlelisteSubscriptionAsync();
                await _firebaseService.StartTodolistSubscriptionAsync();
            }
            AuthStateChanged?.Invoke();
        }

        public async Task SignOutAsync()
        {
            await _firebaseService.SignOutAsync();
            CurrentUser = null;
            await _firebaseService.StopHandlelisteSubscriptionAsync();
            await _firebaseService.StopTodolistSubscriptionAsync();
            AuthStateChanged?.Invoke();
        }
    }
}
