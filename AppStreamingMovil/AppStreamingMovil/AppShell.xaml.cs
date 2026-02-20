namespace AppStreamingMovil
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Registrar rutas para navegación por nombre
            Routing.RegisterRoute(nameof(MoviesPage), typeof(MoviesPage));
        }
    }
}
