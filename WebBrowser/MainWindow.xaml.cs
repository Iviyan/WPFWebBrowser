using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ObservableCollection<HistoryItem> history = new();
        HistoryWindow historyWindow;

        Dictionary<ChromiumWebBrowser, TabItem> browserTab = new();

        ContextMenu MoreMenu;

        public MainWindow()
        {
            InitializeComponent();

            historyWindow = new(history);
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

        static BrowserLifeSpanHandler browserLifeSpanHandler = new();
        private ChromiumWebBrowser CreateBrowser()
        {
            ChromiumWebBrowser browser = new();
            browser.AddressChanged += Browser_AddressChanged;
            browser.TitleChanged += Browser_TitleChanged;
            browser.LifeSpanHandler = browserLifeSpanHandler;
            return browser;
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddTab();
        }

        private void AddTab(string url = null)
        {
            ChromiumWebBrowser browser = CreateBrowser();

            TabItem tab = new();
            tab.Header = "Новая вкладка";
            tab.Content = browser;
            Tabs.Items.Add(tab);

            browserTab[browser] = tab;

            if (url != null) browser.Address = url;
        }

        private void Browser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var br = sender as ChromiumWebBrowser;
            var tab = browserTab[br];
            tab.Header = br.Title;
        }

        private void Browser_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            history.Add(new(e.NewValue as string));
            var br = sender as ChromiumWebBrowser;
            var tab = browserTab[br];
            if (tab == Tabs.SelectedItem) UrlTB.Text = br.Address;
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

        private void MoreButton_Click(object sender, RoutedEventArgs e)
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
        
        private void ShowHistoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            historyWindow.Show();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var br = Tabs.SelectedContent as ChromiumWebBrowser;
            br.Reload();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) =>
            (Tabs.SelectedContent as ChromiumWebBrowser).Back();
        
        private void ForwardButton_Click(object sender, RoutedEventArgs e) =>
            (Tabs.SelectedContent as ChromiumWebBrowser).Back();

        static BooleanToVisibilityConverter booleanToVisibilityConverter = new();
        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Visibility="{Binding IsLoading, ElementName=Browser, Converter={StaticResource BooleanToVisibilityConverter}}" 
            // IsIndeterminate="{Binding IsLoading, ElementName=Browser}"
            var br = Tabs.SelectedContent as ChromiumWebBrowser;

            UrlTB.Text = br.Address;

            Binding statusBind = new Binding();
            statusBind.Source = br;
            statusBind.Path = new PropertyPath(ChromiumWebBrowser.IsLoadingProperty);
            LoadingBar.SetBinding(ProgressBar.IsIndeterminateProperty, statusBind);
            
            Binding visBind = new Binding();
            visBind.Source = br;
            visBind.Path = new PropertyPath(ChromiumWebBrowser.IsLoadingProperty);
            visBind.Converter = booleanToVisibilityConverter;

            LoadingBar.SetBinding(ProgressBar.VisibilityProperty, visBind);
        }

        private void IncognitoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var tab = e.Parameter as TabItem;
                var br = tab.Content as ChromiumWebBrowser;

            if (tab.Tag != null && tab.Tag.Equals(true))
            {
                br.AddressChanged += Browser_AddressChanged;
                tab.Tag = null;
                //tab.Background = new SolidColorBrush(Colors.White);
            } else
            {
                br.AddressChanged -= Browser_AddressChanged;
                tab.Tag = true;
                //tab.Background = new SolidColorBrush(Colors.Black);

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e) =>
            (Tabs.SelectedContent as ChromiumWebBrowser).Print();
    }

    public class BrowserLifeSpanHandler : ILifeSpanHandler
    {
        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName,
            WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo,
            IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            browserControl.Load(targetUrl);
            return true;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {
            //
        }

        public bool DoClose(IWebBrowser browserControl, IBrowser browser)
        {
            return false;
        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {
            //nothing
        }
    }
}
