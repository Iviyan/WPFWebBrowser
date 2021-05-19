using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<(DateTime dateTime, string address)> history = new();

        Dictionary<ChromiumWebBrowser, TabItem> browserTab = new();

        ContextMenu MoreMenu;

        public MainWindow()
        {
            InitializeComponent();

            MoreMenu = this.Resources["More"] as ContextMenu;

            AddTab("yandex.ru");
        }

        private TabItem GetTargetTabItem(object originalSource)
        {
            var current = originalSource as DependencyObject;

            while (current != null)
            {
                if (current is Button) return null;

                var tabItem = current as TabItem;
                if (tabItem != null)
                    return tabItem;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private void TabItem_Drag(object sender, MouseEventArgs e)
        {
 /*           var tabItem = e.Source as TabItem;

            if (tabItem == null)
                return;

            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);*/
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            var tabItemTarget = GetTargetTabItem(e.OriginalSource);
            if (tabItemTarget == null) return;

            var tabItemSource = e.Data.GetData(typeof(TabItem)) as TabItem;

            if (!tabItemTarget.Equals(tabItemSource))
            {
                int sourceIndex = Tabs.Items.IndexOf(tabItemSource);
                int targetIndex = Tabs.Items.IndexOf(tabItemTarget);

                Tabs.Items.Remove(tabItemSource);
                Tabs.Items.Insert(targetIndex, tabItemSource);

                /*Tabs.Items.Remove(tabItemTarget);
                Tabs.Items.Insert(sourceIndex, tabItemTarget);*/

                Tabs.SelectedIndex = targetIndex;
            }
        }

        private ChromiumWebBrowser CreateBrowser()
        {
            ChromiumWebBrowser browser = new();
            browser.AddressChanged += Browser_AddressChanged;
            browser.TitleChanged += Browser_TitleChanged;
            return browser;
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddTab();
        }

        private void AddTab(string url = null)
        {
            ChromiumWebBrowser browser = CreateBrowser();
            if (url != null) browser.Address = url;

            TabItem tab = new();
            tab.Header = "Новая вкладка";
            tab.Content = browser;
            Tabs.Items.Add(tab);

            browserTab[browser] = tab;
        }

        private void Browser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var br = sender as ChromiumWebBrowser;
            var tab = browserTab[br];
            tab.Header = br.Title;
        }

        private void Browser_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            history.Add((DateTime.Now, e.NewValue as string));
        }


        private void UrlTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var br = Tabs.SelectedContent as ChromiumWebBrowser;
                br.Address = UrlTB.Text;
            }
        }

        private void CloseTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            browserTab.Remove((e.Parameter as TabItem).Content as ChromiumWebBrowser);
            Tabs.Items.Remove(e.Parameter);
        }
        
        private void DuplicateTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var tab = e.Parameter as TabItem;
            var br = tab.Content as ChromiumWebBrowser;

            ChromiumWebBrowser newBr = CreateBrowser();
            newBr.Address = br.Address;
            TabItem newTab = new();
            newTab.Content = newBr;
            newTab.Header = tab.Header;
            Tabs.Items.Add(newTab);

            browserTab[newBr] = newTab;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MoreMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            MoreMenu.PlacementTarget = sender as UIElement;
            MoreMenu.IsOpen = true;
        }

        private void CloseAllTabsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Tabs.Items.Clear();
        }

        private void CloseAllTabsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Tabs.Items.Count > 0;
        }

        private void CommandBinding_CanExecute_True(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
