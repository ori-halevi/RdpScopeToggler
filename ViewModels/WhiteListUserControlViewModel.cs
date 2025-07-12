using Prism.Commands;
using Prism.Mvvm;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RdpScopeToggler.Services.FilesService;
using System.Text.RegularExpressions;
using Prism.Navigation.Regions;
using RdpScopeToggler.Stores;
using RdpScopeToggler.Models;
using RdpScopeToggler.Views;
using System.Diagnostics;

namespace RdpScopeToggler.ViewModels
{
    public class WhiteListEntry : BindableBase
    {
        private string address;
        public string Address
        {
            get => address;
            set => SetProperty(ref address, value);
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }
    }

    public class WhiteListUserControlViewModel : BindableBase
    {
        private bool isNotSaved;
        public bool IsNotSaved
        {
            get { return isNotSaved; }
            set { SetProperty(ref isNotSaved, value); }
        }
        public ObservableCollection<WhiteListEntry> WhiteListItems { get; } = new();

        public DelegateCommand<WhiteListEntry> RemoveItemCommand { get; }
        public DelegateCommand AddItemCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand NavigateToHomeCommand { get; }

        private readonly IFilesService filesService;
        public WhiteListUserControlViewModel(IRegionManager regionManager, IFilesService filesService)
        {
            this.filesService = filesService;

            List<Client> whiteList = filesService.GetWhiteList();

            WhiteListItems.Clear();
            foreach (var ip in whiteList)
            {
                WhiteListEntry client = new();
                client.Address = ip.Address;
                client.Name = ip.Name;
                WhiteListItems.Add(client);
            }

            // Subscribe to collection changes
            foreach (var item in WhiteListItems)
            {
                item.PropertyChanged += AlwaysOnEntry_PropertyChanged;
            }

            AddItemCommand = new DelegateCommand(AddItem);

            RemoveItemCommand = new DelegateCommand<WhiteListEntry>(RemoveItem);

            SaveCommand = new DelegateCommand(Save);

            NavigateToHomeCommand = new DelegateCommand(() =>
            {
                regionManager.RequestNavigate("ContentRegion", "MainUserControl");
            });
        }




        private void WhiteListItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (WhiteListEntry newItem in e.NewItems)
                {
                    newItem.PropertyChanged += AlwaysOnEntry_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (WhiteListEntry oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= AlwaysOnEntry_PropertyChanged;
                }
            }
        }

        private void AlwaysOnEntry_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // When any property changes, mark as not saved
            IsNotSaved = true;
        }









        private void RemoveItem(WhiteListEntry item)
        {
            if (WhiteListItems.Contains(item))
                WhiteListItems.Remove(item);

            IsNotSaved = true;
        }

        private void AddItem()
        {
            WhiteListItems.Add(new WhiteListEntry { Address = string.Empty, Name = string.Empty });
            IsNotSaved = true;
        }

        private void ShowWarrning(string ipAddress)
        {
            var options = new GenericDialogOptions
            {
                Title = "Ip address error",
                Message = $"כתובת ip לא הגיונית.\r\n{ipAddress}",
                OnClose = () => { },
                IsModal = true,
                Topmost = true,
            };

            var dialog = new GenericDialogWindow(options);
            if (options.IsModal)
                dialog.ShowDialog();
            else
                dialog.Show();
        }

        private void Save()
        {
            filesService.CleanWhiteList();
            bool vaild = true;
            foreach (var client in WhiteListItems)
            {
                if (!IsValidIPv4(client.Address))
                {
                    Debug.WriteLine(client.Name + " " + client.Address);
                    ShowWarrning(client.Address);
                    vaild = false;
                    continue;
                }
                filesService.AddToWhiteList(client.Address, client.Name);
            }
            if (!vaild) return;


            List<Client> whiteList = filesService.GetWhiteList();
            WhiteListItems.Clear();
            foreach (var ip in whiteList)
            {
                WhiteListEntry asd = new();
                asd.Address = ip.Address;
                asd.Name = ip.Name;
                WhiteListItems.Add(asd);
            }

            // Subscribe to collection changes
            foreach (var item in WhiteListItems)
            {
                item.PropertyChanged += AlwaysOnEntry_PropertyChanged;
            }

            IsNotSaved = false;
        }




        bool IsValidIPv4(string ipString)
        {
            return Regex.IsMatch(ipString, @"^((25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)(\.|$)){4}$");
        }

    }
}
