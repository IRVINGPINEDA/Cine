using AppStreamingMovil.Services;

namespace AppStreamingMovil
{
    public partial class MainPage : ContentPage
    {
        private readonly IMobileApiClient _apiClient;
        private bool _isBusy;

        public MainPage(IMobileApiClient apiClient)
        {
            InitializeComponent();
            _apiClient = apiClient;
            SetAuthenticatedState(_apiClient.HasToken);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_apiClient.HasToken)
            {
                await LoadMoviesAsync();
            }
        }

        private async void Sesion(object? sender, EventArgs e)
        {
            var correo = correoCliente?.Text?.Trim() ?? string.Empty;
            var contrasena = contrasenaCliente?.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                await DisplayAlertAsync("Error", "Ingresa correo y contrasena.", "Aceptar");
                return;
            }

            if (!correo.Contains('@') || !correo.Contains('.'))
            {
                await DisplayAlertAsync("Error", "Ingresa un correo valido.", "Aceptar");
                return;
            }

            await RunBusyAsync(async () =>
            {
                var result = await _apiClient.LoginAsync(correo, contrasena);

                if (!result.IsSuccess)
                {
                    await DisplayAlertAsync("Error", result.ErrorMessage ?? "No fue posible iniciar sesion.", "Aceptar");
                    return;
                }

                SetAuthenticatedState(true);
                await LoadMoviesCoreAsync();
            });
        }

        private async Task LoadMoviesAsync()
        {
            await RunBusyAsync(LoadMoviesCoreAsync);
        }

        private async Task LoadMoviesCoreAsync()
        {
            try
            {
                var movies = await _apiClient.GetMoviesAsync();
                moviesCollectionView.ItemsSource = movies;
                SetAuthenticatedState(true);
            }
            catch (UnauthorizedAccessException ex)
            {
                SetAuthenticatedState(false);
                moviesCollectionView.ItemsSource = null;
                await DisplayAlertAsync("Sesion", ex.Message, "Aceptar");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo cargar el catalogo: {ex.Message}", "Aceptar");
            }
        }

        private async void CargarCatalogoClicked(object? sender, EventArgs e)
        {
            await LoadMoviesAsync();
        }

        private async void CerrarSesionClicked(object? sender, EventArgs e)
        {
            await _apiClient.LogoutAsync();
            moviesCollectionView.ItemsSource = null;
            SetAuthenticatedState(false);
            contrasenaCliente.Text = string.Empty;
            await DisplayAlertAsync("Sesion", "Se cerro la sesion correctamente.", "Aceptar");
        }

        private async void RegistrarseClicked(object? sender, EventArgs e)
        {
            var firstName = await DisplayPromptAsync("Registro", "Nombre", "Continuar", "Cancelar", keyboard: Keyboard.Text);
            if (string.IsNullOrWhiteSpace(firstName))
            {
                return;
            }

            var lastNamePaternal = await DisplayPromptAsync("Registro", "Apellido paterno", "Continuar", "Cancelar", keyboard: Keyboard.Text);
            if (string.IsNullOrWhiteSpace(lastNamePaternal))
            {
                return;
            }

            var lastNameMaternal = await DisplayPromptAsync("Registro", "Apellido materno (opcional)", "Continuar", "Omitir", keyboard: Keyboard.Text);

            var email = await DisplayPromptAsync("Registro", "Correo", "Continuar", "Cancelar", keyboard: Keyboard.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            var password = await DisplayPromptAsync(
                "Registro",
                "Contrasena (min 8, mayuscula, minuscula, numero y simbolo)",
                "Registrarme",
                "Cancelar",
                placeholder: "********",
                maxLength: 64,
                keyboard: Keyboard.Text);

            if (string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            await RunBusyAsync(async () =>
            {
                var result = await _apiClient.RegisterAsync(
                    firstName.Trim(),
                    lastNamePaternal.Trim(),
                    string.IsNullOrWhiteSpace(lastNameMaternal) ? null : lastNameMaternal.Trim(),
                    email.Trim(),
                    password);

                if (!result.IsSuccess)
                {
                    await DisplayAlertAsync("Registro", result.ErrorMessage ?? "No se pudo registrar la cuenta.", "Aceptar");
                    return;
                }

                correoCliente.Text = email.Trim();
                contrasenaCliente.Text = string.Empty;
                SetAuthenticatedState(true);
                await LoadMoviesCoreAsync();
            });
        }

        private async void TrailerClicked(object? sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not string trailerUrl)
            {
                return;
            }

            if (!Uri.TryCreate(trailerUrl, UriKind.Absolute, out var trailerUri))
            {
                await DisplayAlertAsync("Trailer", "La URL del trailer no es valida.", "Aceptar");
                return;
            }

            try
            {
                await Launcher.Default.OpenAsync(trailerUri);
            }
            catch (Exception)
            {
                await DisplayAlertAsync("Trailer", "No fue posible abrir el trailer.", "Aceptar");
            }
        }

        private void ShowPasswordChanged(object? sender, CheckedChangedEventArgs e)
        {
            if (contrasenaCliente != null)
            {
                contrasenaCliente.IsPassword = !e.Value;
            }
        }

        private void SetAuthenticatedState(bool isAuthenticated)
        {
            loginSection.IsVisible = !isAuthenticated;
            catalogSection.IsVisible = isAuthenticated;
        }

        private async Task RunBusyAsync(Func<Task> action)
        {
            if (_isBusy)
            {
                return;
            }

            _isBusy = true;
            loadingIndicatorContainer.IsVisible = true;
            loadingIndicator.IsVisible = true;
            loadingIndicator.IsRunning = true;
            iniciarSesion.IsEnabled = false;
            registrarseButton.IsEnabled = false;

            try
            {
                await action();
            }
            finally
            {
                _isBusy = false;
                loadingIndicator.IsRunning = false;
                loadingIndicator.IsVisible = false;
                loadingIndicatorContainer.IsVisible = false;
                iniciarSesion.IsEnabled = true;
                registrarseButton.IsEnabled = true;
            }
        }
    }
}
