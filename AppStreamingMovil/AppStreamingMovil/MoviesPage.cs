using System.Collections.ObjectModel;

namespace AppStreamingMovil
{
    // Code-behind simple page (using C# page instead of XAML to avoid XAML parsing issues)
    public class MoviesPage : ContentPage
    {
        public ObservableCollection<Movie> Movies { get; } = new ObservableCollection<Movie>();

        public MoviesPage()
        {
            Title = "Películas";

            // Datos de ejemplo
            Movies.Add(new Movie
            {
                Title = "Jeepers Creepers - Terror",
                ShortDescription = "Trish y Darry, dos hermanos universitarios, viajan por carretera...",
                ImageSource = "dotnet_bot.png" // sustituir por imagen real en Resources/Images
            });

            Movies.Add(new Movie
            {
                Title = "¿Y dónde están las rubias? - Comedia",
                ShortDescription = "Dos torpes agentes del FBI se hacen pasar por dos chicas...",
                ImageSource = "dotnet_bot.png"
            });

            // Construir UI en código para evitar problemas de XAML
            var collection = new CollectionView
            {
                ItemsSource = Movies,
                SelectionMode = SelectionMode.None,
                ItemTemplate = new DataTemplate(() =>
                {
                    var frame = new Frame { CornerRadius = 10, HasShadow = true, Padding = 8, Margin = new Thickness(0,4), BorderColor = Colors.LightGray };

                    var title = new Label { FontAttributes = FontAttributes.Bold };
                    title.SetBinding(Label.TextProperty, "Title");

                    var img = new Image { WidthRequest = 80, HeightRequest = 120, Aspect = Aspect.AspectFill };
                    img.SetBinding(Image.SourceProperty, "ImageSource");

                    var descFrame = new Frame { CornerRadius = 10, Padding = 8, BorderColor = Color.FromArgb("#E6E6E6"), Margin = new Thickness(8,0,0,0) };
                    var desc = new Label { LineBreakMode = LineBreakMode.WordWrap };
                    desc.SetBinding(Label.TextProperty, "ShortDescription");
                    descFrame.Content = desc;

                    var grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                    grid.Add(img);
                    Grid.SetColumn(descFrame, 1);
                    grid.Add(descFrame);

                    var btn = new Button { Text = "Ver", BackgroundColor = Color.FromArgb("#007AFF"), TextColor = Colors.White, HorizontalOptions = LayoutOptions.End };
                    btn.Clicked += OnViewClicked;

                    var stack = new VerticalStackLayout { Spacing = 8 };
                    stack.Add(title);
                    stack.Add(grid);
                    stack.Add(btn);

                    frame.Content = stack;

                    return frame;
                })
            };

            var layout = new VerticalStackLayout { Padding = 12, Spacing = 12 };
            layout.Add(new Label { Text = "Películas", FontAttributes = FontAttributes.Bold, FontSize = 20, HorizontalOptions = LayoutOptions.Center });
            layout.Add(collection);

            Content = new ScrollView { Content = layout };
        }

        private async void OnViewClicked(object? sender, EventArgs e)
        {
            if (sender is Button b && b.BindingContext is Movie m)
            {
                await DisplayAlert(m.Title, m.ShortDescription, "Cerrar");
            }
            else
            {
                await DisplayAlert("Detalle", "No se encontró la película.", "Cerrar");
            }
        }
    }
}