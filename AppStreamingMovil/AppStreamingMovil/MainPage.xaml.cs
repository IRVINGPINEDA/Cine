namespace AppStreamingMovil
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Sesion(object? sender, EventArgs e)
        {
            var correo = correoCliente?.Text?.Trim();
            var contrasena = contrasenaCliente?.Text ?? string.Empty;

            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
            {
                await DisplayAlert("Error", "Por favor ingresa correo y contraseña.", "Aceptar");
                return;
            }

            // Validación básica de correo
            if (!correo.Contains("@") || !correo.Contains("."))
            {
                await DisplayAlert("Error", "Ingresa un correo válido.", "Aceptar");
                return;
            }

            // Aquí iría la lógica de autenticación (API, base de datos, etc.)
            // Por ahora simulamos inicio de sesión exitoso si la contraseña tiene al menos 4 caracteres
            if (contrasena.Length < 4)
            {
                await DisplayAlert("Error", "La contraseña es demasiado corta.", "Aceptar");
                return;
            }

            await DisplayAlert("Bienvenido", $"Sesión iniciada como {correo}", "Aceptar");
        }

        private void ShowPasswordChanged(object? sender, CheckedChangedEventArgs e)
        {
            if (contrasenaCliente != null)
            {
                contrasenaCliente.IsPassword = !e.Value;
            }
        }
    }
}
