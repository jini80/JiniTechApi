using Microsoft.Maui.Controls;
using JiniTechApp.Services;

namespace JiniTechApp.Views
{
    public partial class ImagePage : ContentPage
    {
        private readonly ApiService _apiService = new ApiService();

        public ImagePage()
        {
            InitializeComponent();
        }

        private async void OnGenerateImageClicked(object sender, EventArgs e)
        {
            var prompt = PromptEntry.Text;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                await DisplayAlert("Error", "Please enter a prompt", "OK");
                return;
            }

            try
            {
                var imageUrl = await _apiService.CreateImageAsync(prompt);
                GeneratedImage.Source = ImageSource.FromUri(new Uri(imageUrl));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
