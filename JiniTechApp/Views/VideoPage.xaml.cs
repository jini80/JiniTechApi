using Microsoft.Maui.Controls;
using JiniTechApp.Services;

namespace JiniTechApp.Views
{
    public partial class VideoPage : ContentPage
    {
        private readonly ApiService _apiService = new ApiService();

        public VideoPage()
        {
            InitializeComponent();
        }

        private async void OnGenerateVideoClicked(object sender, EventArgs e)
        {
            var prompt = PromptEntry.Text;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                await DisplayAlert("Error", "Please enter a prompt", "OK");
                return;
            }

            try
            {
                StatusLabel.Text = "Generating video...";
                var videoUrl = await _apiService.CreateVideoAsync(prompt);
                StatusLabel.Text = $"Video ready: {videoUrl}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
