﻿#region

using MahApps.Metro.Controls;
using RabaMetroStyle.Navigation;
using RabaMetroStyle.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

#endregion

namespace RabaMetroStyle
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly NavigationServiceEx navigationServiceEx;

        public MainWindow()
        {
            this.InitializeComponent();

            this.navigationServiceEx = new NavigationServiceEx();
            this.navigationServiceEx.Navigated += this.NavigationServiceEx_OnNavigated;
            this.HamburgerMenuControl.Content = this.navigationServiceEx.Frame;            

            // Navigate to the home page.
            this.Loaded += (sender, args) => this.navigationServiceEx.Navigate(new Uri("Views/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GoBack_OnClick(object sender, RoutedEventArgs e)
        {
            this.navigationServiceEx.GoBack();
        }

        private void HamburgerMenuControl_OnItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            if (e.InvokedItem is MenuItem menuItem && menuItem.IsNavigation)
            {
                this.navigationServiceEx.Navigate(menuItem.NavigationDestination);
            }
        }

        private void NavigationServiceEx_OnNavigated(object sender, NavigationEventArgs e)
        {
            // select the menu item
            this.HamburgerMenuControl.SelectedItem = this.HamburgerMenuControl
                                                         .Items
                                                         .OfType<MenuItem>()
                                                         .FirstOrDefault(x => x.NavigationDestination == e.Uri);
            this.HamburgerMenuControl.SelectedOptionsItem = this.HamburgerMenuControl
                                                                .OptionsItems
                                                                .OfType<MenuItem>()
                                                                .FirstOrDefault(x => x.NavigationDestination == e.Uri);

            // update back button
            this.GoBackButton.Visibility = this.navigationServiceEx.CanGoBack ? Visibility.Visible : Visibility.Collapsed;

            this.Title = "RABA";
            if (this.HamburgerMenuControl.SelectedItem != null)
            {
                var menuItem = (MenuItem)this.HamburgerMenuControl.SelectedItem;
                this.Title = string.Format("{0} - {1}", this.Title, menuItem.Label);
            }
            else
            {
                if (this.HamburgerMenuControl.SelectedOptionsItem != null)
                {
                    var menuItem = (MenuItem)this.HamburgerMenuControl.SelectedOptionsItem;
                    this.Title = string.Format("{0} - {1}", this.Title, menuItem.Label);
                }
            }
        }
    }
}