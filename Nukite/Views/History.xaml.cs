using Nukite.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=234238

namespace Nukite.Views
{
    /// <summary>
    /// Una página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class History : Page
    {
        int LBICount = 0;

        public History()
        {
            this.InitializeComponent();
        }

        private async void AddListBoxItems()
        {
            DataTransfer dataTransfer = new DataTransfer();
            List<string> historyUrlItems = await dataTransfer.Fetch("url");

            foreach (var item in historyUrlItems)
            {
                ListBoxItem newLBI = new ListBoxItem();

                newLBI.Name = "LBI" + LBICount;

                listory.Items.Add(newLBI);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AddListBoxItems();
        }
    }

    public class URL
    {
        public string Url { get; set; }
    }
}
