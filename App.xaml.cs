using Microsoft.UI.Xaml;

namespace homarr {
    public partial class App : Application {
        public App() {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {
            m_window = new MainWindow();
            m_window.AppWindow.Title = "Homarr";
            m_window.AppWindow.SetIcon("Assets/Logo.ico");
            m_window.Activate();
        }

        private Window m_window;
    }
}
