using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2;
using Nukite.Services.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static Nukite.Services.Data.DataTransfer;
using muxc = Microsoft.UI.Xaml.Controls;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a
//https://codebeautify.org/xmlvalidator/cb35e9c7
namespace Nukite
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Search Engine Prefix
        public string prefix = string.Empty;

        // The number of tabs open
        int settingTabCount = 0;

        // Home button Url and Home Name
        string homeUrl = string.Empty, homeName = string.Empty;

        // The current selected Tab
        TabViewItem currentSelectedTab = null;
        // The current selected Web View
        WebView2 currentSelectedWebView = null;

        public MainPage()
        {
            Debug.WriteLine("ANTES");
            InitializeComponent();
            Debug.WriteLine("Después");

            // Instance of Data Access class
            DataAccess dataAccess = new DataAccess();

            // Call the function that checks if the settings file exists, and if it doesn't create one new.
            //dataAccess.CheckIfXMLExists();
            dataAccess.CreateSettingsFile();

            // On start navigate the default browser to home that is set in the xml file.
            GetHome();
            
            // Load Bookmarks
            SetupBookmarks();
        }



        private async void SetupBookmarks()
        {
            await GetBookmarks();
        }

        private async Task GetBookmarks()
        {
            DataTransfer dt = new DataTransfer();
            await Task.Delay(1);

            List<BookmarkDetails> BookmarkItemList = await dt.GetBookmarkList();

            for (int i = 0; i < BookmarkItemList.Count; i++)
            {
                Button btn = new Button();
                btn.Style = Application.Current.Resources["BookmarkBtn"] as Style;
                btn.DataContext = BookmarkItemList[i];
                btn.Margin = new Thickness(10, 0, 5, 0);

                bookmarkBarSP.Children.Add(btn);
            }
        }

        private async void GetHome()
        {
            // Try navigate home
            try
            {
                DataTransfer dt = new DataTransfer();

                homeName = await dt.GetHomeAttribute("name");
                Debug.WriteLine(homeName);
                homeUrl = await dt.GetHomeAttribute("url");
            }
            catch (Exception ex)
            {
                // Show a error message if there is an issue.
                MessageDialog msg = new MessageDialog(ex.Message);
                await msg.ShowAsync();
            }

            // See if the homeUrl and homeName are not null.
            if (!string.IsNullOrEmpty(homeUrl) || !string.IsNullOrEmpty(homeName))
            {
                // Naviaget home if they aren't.
                NavigateHome();
            }

            currentSelectedTab = defaultTab;
            currentSelectedWebView = webBrowser;
        }

        private void NavigateHome()
        {
            if (currentSelectedWebView == null)
            {
                webBrowser.CoreWebView2.Navigate(homeUrl);
                defaultTab.Header = webBrowser.CoreWebView2.DocumentTitle;
            }
            else
            {
                // Navigate the current selected web view to the home url.
                currentSelectedWebView.CoreWebView2.Navigate(homeUrl);
                // Set the current selected tab header to home name.
                currentSelectedTab.Header = currentSelectedWebView.CoreWebView2.DocumentTitle;
            }
        }

        /// <summary>
        /// Navigate the default browser to go back.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            // Navigate backwards.
            if (currentSelectedWebView.CanGoBack) // <<Fix Needed>>
            {
                currentSelectedWebView.GoBack();
            }
        }

        private void frdBtn_Click(object sender, RoutedEventArgs e)
        {
            // Navigate forwards.
            if (currentSelectedWebView.CanGoForward) // <<Fix Needed>>
            {
                currentSelectedWebView.GoForward();
            }
        }

        /// <summary>
        /// When a key is pressed while the search bar has focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBar_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // If the key pressed is the enter key
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Call the search function.
                Search();
            }
        }

        private async void Search()
        {
            // Get an instance of the Data Transfer class.
            DataTransfer dt = new DataTransfer();

            // Returns a true or false if the url bar has a host type (set in the xml settings file).
            bool hasUrlType = await dt.HasUrlType(SearchBar.Text);

            // If there is a type the navigate the current selected web view to the destination and adds https to the beginning.

            if (hasUrlType)
            {
                // If the url doesn't contain http or https the add it to the beginning.
                if (!SearchBar.Text.Contains("http://") || !SearchBar.Text.Contains("https://"))
                {
                    currentSelectedWebView.CoreWebView2.Navigate("https://www." + SearchBar.Text);
                }
                else
                {
                    SearchBar.Text = "https://www." + SearchBar.Text;
                }

                // Change the search text to the url.
                SearchBar.Text = currentSelectedWebView.Source.AbsoluteUri;
            }
            else
            {
                // Set the global veriable "prefix" to the selected engine.
                prefix = await dt.GetSelectedEngineAttribute("prefix");

                if (currentSelectedTab != null)
                {
                    // add the prefix if it's not a settings page.
                    if (currentSelectedTab.Name != "settingsTab")
                    {
                        if (currentSelectedWebView == null) // Posible error if no tab, could cause crash. <<Fix Needed>>
                        {
                            // search with the prefix of the selected search engine on the default.
                            webBrowser.Source = new Uri(prefix + SearchBar.Text);
                        }
                        else
                        {
                            // search with the prefix of the selected search engine on the current
                            currentSelectedWebView.Source = new Uri(prefix + SearchBar.Text);
                        }
                    }
                }
                else
                {

                }


            }
        }

        /// <summary>
        /// Refresh Web Browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshBtn_Click(object sender, RoutedEventArgs e)
        {
            // Refresh current selected web view.
            currentSelectedWebView.Reload();
        }

        private void settingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //this.Frame.Navigate(typeof(SettingsPage));
            AddSettingsTab();
            settingTabCount++; // Increment tab (settings) count.
        }

        private void AddSettingsTab()
        {
            //New Tab
            var settingsTab = new TabViewItem();
            //Name the header "Ajustes"
            settingsTab.Header = "Ajustes";
            //Name the tab
            settingsTab.Name = "settingsTab";
            // Change the tab icon.
            settingsTab.IconSource = new muxc.SymbolIconSource() { Symbol = Symbol.Setting };
            // Create a frame instance
            Frame frame = new Frame();
            // Add the frame to the tab
            settingsTab.Content = frame;
            // Navigate the frame to the settings page.
            frame.Navigate(typeof(SettingsPage));
            // Add the tab to the tab control.
            TabControl.TabItems.Add(settingsTab);
            // Set the newly created tab as the selected tab.
            TabControl.SelectedItem = settingsTab;

        }

        private void MainBrowserWindow_Loading(FrameworkElement sender, object args)
        {

        }

        private void webBrowser_Loading(FrameworkElement sender, object args)
        {
            // Set the status text at the bottom-left of the browser window.
            statusText.Text = webBrowser.Source.AbsoluteUri;
        }

        private void webBrowser_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            browserProgress.IsActive = false;
            try
            {
                // Change the status text to the url
                statusText.Text = webBrowser.Source.AbsoluteUri;

                AppTitle.Text = "Nukite" + " | " + webBrowser.CoreWebView2.DocumentTitle;

                DataTransfer dataTransfer = new DataTransfer();

                // Check if the search bar doesn't contain text.
                if (!string.IsNullOrEmpty(SearchBar.Text))
                {
                    // if it doesn't then save the search term in history.
                    dataTransfer.SaveSearchTerm(SearchBar.Text, webBrowser.CoreWebView2.DocumentTitle, webBrowser.Source.AbsoluteUri, DateTime.Now); // Error
                }
            }
            catch
            {

            }

            // Call the check ssl function.
            CheckSSL();

            // check if the status text contains "BlankPage".
            if (!statusText.Text.Contains("BlankPage"))
            {
                if (currentSelectedWebView == null)
                {
                    statusText.Text = webBrowser.Source.AbsoluteUri;
                }
                else
                {
                    // Sets the status text to the current selected web view url
                    statusText.Text = currentSelectedWebView.Source.AbsoluteUri; // Error 
                }
            }
            else
            {
                // Set the staus text to Blank Page
                statusText.Text = "Blank Page";
            }

            // Default header = the default web view doc title.
            defaultTab.Header = webBrowser.CoreWebView2.DocumentTitle;

        }

        /// <summary>
        /// Do a check to see if the current selected view has a ssl cert.
        /// </summary>
        private void CheckSSL()
        {


            if (currentSelectedWebView != null)
            {
                if (currentSelectedWebView.Source.AbsoluteUri.Contains("https"))
                {
                    // Change icon image
                    sslIcon.FontFamily = new FontFamily("Segoe MDL2 Assets");
                    sslIcon.Glyph = "\xE72E";

                    ToolTip toolTip = new ToolTip();
                    toolTip.Content = "This website has a SSL certificate.";
                    ToolTipService.SetToolTip(sslButton, toolTip);
                }
                else
                {
                    // Change icon image.
                    sslIcon.FontFamily = new FontFamily("Segoe MDL2 Assets");
                    sslIcon.Glyph = "\xE785";

                    ToolTip toolTip = new ToolTip();
                    toolTip.Content = "This website is unsafe and doesn't have a SSL certificate.";
                    ToolTipService.SetToolTip(sslButton, toolTip);
                }
            }
            else
            {
                if (webBrowser.Source.AbsoluteUri.Contains("https"))
                {
                    // Change icon image
                    sslIcon.FontFamily = new FontFamily("Segoe MDL2 Assets");
                    sslIcon.Glyph = "\xE72E";

                    ToolTip toolTip = new ToolTip();
                    toolTip.Content = "This website has a SSL certificate.";
                    ToolTipService.SetToolTip(sslButton, toolTip);
                }
                else
                {
                    // Change icon image.
                    sslIcon.FontFamily = new FontFamily("Segoe MDL2 Assets");
                    sslIcon.Glyph = "\xE785";

                    ToolTip toolTip = new ToolTip();
                    toolTip.Content = "This website is unsafe and doesn't have a SSL certificate.";
                    ToolTipService.SetToolTip(sslButton, toolTip);
                }
            }
        }

        private void webBrowser_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            browserProgress.IsActive = true;
        }

        private void TabControl_AddTabButtonClick(muxc.TabView sender, object args)
        {
            
            AddNewTab(new Uri(homeUrl));
        }

        private void BrowserNaviagted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {

            /*currentSelectedWebView.CoreWebView2.ContainsFullScreenElementChanged += (obj, args) =>
            {
                this.FullScreen = webView21.CoreWebView2.ContainsFullScreenElement;
            };*/

            browserProgress.IsActive = false;
            var view = sender;
            var tab = view.Parent as muxc.TabViewItem;

            if (view != null) // Part 26 Changed
            {
                tab.Header = view.CoreWebView2.DocumentTitle;

            }

            if (!statusText.Text.Contains("BlankPage"))
            {
                statusText.Text = currentSelectedWebView.Source.AbsoluteUri; // Error
            }
            else
            {
                statusText.Text = "Blank Page";
            }

            tab.IconSource = new muxc.BitmapIconSource() { UriSource = new Uri(view.Source.Host + "favicon.ico") };
            CheckSSL();
            tab.Header = view.CoreWebView2.DocumentTitle;
        }

        private void TabControl_TabCloseRequested(muxc.TabView sender, muxc.TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
            currentSelectedTab = null;
            currentSelectedWebView = null;

            if (args.Tab.Name == "settingsTab")
            {
                settingTabCount = 0;
            }
        }

        private void OnNewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            Uri uri = new Uri(args.Uri);
            AddNewTab(uri);
            args.Handled = true;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Search
            Search();
        }

        private void SearchBar_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentSelectedTab = TabControl.SelectedItem as muxc.TabViewItem;
            if (currentSelectedTab != null)
            {
                currentSelectedWebView = currentSelectedTab.Content as WebView2;
            }

            if (currentSelectedWebView != null)
            {
                AppTitle.Text = "Nukite" + " | " + webBrowser.CoreWebView2.DocumentTitle;
            }

            if (!statusText.Text.Contains("BlankPage") && currentSelectedWebView != null)
            {
                statusText.Text = currentSelectedWebView.Source.AbsoluteUri;
            }
            else
            {
                statusText.Text = "Blank Page";
            }
        }

        private async void MainBrowserWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTransfer dt = new DataTransfer();

                string searchEngineName = await dt.GetSelectedEngineAttribute("name");
                prefix = await dt.GetSelectedEngineAttribute("prefix");

                SearchBar.PlaceholderText = "Buscar con " + searchEngineName + "...";
            }
            catch
            {

            }
        }

        private void webBrowser_NewWindowRequested(WebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            Uri uri = new Uri(homeUrl);
            AddNewTab(uri);
            args.Handled = true;
        }

        private void homeBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigateHome();
        }

        private void favButton_Click(object sender, RoutedEventArgs e)
        {
                if (currentSelectedTab.Name != "settingsTab")
                {
                    bm_title_box.Text = currentSelectedWebView.CoreWebView2.DocumentTitle;
                    bm_url_box.Text = currentSelectedWebView.Source.AbsoluteUri;
                }

        }

        private async void bm_add_btn_Click(object sender, RoutedEventArgs e)
        {
            DataTransfer dt = new DataTransfer();
            string host = currentSelectedWebView.Source.Host + "/favicon.ico";

            if (!string.IsNullOrEmpty(bm_title_box.Text) && !string.IsNullOrEmpty(bm_url_box.Text))
            {
                dt.SaveBookmark(bm_url_box.Text, bm_title_box.Text, host);
                bookmark_btn_flyout.Hide();

            }
            else
            {
                MessageDialog msg = new MessageDialog("Fields must not be null");
                await msg.ShowAsync();
            }
        }

        private void bm_cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            bookmark_btn_flyout.Hide();
        }

        private void AddNewTab(Uri Url)
        {
            var newTab = new TabViewItem();
            newTab.IconSource = new muxc.SymbolIconSource() { Symbol = Symbol.Document };
            newTab.Header = "Nueva pagina";

            WebView2 webView = new WebView2();
            webView.IsRightTapEnabled = true;

            newTab.Content = webView;
            webView.Source = new Uri(homeUrl);

            TabControl.TabItems.Add(newTab);

            TabControl.SelectedItem = newTab;

            webView.NavigationCompleted += BrowserNaviagted;
            webView.CoreWebView2.NewWindowRequested += OnNewWindowRequested;

            currentSelectedTab = newTab;
            currentSelectedWebView = webView;
        }

    }
}
