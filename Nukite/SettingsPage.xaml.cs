using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Nukite.Views;
using Nukite.Services.Data;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=234238

namespace Nukite
{
    /// <summary>
    /// Una página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void SettingsNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            string tag = ((string)selectedItem.Tag);

            if (!args.IsSettingsSelected)
            {
                if (tag == "bookmarkSetMenu")
                {
                    ContentFrame.Navigate(typeof(BookmarksSettings), null, args.RecommendedNavigationTransitionInfo);
                }
                else if (tag == "historySetMenu")
                {
                    ContentFrame.Navigate(typeof(History), null, args.RecommendedNavigationTransitionInfo);
                }
                else if (tag == "searchSetMenu")
                {
                    ContentFrame.Navigate(typeof(SearchSettings), null, args.RecommendedNavigationTransitionInfo);
                }
                else if (tag == "launchSettingsFile")
                {
                    DataTransfer dt = new DataTransfer();
                    dt.LoadXmlFile();
                }
            }
        }

        private void settingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsNavView.SelectedItem = SettingsNavView.MenuItems[0];
        }
    }
}
