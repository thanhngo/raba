#region

using System;
using System.Collections.ObjectModel;
using MahApps.Metro.IconPacks;
using RabaMetroStyle.Mvvm;
using RabaMetroStyle.Views;

#endregion

namespace RabaMetroStyle.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        private static readonly ObservableCollection<MenuItem> AppMenu = new ObservableCollection<MenuItem>();
        private static readonly ObservableCollection<MenuItem> AppOptionsMenu = new ObservableCollection<MenuItem>();

        public ShellViewModel()
        {
            // Build the menus            
            this.Menu.Add(new MenuItem
                          {
                              Icon = new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.CogsSolid },
                              Label = "Service",
                              NavigationType = typeof(SettingsPage),
                              NavigationDestination = new Uri("Views/ServicePage.xaml", UriKind.RelativeOrAbsolute)
                          });

            this.Menu.Add(new MenuItem
                          {
                              Icon = new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.CogSolid },
                              Label = "Settings",
                              NavigationType = typeof(SettingsPage),
                              NavigationDestination = new Uri("Views/SettingsPage.xaml", UriKind.RelativeOrAbsolute)
                          });
            this.OptionsMenu.Add(new MenuItem
                                 {
                                     Icon = new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.InfoCircleSolid },
                                     Label = "About",
                                     NavigationType = typeof(AboutPage),
                                     NavigationDestination = new Uri("Views/AboutPage.xaml", UriKind.RelativeOrAbsolute)
                                 });
        }

        public ObservableCollection<MenuItem> Menu => AppMenu;

        public ObservableCollection<MenuItem> OptionsMenu => AppOptionsMenu;
    }
}