using Microsoft.Maui.Controls;

namespace JiniTechApp.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private async void OnCreateImageClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ImagePage());
        }

        private async void OnCreateVideoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new VideoPage());
        }
    }
}
