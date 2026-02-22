namespace AppStreamingMovil
{
    public partial class AppShell : Shell
    {
        public AppShell(MainPage mainPage)
        {
            InitializeComponent();

            Items.Add(new ShellContent
            {
                Title = "Catalogo",
                Route = nameof(MainPage),
                Content = mainPage
            });
        }
    }
}
