using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace RstEditor
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class StartPage : Page
    {
        private static event TappedEventHandler openButtonTappedEvent;
        public StartPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                openButtonTappedEvent = e.Parameter as TappedEventHandler;
            }
            base.OnNavigatedTo(e);
        }
        private void button_Tutorial_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ContentFrame?.Navigate(typeof(TutorialPage),null,new ContinuumNavigationTransitionInfo());
            MainPage.TopNavigationView.SelectedItem = null;
        }

        private void button_InputFile_Click(object sender, RoutedEventArgs e)
        {
            openButtonTappedEvent?.Invoke(null, null);
        }

        private void button_Settings_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ContentFrame?.Navigate(typeof(SettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight});
            MainPage.TopNavigationView.SelectedItem = MainPage.TopNavigationView.SettingsItem;
        }
    }
}
