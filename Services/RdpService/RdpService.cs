using Microsoft.Win32;
using System.Net.Sockets;
using System;
using System.Diagnostics;
using NetFwTypeLib;
using System.Linq;
using System.Windows;
using GraphicRdpScopeToggler.Services.FilesService;
using System.Collections.Generic;
using RdpScopeToggler.Stores;
using System.Data;

namespace GraphicRdpScopeToggler.Services.RdpService
{
    public class RdpService : IRdpService
    {
        public int? Port { get; set; }

        private readonly IFilesService _filesService;
        public RdpService(IFilesService filesService)
        {
            _filesService = filesService;

            Port = GetRdpPort();
            if (Port == null)
            {
                throw new Exception("The port for RDP did not found!");
            }

            _ = GetRdpInfoData();

        }


        public void CloseRdpForAll()
        {
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
                    string ruleName = rule.Name;
                    didFound = true;

                    Debug.WriteLine($"Rule found: {ruleName}");

                    rule.Enabled = false;

                    Debug.WriteLine("Rule Disabled");
                }
            }

            if (!didFound)
                MessageBox.Show($"No firewall rule found for port {Port}");
        }

        public void OpenRdpForAll()
        {
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
                    string ruleName = rule.Name;
                    didFound = true;

                    rule.Enabled = true;

                    Debug.WriteLine($"Rule found: {ruleName}");
                    Debug.WriteLine($"Current RemoteAddresses: {rule.RemoteAddresses}");

                    rule.RemoteAddresses = "*";

                    Debug.WriteLine("Changed to: * (Any)");
                }
            }

            if (!didFound)
                MessageBox.Show($"No firewall rule found for port {Port}");
        }

        public void OpenRdpForLocalComputers()
        {
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
                    string ruleName = rule.Name;
                    didFound = true;

                    rule.Enabled = true;

                    Debug.WriteLine($"Rule found: {ruleName}");
                    Debug.WriteLine($"Current RemoteAddresses: {rule.RemoteAddresses}");

                    rule.RemoteAddresses = "192.168.0.0-192.168.255.255";
                    Debug.WriteLine("Changed to: 192.168.0.0-192.168.255.255");
                }
            }

            if (!didFound)
                MessageBox.Show($"No firewall rule found for port {Port}");
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

        public void OpenRdpForWhiteList()
        {
            List<string> ipList = _filesService.GetWhiteList();
            if (ipList.FirstOrDefault() == null) { return; }
            string result = string.Join(",", ipList.Select(ip => $"{ip}/255.255.255.255"));

            int? port = GetRdpPort();
            if (port == null) return;
            string protocolTcp = "6"; // TCP protocol number
            bool didFound = false;

            var fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            foreach (INetFwRule rule in fwPolicy2.Rules)
            {
                // נוודא שזה חוק TCP עם פורט תואם
                if (rule.Protocol.ToString() == protocolTcp &&
                    rule.LocalPorts != null &&
                    rule.LocalPorts.Split(',').Any(p => p.Trim() == port.ToString()))
                {
                    string ruleName = rule.Name;
                    didFound = true;

                    rule.Enabled = true;

                    Debug.WriteLine($"Rule found: {ruleName}");
                    Debug.WriteLine($"Current RemoteAddresses: {rule.RemoteAddresses}");

                    rule.RemoteAddresses = result;
                    Debug.WriteLine("Changed to WhiteList");
                }
            }

            if (!didFound)
                MessageBox.Show($"No firewall rule found for port {port}");
        }

        public void OpenRdpForLocalComputersAndForWhiteList()
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

                string remoteAddresses = "192.168.0.0-192.168.255.255";
                remoteAddresses += ",";
                remoteAddresses += result;
                rule.RemoteAddresses = remoteAddresses;

                Debug.WriteLine("Changed to WhiteList");
            });
        }

        public RdpInfoData GetRdpInfoData()
        {
            RdpInfoData data = new RdpInfoData();

            if (false)
            {
                Debug.WriteLine("port is open!");
            }
            else
            {
                Debug.WriteLine("port is closed!");
            }
            return data;
        }




        private void ExecuteOnMatchingFirewallRules(Action<INetFwRule> action)
        {
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
                }
            }

            if (!didFound)
                throw new Exception($"No firewall rule found for port {Port}");
        }

    }
}
