using JiniTechApp.Views;

namespace JiniTechApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        // ✅ Modern .NET 9 pattern (no obsolete warning)
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(new HomePage()));
        }
    }
}
