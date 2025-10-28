# \# RdpScopeToggler

# 

# <img src="Assets/remote-desktop.png" alt="RdpScopeToggler Icon" width="64" height="64">

# 

# \*\*RdpScopeToggler\*\* is a powerful Windows application that provides granular control over Remote Desktop Protocol (RDP) access to your computer. Take full control of who can connect to your machine, when they can connect, and for how long.

# 

# \## 🚀 Features

# 

# \### Smart Access Control

# \- \*\*Trusted Address List\*\*: Define addresses that should always have access.

# \- \*\*Whitelist Management\*\*: Flexible whitelist system with enable/disable functionality

# \- \*\*Time-Based Control\*\*: Set specific time windows for RDP access

# \- \*\*Duration Limits\*\*: Configure maximum connection duration with automatic disconnection

# 

# \### Real-Time Status Monitoring

# The application provides four distinct status indicators to keep you informed:

# 

# 1\. \*\*📋 Whitelist Status\*\* - Shows current whitelist state

# 2\. \*\*🌐 Trusted Address List Status\*\* - Displays Trusted Address access rules status  

# 3\. \*\*🔓 Open to All\*\* - Indicates when RDP is open to all addresses (no filtering)

# 4\. \*\*🏠 Local\*\* - Shows when access is open for local addresses

# 

# \## 📸 Screenshots

# 

# \### Main Interface

# <img src="Assets/Preview%20images/Main-removebg.png" alt="Main Interface" style="max-width: 100%; height: auto;">

# 

# \*The main control panel showing access lists and status indicators\*

# 

# \### Whitelist Management

# <img src="Assets/Preview%20images/Local-removebg.png" alt="Whitelist Management" style="max-width: 100%; height: auto;">

# 

# \*Configure and manage your whitelist addresses\*

# 

# \### Waiting Page

# <img src="Assets/Preview%20images/Waiting-removebg.png" alt="Status Indicators" style="max-width: 100%; height: auto;">

# 

# \*Real-time monitoring of RDP access status\*

# 

# \### Access enabled Page

# <img src="Assets/Preview%20images/AccessEnabled-removebg.png" alt="Scheduling Interface" style="max-width: 100%; height: auto;">

# 

# \*Monitor access control and disconnection\*

# 

# \## 🛠️ Installation

# 

# 1\. Download the latest release from the \[Releases](../../releases) page

# 2\. Run the installer as Administrator

# 3\. On first launch, the application will automatically install the `RdpScopeTogglerToastListener` service

# 4\. Configure your access lists and start managing RDP connections

# 

# \## 📋 Requirements

# 

# \- Windows 10/11

# \- Administrator privileges (required for RDP configuration)

# 

# \## 🚦 How to Use

# 

# \### Setting Up Access Lists (There may be unexpected behavior if the lists are empty.)

# 

# 1\. \*\*Trusted Address List\*\*: Add addresses that should always have RDP access

# &nbsp;  - These addresses bypass time restrictions

# 

# 2\. \*\*Whitelist\*\*: Configure addresses with time-based access control

# &nbsp;  - Enable/disable the entire whitelist as needed

# &nbsp;  - Set specific time windows for access

# &nbsp;  - Configure automatic disconnection times

# 

# \### Managing RDP Access

# 

# \- \*\*Monitor Status\*\*: Use the four status indicators to track current access state

# \- \*\*Schedule Disconnections\*\*: All access grants must include a scheduled end time

# \- \*\*Receive Notifications\*\*: Get alerts before automatic disconnections occur

# 

# \## ⚠️ Important Security Notes

# 

# \- \*\*Mandatory Scheduling\*\*: Every RDP access operation requires a scheduled disconnection time

# \- \*\*Administrator Rights\*\*: The application requires admin privileges to modify RDP settings

# \- \*\*Important\*\*: There may be unexpected behavior if the lists are empty so fill them with some addresses.

# 

# \## 🔧 Technical Details

# 

# \- \*\*Primary Application\*\*: RdpScopeToggler.exe

# \- \*\*Actions Service\*\*: RdpScopeService (auto-installed)

# \- \*\*Configuration\*\*: Settings are stored locally and applied to Windows RDP configuration

# 

# \## 📞 Support

# 

# If you encounter any issues or have feature requests, please:

# 

# 1\. Check the \[Issues](../../issues) page for existing reports

# 2\. Create a new issue with detailed information about your problem

# 3\. Include your Windows version and any error messages

# 

# \## 📄 License

# 

# This project is licensed under the MIT License - see the \[LICENSE](LICENSE) file for details.

# 

# \## 🤝 Contributing

# 

# Contributions are welcome! Please feel free to submit a Pull Request.

# 

# ---

# 

# \*\*Made with ❤️ for secure remote desktop management\*\*

