using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeToggler.Services.FilesService;
using RdpScopeToggler.Stores;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Text.RegularExpressions;
using RdpScopeToggler.Services.RdpService;
using System;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using System.Linq;
using RdpScopeToggler.Models;
using RdpScopeToggler.Views;

namespace RdpScopeToggler.ViewModels
{
    public class AlwaysOnListEntry : BindableBase
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

        private bool isOpen;
        public bool IsOpen
        {
            get { return isOpen; }
            set { SetProperty(ref isOpen, value); }
        }
    }
    public class LocalAddressesUserControlViewModel
    {
        public ObservableCollection<AlwaysOnListEntry> AlwaysOnListItems { get; } = new();

        public DelegateCommand<AlwaysOnListEntry> RemoveItemCommand { get; }
        public DelegateCommand AddItemCommand { get; }
        public DelegateCommand LoadLocalDevicesCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand NavigateToHomeCommand { get; }

        private readonly IFilesService filesService;
        private readonly IRdpService rdpService;

        public LocalAddressesUserControlViewModel(IRegionManager regionManager, IFilesService filesService, IRdpService rdpService)
        {
            this.filesService = filesService;
            this.rdpService = rdpService;
            List<Client> alwaysOnList = filesService.GetAlwaysOnList();

            foreach (var ip in alwaysOnList)
            {
                AlwaysOnListEntry client = new();
                client.Address = ip.Address;
                client.Name = ip.Name;
                client.IsOpen = ip.IsOpen;
                AlwaysOnListItems.Add(client);
            }

            LoadLocalDevicesCommand = new DelegateCommand(LoadLocalDevices);

            AddItemCommand = new DelegateCommand(AddItem);

            RemoveItemCommand = new DelegateCommand<AlwaysOnListEntry>(RemoveItem);

            SaveCommand = new DelegateCommand(Save);
        }

        private async void LoadLocalDevices()
        {
            var clients = await ScanNetworkAsync("192.168.1.");
            foreach (var client in clients)
            {
                Debug.WriteLine($"IP: {client.Address}, Name: {client.Name}, IsOpen: {client.IsOpen}");
                if (!AlwaysOnListItems.Any(item => item.Address == client.Address))
                    AlwaysOnListItems.Add(new AlwaysOnListEntry { Address = client.Address, Name = client.Name, IsOpen = client.IsOpen });
            }
        }

        private void RemoveItem(AlwaysOnListEntry item)
        {
            if (AlwaysOnListItems.Contains(item))
                AlwaysOnListItems.Remove(item);


            filesService.CleanAlwaysOnList();

            foreach (var item1 in AlwaysOnListItems)
            {
                if (!IsValidIPv4(item1.Address))
                {
                    MessageBox.Show("Not Valid " + item1.Address);
                    return;
                }
                filesService.AddToAlwaysOnList(item1.Address, item.IsOpen, item.Name);
            }
        }

        private void AddItem()
        {
            AlwaysOnListItems.Add(new AlwaysOnListEntry { Address = string.Empty, Name = string.Empty });
        }

        private void ShowWarrning()
        {
            var options = new GenericDialogOptions
            {
                Title = "Ip address error",
                Message = "כתובת ip לא הגיונית.",
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
            filesService.CleanAlwaysOnList();
            foreach (var client in AlwaysOnListItems)
            {
                if (client.Address == "" && client.Name == "") continue;
                if (!IsValidIPv4(client.Address))
                {
                    ShowWarrning();
                    return;
                }
                filesService.AddToAlwaysOnList(client.Address, client.IsOpen, client.Name);
            }


            List<Client> alwaysOnList = filesService.GetAlwaysOnList();
            AlwaysOnListItems.Clear();
            foreach (var ip in alwaysOnList)
            {
                AlwaysOnListEntry client = new();
                client.Address = ip.Address;
                client.Name = ip.Name;
                client.IsOpen = ip.IsOpen;
                AlwaysOnListItems.Add(client);
            }

            if (rdpService.LastAction == ActionsEnum.LocalComputersAndWhiteList)
            {
                rdpService.OpenRdpForLocalComputersAndForWhiteList();
            }
            else if (rdpService.LastAction == ActionsEnum.RemoteSystems)
            {
                rdpService.OpenRdpForAll();
            }
            else
            {
                rdpService.CloseRdpForAll();
            }

        }

        bool IsValidIPv4(string ipString)
        {
            return Regex.IsMatch(ipString, @"^((25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)(\.|$)){4}$");
        }


        private async Task<List<Client>> ScanNetworkAsync(string subnet, int timeout = 100)
        {
            List<Client> clients = new List<Client>();
            List<Task> pingTasks = new List<Task>();

            for (int i = 1; i < 255; i++)
            {
                string ip = subnet + i;
                pingTasks.Add(Task.Run(async () =>
                {
                    using Ping ping = new Ping();
                    try
                    {
                        PingReply reply = await ping.SendPingAsync(ip, timeout);
                        if (reply.Status == IPStatus.Success)
                        {
                            string hostName = "";
                            try
                            {
                                var entry = await Dns.GetHostEntryAsync(ip);
                                hostName = entry.HostName;
                            }
                            catch
                            {
                                hostName = "(no hostname)";
                            }

                            // Add to clients list
                            lock (clients)
                            {
                                clients.Add(new Client
                                {
                                    Address = ip,
                                    Name = hostName,
                                    IsOpen = true
                                });
                            }
                        }
                    }
                    catch
                    {
                        // Silent fail
                    }
                }));
            }

            await Task.WhenAll(pingTasks);

            return clients;
        }

    }
}
