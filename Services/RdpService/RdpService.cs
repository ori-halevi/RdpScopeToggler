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
        public int? Port { get; set; }

        private readonly IFilesService _filesService;
        public RdpService(IFilesService filesService)
        {
            _filesService = filesService;
            RdpData = new RdpInfoData();

            Port = GetRdpPort();
            if (Port == null)
            {
                throw new Exception("The port for RDP did not found!");
            }
        }

        #region Public methodes
        public void CloseRdpForAll()
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

        public void OpenRdpForAll()
        {
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
            ExecuteOnMatchingFirewallRules(
            (rule) =>
            {
                string ruleName = rule.Name;

                rule.Enabled = true;

                Debug.WriteLine($"Rule found: {ruleName}");
                Debug.WriteLine($"Current RemoteAddresses: {rule.RemoteAddresses}");

                rule.RemoteAddresses = "192.168.0.0-192.168.255.255";
                Debug.WriteLine("Changed to: 192.168.0.0-192.168.255.255");
            });
        }




        public void OpenRdpForWhiteList()
        {
            List<string> ipList = _filesService.GetWhiteList();
            if (ipList.FirstOrDefault() == null) { return; }
            string result = string.Join(",", ipList.Select(ip => $"{ip}/255.255.255.255"));

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
            var ipList = _filesService.GetWhiteList();
            if (ipList.FirstOrDefault() == null) return;

            string remoteAddresses = "192.168.0.0-192.168.255.255," +
                                     string.Join(",", ipList.Select(ip => $"{ip}/255.255.255.255"));

            ExecuteOnMatchingFirewallRules(rule =>
            {
                rule.Enabled = true;
                rule.RemoteAddresses = remoteAddresses;

                Debug.WriteLine($"Rule found: {rule.Name}");
                Debug.WriteLine($"Changed to WhiteList: {remoteAddresses}");
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



        private void UpdateRdpData(INetFwRule rule)
        {
            if (rule.Enabled)
                RdpData.IsRoleActive = true;
            if (rule.RemoteAddresses == "192.168.0.0-192.168.255.255")
                RdpData.IsOpenOnlyForLocal = true;
            if (rule.RemoteAddresses == "*")
                RdpData.IsOpenForAll = true;

            var ipList = _filesService.GetWhiteList();
            if (ipList.FirstOrDefault() != null)
            {
                string expected = "192.168.0.0-192.168.255.255," +
                                  string.Join(",", ipList.Select(ip => $"{ip}/255.255.255.255"));

                var listA = rule.RemoteAddresses.Split(',').Select(x => x.Trim()).OrderBy(x => x).ToList();
                var listB = expected.Split(',').Select(x => x.Trim()).OrderBy(x => x).ToList();

                if (listA.SequenceEqual(listB))
                    RdpData.IsOpenForLocalComputersAndForWhiteList = true;

            }
        }



        private void ExecuteOnMatchingFirewallRules(Action<INetFwRule> action)
        {
            RdpData.IsRoleActive = false;
            RdpData.IsOpenOnlyForLocal = false;
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
                    UpdateRdpData(rule);
                }
            }

            if (!didFound)
                throw new Exception($"No firewall rule found for port {Port}");
            RdpDataUpdated?.Invoke();
        }
    }
}
