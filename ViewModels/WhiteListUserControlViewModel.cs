using Prism.Commands;
using Prism.Mvvm;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RdpScopeToggler.Services.FilesService;
using System.Text.RegularExpressions;
using Prism.Navigation.Regions;
using RdpScopeToggler.Stores;
using System.Xml.Linq;

namespace RdpScopeToggler.ViewModels
{
    public class WhiteListEntry : BindableBase
    {
        private string _address;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
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
        public ObservableCollection<WhiteListEntry> WhiteListItems { get; } = new();

        public DelegateCommand<WhiteListEntry> RemoveItemCommand { get; }
        public DelegateCommand AddItemCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand NavigateToHomeCommand { get; }

        private readonly IFilesService filesService;
        public WhiteListUserControlViewModel(IRegionManager regionManager ,IFilesService filesService)
        {
            this.filesService = filesService;
            List<WhiteListClient> whiteList = filesService.GetWhiteList();

            foreach (var ip in whiteList)
            {
                WhiteListEntry asd = new();
                asd.Address = ip.Address;
                asd.Name = ip.Name;
                WhiteListItems.Add(asd);
            }
            AddItemCommand = new DelegateCommand(AddItem);

            RemoveItemCommand = new DelegateCommand<WhiteListEntry>(RemoveItem);

            SaveCommand = new DelegateCommand(Save);

            NavigateToHomeCommand = new DelegateCommand(() =>
            {
                regionManager.RequestNavigate("ContentRegion", "HomeUserControl");
            });
        }
        private void RemoveItem(WhiteListEntry item)
        {
            if (WhiteListItems.Contains(item))
                WhiteListItems.Remove(item);


            filesService.CleanWhiteList();

            foreach (var item1 in WhiteListItems)
            {
                if (!IsValidIPv4(item1.Address))
                {
                    MessageBox.Show("Not Valid " + item1.Address);
                    return;
                }
                filesService.AddToWhiteList(item1.Address, item.Name);
            }
        }

        private void AddItem()
        {
            WhiteListItems.Add(new WhiteListEntry { Address = string.Empty, Name = string.Empty });
        }

        private void Save()
        {
            filesService.CleanWhiteList();
            foreach (var client in WhiteListItems)
            {
                if (!IsValidIPv4(client.Address))
                {
                    MessageBox.Show("Not Valid " + client.Address);
                    return;
                }
                filesService.AddToWhiteList(client.Address, client.Name);
            }


            List<WhiteListClient> whiteList = filesService.GetWhiteList();
            WhiteListItems.Clear();
            foreach (var ip in whiteList)
            {
                WhiteListEntry asd = new();
                asd.Address = ip.Address;
                asd.Name = ip.Name;
                WhiteListItems.Add(asd);
            }
        }

        bool IsValidIPv4(string ipString)
        {
            return Regex.IsMatch(ipString, @"^((25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)(\.|$)){4}$");
        }

    }
}
