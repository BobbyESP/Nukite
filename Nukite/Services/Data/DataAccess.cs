using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;
using DOM = Windows.Data.Xml.Dom;

namespace Nukite.Services.Data
{
    public class DataAccess
    {
        public readonly string settingsFileName = "settings.xml";

        public async void CheckIfXMLExists()
        {
            #region "SHIT"
            /*var Settings = await ApplicationData.Current.LocalFolder.GetFileAsync("settings.xml");
             StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(settingsFileName);
             IBuffer buffer = await FileIO.ReadBufferAsync(file);
             string filePathString = Convert.ToBase64String(buffer.ToArray());
             if (File.Exists(filePathString))
             {

             }else if (!File.Exists(filePathString))
             {
                 CreateSettingsFile();
             }*/
            #endregion

            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(settingsFileName);
                Debug.WriteLine("Lo intentó");
            }
            catch
            {
                Debug.WriteLine("Settings File doesn't exist.");
                CreateSettingsFile();
            }
        }

        public async void CreateSettingsFile()
        {
            try
            {
                new ToastContentBuilder()
                    .AddText("Creando archivo XML de configuración...")
                    .AddText("Por favor, espere...")
                    .AddAttributionText("Nukite")
                    .Show();

                var createSettings = await ApplicationData.Current.LocalFolder.CreateFileAsync(settingsFileName);

                using (IRandomAccessStream writeStream = await createSettings.OpenAsync(FileAccessMode.ReadWrite))
                {
                    Stream s = writeStream.AsStreamForWrite();
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Async = true,
                        Indent = true
                    };

                    using (XmlWriter w = XmlWriter.Create(s, settings))
                    {
                        #region "RLly bad lol"
                       /* w.WriteStartDocument();
                        w.WriteStartElement("settings");
                        w.WriteStartElement("history");
                        w.WriteEndElement();
                        w.WriteStartElement("bookmarks");
                        w.WriteEndElement();
                        w.WriteStartElement("searchengine");
                        w.WriteStartElement("engine");
                        w.WriteAttributeString("prefix", "https://google.conm/search?q=");
                        w.WriteAttributeString("name", "Google");
                        w.WriteAttributeString("selected", "true");
                        w.WriteStartElement("types");
                        w.WriteStartElement("type");
                        w.WriteAttributeString("name", ".com");
                        w.WriteAttributeString("name", ".es");
                        w.WriteAttributeString("name", ".net");
                        w.WriteAttributeString("name", ".org");
                        w.WriteStartElement("home");
                        w.WriteAttributeString("name", "Google");
                        w.WriteAttributeString("url", "https://google.com");
                        w.WriteEndElement();
                        w.WriteEndElement();
                        w.WriteEndElement();
                        w.WriteEndElement();
                        w.WriteEndElement();
                        w.WriteEndElement();
                        w.WriteEndDocument();
                        w.Flush();
                        await w.FlushAsync();*/
                        #endregion
                        w.WriteStartDocument();
                        w.WriteStartElement("settings");
                        w.WriteStartElement("history");
                        w.WriteEndElement();
                        w.WriteStartElement("bookmarks");
                        w.WriteEndElement();
                        w.WriteStartElement("searchengine");
                        w.WriteStartElement("engine");
                        w.WriteAttributeString("prefix", "https://google.com/search?q=");
                        w.WriteAttributeString("name", "Google");
                        w.WriteAttributeString("selected", "true");
                        w.WriteEndElement();
                        w.WriteEndElement();
                        w.WriteStartElement("types");
                        w.WriteStartElement("type");
                        w.WriteAttributeString("name", ".com");
                        w.WriteEndElement();
                        w.WriteEndElement();
                        w.WriteStartElement("home");
                        w.WriteAttributeString("name", "Google");
                        w.WriteAttributeString("url", "https://google.com");
                        w.WriteEndElement();
                        w.WriteEndElement();
                        w.WriteEndDocument();
                        w.Flush();
                        await w.FlushAsync();
                    }
                }

                await Windows.System.Launcher.LaunchFileAsync(createSettings);

                new ToastContentBuilder()
                    .AddText("XML Creado correctamente")
                    .AddText("El XML de Configuración ha sido creado correctamente.")
                    .AddAttributionText("Nukite")
                    .Show();

            }
            catch
            {
                new ToastContentBuilder()
                    .AddText("Se ha producido un error")
                    .AddText("Error al intentar crear el archivo de configuración.")
                    .AddAttributionText("Nukite")
                    .Show();

            }
        }
        public async void SaveSearchTerm(string SearchTerm)
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(settingsFileName);
            var doc = await DOM.XmlDocument.LoadFromFileAsync(file);

            var elementHistory = doc.GetElementsByTagName("history");

            DOM.XmlElement elem = doc.CreateElement("searchedterm");
            DOM.XmlText text = doc.CreateTextNode(SearchTerm);

            elementHistory[0].AppendChild(elem);
            elementHistory[0].LastChild.AppendChild(text);

            await doc.SaveToFileAsync(file);


        }
    }
}
