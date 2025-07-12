using Microsoft.Win32;
using System.Net.Sockets;
using System;
using System.Diagnostics;
using NetFwTypeLib;
using System.Linq;
using System.Collections.Generic;
using RdpScopeToggler.Services.FilesService;
using RdpScopeToggler.Stores;
using System.Data;

namespace RdpScopeToggler.Services.RdpService
{
    public class RdpService : IRdpService
    {
        public event Action RdpDataUpdated;
        public RdpInfoData RdpData { get; set; }

        public ActionsEnum LastAction { get; set; }

        public int? Port { get; set; }

        private readonly IFilesService _filesService;
        public RdpService(IFilesService filesService)
        {
            _filesService = filesService;
            RdpData = new RdpInfoData();
            LastAction = ActionsEnum.LocalComputersAndWhiteList;

            Port = GetRdpPort();
            if (Port == null)
            {
                throw new Exception("The port for RDP did not found!");
            }
        }

        #region Public methodes
        public void CloseRdpForAllIncludingAlwaysOnList()
        {
            ExecuteOnMatchingFirewallRules(
            (rule) =>
            {
                string ruleName = rule.Name;

                Debug.WriteLine($"Rule found: {ruleName}");

                rule.Enabled = false;

                Debug.WriteLine("Rule Disabled");
            });
        }

        public void CloseRdpForAll()
        {

            List<Client> alwaysOnList = _filesService
                .GetAlwaysOnList()
                .Where(client => client.IsOpen)
                .ToList();

            if (alwaysOnList.FirstOrDefault() == null)
            {
                CloseRdpForAllIncludingAlwaysOnList();
                return;
            }

            string result = string.Join(",", alwaysOnList
                .Select(client => $"{client.Address}/255.255.255.255"));


            ExecuteOnMatchingFirewallRules(
            (rule) =>
            {
                string ruleName = rule.Name;

                rule.Enabled = true;

                Debug.WriteLine($"Rule found: {ruleName}");
                Debug.WriteLine($"Current RemoteAddresses: {rule.RemoteAddresses}");

                rule.RemoteAddresses = result;
                Debug.WriteLine("Open for AlwaysOnList only!");
            });
        }

        public void OpenRdpForAll()
        {
            LastAction = ActionsEnum.RemoteSystems;
            ExecuteOnMatchingFirewallRules(
            (rule) =>
            {
                string ruleName = rule.Name;

                rule.Enabled = true;

                Debug.WriteLine($"Rule found: {ruleName}");
                Debug.WriteLine($"Current RemoteAddresses: {rule.RemoteAddresses}");

                rule.RemoteAddresses = "*";

                Debug.WriteLine("Changed to: * (Any)");
            });
        }

        public void OpenRdpForLocalComputers()
        {
            LastAction = ActionsEnum.LocalComputers;
            List<Client> alwaysOnList = _filesService
                .GetAlwaysOnList()
                .Where(client => client.IsOpen)
                .ToList();


            string result = string.Join(",", alwaysOnList
                .Select(client => $"{client.Address}/255.255.255.255"));

            string remoteAddresses = "192.168.0.0-192.168.255.255," + result;


            ExecuteOnMatchingFirewallRules(
            (rule) =>
            {
                string ruleName = rule.Name;

                rule.Enabled = true;

                Debug.WriteLine($"Rule found: {ruleName}");
                Debug.WriteLine($"Current RemoteAddresses: {rule.RemoteAddresses}");

                rule.RemoteAddresses = remoteAddresses;
                Debug.WriteLine("Changed to: 192.168.0.0-192.168.255.255");
            });
        }




        public void OpenRdpForWhiteList()
        {
            LastAction = ActionsEnum.WhiteList;
            List<Client> whiteList = _filesService.GetWhiteList();
            List<Client> alwaysOnList = _filesService
                .GetAlwaysOnList()
                .Where(client => client.IsOpen)
                .ToList();


            List<Client> combinedList = whiteList.Concat(alwaysOnList).ToList();

            if (combinedList.FirstOrDefault() == null)
            {
                CloseRdpForAllIncludingAlwaysOnList();
                LastAction = ActionsEnum.WhiteList;
                return;
            }

            string result = string.Join(",", combinedList
                .Select(client => $"{client.Address}/255.255.255.255"));


            ExecuteOnMatchingFirewallRules(
            (rule) =>
            {
                string ruleName = rule.Name;

                rule.Enabled = true;

                Debug.WriteLine($"Rule found: {ruleName}");
                Debug.WriteLine($"Current RemoteAddresses: {rule.RemoteAddresses}");

                rule.RemoteAddresses = result;
                Debug.WriteLine("Changed to WhiteList");
            });
        }


        public void OpenRdpForLocalComputersAndForWhiteList()
        {
            LastAction = ActionsEnum.LocalComputersAndWhiteList;
            List<Client> whiteList = _filesService.GetWhiteList();
            List<Client> alwaysOnList = _filesService
                .GetAlwaysOnList()
                .Where(client => client.IsOpen)
                .ToList();


            List<Client> combinedList = whiteList.Concat(alwaysOnList).ToList();

            string result = string.Join(",", combinedList
                .Select(client => $"{client.Address}/255.255.255.255"));


            string remoteAddresses = "192.168.0.0-192.168.255.255," + result;

            ExecuteOnMatchingFirewallRules(rule =>
            {
                rule.Enabled = true;
                rule.RemoteAddresses = remoteAddresses;

                Debug.WriteLine($"Rule found: {rule.Name}");
                Debug.WriteLine($"Changed to: {remoteAddresses}");
            });
        }







        public void RefreshRdpData()
        {
            ExecuteOnMatchingFirewallRules((rule) => { });
        }

        public int? GetRdpPort()
        {
            try
            {
                // קריאה מהרישום - כאן שמור הפורט של RDP
                const string keyPath = @"SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        object portNumber = key.GetValue("PortNumber");
                        if (portNumber != null)
                        {
                            int port = Convert.ToInt32(portNumber);
                            Debug.WriteLine($"RDP Port (from registry): {port}");

                            return port;
                        }
                        else
                        {
                            Debug.WriteLine("Couldn't find PortNumber in the registry.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Couldn't open the registry key.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                throw new Exception(ex.Message);
            }
            return null;
        }
        public bool IsRdpPortOpenForLocalhost()
        {
            // בדיקה אם הפורט פתוח על localhost
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect("127.0.0.1", (int)Port);
                    Debug.WriteLine("RDP port is OPEN and accepting connections.");
                    return true;
                }
                catch (SocketException)
                {
                    Debug.WriteLine("RDP port is CLOSED or blocked by firewall.");
                    return false;
                }
            }
        }
        #endregion


        private void UpdateRdpInfoData(INetFwRule rule)
        {
            List<Client> whiteList = _filesService.GetWhiteList();
            List<Client> alwaysOnList = _filesService.GetAlwaysOnList().Where(c => c.IsOpen).ToList();

            if (rule.Enabled)
                RdpData.IsRoleActive = true;

            RdpData.IsOpenForLocalComputers = rule.RemoteAddresses == "*" || rule.RemoteAddresses.Contains("192.168.0.0-192.168.255.255");

            RdpData.IsOpenForAll = rule.RemoteAddresses == "*";


            if (whiteList.Any())
            {
                string result = string.Join(",", whiteList.Select(c => $"{c.Address}/255.255.255.255"));
                string expected = $"192.168.0.0-192.168.255.255,{result}";

                var remoteAddressesList = SplitAndOrder(rule.RemoteAddresses);
                var whiteListAfter = SplitAndOrder(expected);
                
                RdpData.IsOpenForLocalComputersAndForWhiteList = (whiteListAfter.All(item => remoteAddressesList.Contains(item)) || rule.RemoteAddresses == "*");
            }

            if (whiteList.Any())
            {
                string expected = string.Join(",", whiteList.Select(c => $"{c.Address}/255.255.255.255"));

                var remoteAddressesList = SplitAndOrder(rule.RemoteAddresses);
                var whiteListAfter = SplitAndOrder(expected);

                RdpData.IsOpenForWhiteList = (whiteListAfter.All(item => remoteAddressesList.Contains(item)) || rule.RemoteAddresses == "*");
            }
            else
            {
                RdpData.IsOpenForWhiteList = rule.RemoteAddresses == "*";
            }

            if (alwaysOnList.Any())
            {
                var remoteAddressesList = SplitAndOrder(rule.RemoteAddresses);
                string result = string.Join(",", alwaysOnList.Select(c => $"{c.Address}/255.255.255.255"));
                var remoteAddressesAfter = SplitAndOrder(result);
                RdpData.IsOpenForAlwaysOnList = (remoteAddressesAfter.All(c => remoteAddressesList.Contains(c)) || rule.RemoteAddresses == "*");
            }
        }

        // Helper method to split, trim and order addresses
        private List<string> SplitAndOrder(string addresses)
        {
            return addresses
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .OrderBy(x => x)
                .ToList();
        }





        private void ExecuteOnMatchingFirewallRules(Action<INetFwRule> action)
        {
            RdpData.IsRoleActive = false;
            RdpData.IsOpenForAlwaysOnList = false;
            RdpData.IsOpenForLocalComputers = false;
            RdpData.IsOpenForAll = false;
            RdpData.IsOpenForLocalComputersAndForWhiteList = false;

            string protocolTcp = "6"; // TCP protocol number
            bool didFound = false;

            var fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            foreach (INetFwRule rule in fwPolicy2.Rules)
            {
                // נוודא שזה חוק TCP עם פורט תואם
                if (rule.Protocol.ToString() == protocolTcp &&
                    rule.LocalPorts != null &&
                    rule.LocalPorts.Split(',').Any(p => p.Trim() == Port.ToString()))
                {
                    didFound = true;
                    action(rule);
                    UpdateRdpInfoData(rule);
                }
            }

            if (!didFound)
                throw new Exception($"No firewall rule found for port {Port}");
            RdpDataUpdated?.Invoke();
        }
    }
}
