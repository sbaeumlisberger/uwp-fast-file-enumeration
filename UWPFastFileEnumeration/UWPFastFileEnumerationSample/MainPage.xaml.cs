using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UWPFastFileEnumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UWPFastFileEnumerationTest
{

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            StorageApplicationPermissions.FutureAccessList.Clear();
            output.Text += "Cleared FutureAccessList\n";

            FolderPicker folderPicker = new FolderPicker()
            {
                FileTypeFilter = { "*" }
            };

            var folder = await folderPicker.PickSingleFolderAsync();

            if (!StorageApplicationPermissions.FutureAccessList.CheckAccess(folder))
            {
                StorageApplicationPermissions.FutureAccessList.Add(folder);
            }

            output.Text += "User selected directory: " + folder.Path + "\n";

            output.Text += "Added selected directory to FutureAccessList\n";

            var watch = Stopwatch.StartNew();

            var count = DirectoryHelper.EnumerateFiles(folder.Path, "*.*", SearchOption.AllDirectories).Count();

            watch.Stop();
            output.Text += "---> Enumerated " + count + " files in " + watch.ElapsedMilliseconds + "ms\n";
        }
    }
}
