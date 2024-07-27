using System.Windows;
using System.Windows.Controls;

namespace SolarSystem
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainUserControl : UserControl
    {
        OrbitsCalculator _data = new OrbitsCalculator();
        public MainUserControl()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
