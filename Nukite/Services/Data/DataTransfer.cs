﻿using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace Nukite.Services.Data
{

    public class DataTransfer
    {

        string SettingsFileName = "settings.xml";

        public async void SaveSearchTerm(string SearchTerm, string title, string url, DateTime dateTime)
        {
            var doc = await DocumentLoad().AsAsyncOperation();
            var history = doc.GetElementsByTagName("history");

            XmlElement elSearchTerm = doc.CreateElement("searchterm");
            XmlElement elSiteName = doc.CreateElement("sitename");
            XmlElement elUrl = doc.CreateElement("url");
            XmlElement elDateTime = doc.CreateElement("datetime");

            var historyItem = history[0].AppendChild(doc.CreateElement("historyitem"));

            historyItem.AppendChild(elSearchTerm);
            historyItem.AppendChild(elSiteName);
            historyItem.AppendChild(elUrl);
            historyItem.AppendChild(elDateTime);

            elSearchTerm.InnerText = SearchTerm;
            elSiteName.InnerText = title;
            elUrl.InnerText = url;
            elDateTime.InnerText = dateTime.ToString();

            SaveDoc(doc);



        }

        //Load Settings XML

        private async Task<XmlDocument> DocumentLoad()
        {
            XmlDocument result = null;

            await Task.Run(async () =>
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(SettingsFileName);
                XmlDocument doc = await XmlDocument.LoadFromFileAsync(file);
                result = doc;
            });
            return result;
        }

        private async void SaveDoc(XmlDocument doc)
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(SettingsFileName);
            await doc.SaveToFileAsync(file);

        }

        public async Task<List<string>> Fetch(string Source)
        {
            List<string> list = new List<string>();

            await Task.Run(async () =>
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(SettingsFileName);
                XmlDocument doc = await XmlDocument.LoadFromFileAsync(file);

                var historyItem = doc.GetElementsByTagName("historyitem");

                for (int i = 0; i < historyItem.Count; i++){

                    var historyItemChild = historyItem[i].ChildNodes;


                    for(int h = 0; h < historyItemChild.Count; h++)
                    {
                        if (historyItemChild[h].NodeName == Source)
                        {
                            list.Add(historyItemChild[h].InnerText);
                        }
                    }
                }
            });

            return list;
        }

        //Opens XML Settings file in an editor
        public async void LoadXmlFile()
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(SettingsFileName);
            await Launcher.LaunchFileAsync(file);
        }

        // Search Engine
        public async Task<List<string>> SearchEngineList(string AttributeSource)
        {
            List<string> list = new List<string>();

            await Task.Run(async () =>
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(SettingsFileName);
                XmlDocument doc = await XmlDocument.LoadFromFileAsync(file);

                var searchengine = doc.GetElementsByTagName("searchengine");

                var searchChild = searchengine[0].ChildNodes;

                for (int j = 0; j < searchChild.Count; j++)
                {
                    if (searchChild[j].NodeName == "engine")
                    {
                        list.Add(searchChild[j].Attributes.GetNamedItem(AttributeSource).InnerText);
                    }
                }
            });
            return list;
        }

        public async void SetSearchEngine(string EngineName)
        {
            var doc = await DocumentLoad();

            var searchEngine = doc.GetElementsByTagName("searchengine");

            var engines = searchEngine[0].ChildNodes;

            for (int i = 0; i < engines.Count; i++)
            {
                if (engines[i].NodeName == "engine")
                {
                    if (engines[i].Attributes.GetNamedItem("name").InnerText == EngineName)
                    {
                        engines[i].Attributes.GetNamedItem("selected").InnerText = true.ToString();
                    }
                    else
                    {
                        engines[i].Attributes.GetNamedItem("selected").InnerText = false.ToString();
                    }
                }
            }

            SaveDoc(doc);
        }

        public async Task<string> GetSelectedEngineAttribute(string AttributeName)
        {
            string value = string.Empty;

            await Task.Run(async () =>
            {
                var doc = await DocumentLoad();

                var searchEngine = doc.GetElementsByTagName("searchengine");

                var engines = searchEngine[0].ChildNodes;


                for (int i = 0; i < engines.Count; i++)
                {
                    if (engines[i].NodeName == "engine")
                    {
                        if (engines[i].Attributes.GetNamedItem("selected").InnerText == true.ToString())
                        {
                            value = engines[i].Attributes.GetNamedItem(AttributeName).InnerText;
                        }
                    }
                }

            });

            return value;
        }
        // Url

        public async Task<bool> HasUrlType(string searchString)
        {
            bool result = false;

            await Task.Run(async () =>
            {
                var doc = await DocumentLoad();

                var types = doc.GetElementsByTagName("types");

                var typeChildren = types[0].ChildNodes;

                for (int i = 0; i < typeChildren.Count; i++)
                {
                    if (typeChildren[i].NodeName == "type")
                    {
                        if (searchString.Contains(typeChildren[i].Attributes.GetNamedItem("name").InnerText))
                        {
                            result = true;
                        }
                    }
                }

            });

            return result;
        }

        // Home & New tab

        public async void SetHome(string Name, string Url)
        {
            var doc = await DocumentLoad();

            var home = doc.GetElementsByTagName("home");

            home[0].Attributes.GetNamedItem("name").InnerText = Name;
            home[0].Attributes.GetNamedItem("url").InnerText = Url;

            SaveDoc(doc);
        }

        public async Task<string> GetHomeAttribute(string Source)
        {
            string result = "";

            await Task.Run(async () =>
            {
                var doc = await DocumentLoad();
                var home = doc.GetElementsByTagName("home");

                result = home[0].Attributes.GetNamedItem(Source).InnerText;

            });

            return result;
        }

        // Bookmarks

        public async void SaveBookmark(string Url, string Title, string Favicon)
        {
            var doc = await DocumentLoad();

            var bookmarks = doc.GetElementsByTagName("bookmarks");

            var bookmark = bookmarks[0].AppendChild(doc.CreateElement("bookmark"));
            var bookUrl = bookmark.AppendChild(doc.CreateElement("url"));
            var bookTitle = bookmark.AppendChild(doc.CreateElement("title"));
            var favicon = bookmark.AppendChild(doc.CreateElement("favicon"));

            bookUrl.InnerText = Url;
            bookTitle.InnerText = Title;
            favicon.InnerText = Favicon;

            SaveDoc(doc);
            Console.WriteLine("Going to load...")
            GetBookmarkList();
            Console.WriteLine("Loaded! Lmao");

        }

        public async Task<List<BookmarkDetails>> GetBookmarkList()
        {
            List<BookmarkDetails> list = new List<BookmarkDetails>();

            await Task.Run(async () =>
            {
                var doc = await DocumentLoad();

                var bookmark = doc.GetElementsByTagName("bookmarks");

                for (int i = 0; i < bookmark.Count; i++)
                {
                    var children = bookmark[i].ChildNodes;

                    string returnUrl = string.Empty;
                    string returnTitle = string.Empty;
                    string returnFavicon = string.Empty;

                    if (bookmark[i].NodeName == "bookmark")
                    {
                        for (int j = 0; j < children.Count; j++)
                        {
                            if (children[j].NodeName == "url")
                            {
                                returnUrl = children[j].InnerText;
                            }

                            if (children[j].NodeName == "title")
                            {
                                returnTitle = children[j].InnerText;
                            }

                            if (children[j].NodeName == "favicon")
                            {
                                returnFavicon = children[j].InnerText;
                            }
                        }
                    }

                    if (returnUrl != string.Empty && returnTitle != string.Empty)
                    {
                        list.Add(new BookmarkDetails
                        {
                            Title = returnTitle,
                            Url = returnUrl,
                            Favicon = returnFavicon
                        });
                    }
                }

            });

            return list;
        }

        public async void RemoveBookmark(string Url)
        {
            var doc = await DocumentLoad();

            var bookmark = doc.GetElementsByTagName("bookmarks");

            for (int i = 0; i < bookmark.Count; i++)
            {
                var children = bookmark[i].ChildNodes;

                for (int j = 0; j < children.Count; j++)
                {
                    if (children[j].NodeName == Url)
                    {
                        children[j].ParentNode.ParentNode.RemoveChild(bookmark[i]);
                    }
                }

            }

            SaveDoc(doc);
        }

        public class BookmarkDetails
        {
            public string Url
            {
                get; set;
            }

            public string Title
            {
                get; set;
            }

            public string Favicon
            {
                get; set;
            }
        }
    }
}
