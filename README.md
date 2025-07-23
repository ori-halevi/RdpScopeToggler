# <!DOCTYPE html>

# <html lang="he" dir="rtl">

# <head>

# &nbsp;   <meta charset="UTF-8">

# &nbsp;   <meta name="viewport" content="width=device-width, initial-scale=1.0">

# &nbsp;   <title>RdpScopeToggler - Remote Desktop Access Control</title>

# &nbsp;   <style>

# &nbsp;       :root {

# &nbsp;           --primary-color: #2563eb;

# &nbsp;           --secondary-color: #1e40af;

# &nbsp;           --accent-color: #3b82f6;

# &nbsp;           --background-color: #f8fafc;

# &nbsp;           --card-background: #ffffff;

# &nbsp;           --text-primary: #1e293b;

# &nbsp;           --text-secondary: #64748b;

# &nbsp;           --border-color: #e2e8f0;

# &nbsp;           --success-color: #10b981;

# &nbsp;           --warning-color: #f59e0b;

# &nbsp;           --danger-color: #ef4444;

# &nbsp;           --shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);

# &nbsp;           --gradient: linear-gradient(135deg, #667eea 0%, #764ba2 100%);

# &nbsp;       }

# 

# &nbsp;       \* {

# &nbsp;           margin: 0;

# &nbsp;           padding: 0;

# &nbsp;           box-sizing: border-box;

# &nbsp;       }

# 

# &nbsp;       body {

# &nbsp;           font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;

# &nbsp;           line-height: 1.6;

# &nbsp;           color: var(--text-primary);

# &nbsp;           background: var(--background-color);

# &nbsp;           direction: ltr;

# &nbsp;       }

# 

# &nbsp;       .container {

# &nbsp;           max-width: 1200px;

# &nbsp;           margin: 0 auto;

# &nbsp;           padding: 0 20px;

# &nbsp;       }

# 

# &nbsp;       header {

# &nbsp;           background: var(--gradient);

# &nbsp;           color: white;

# &nbsp;           padding: 3rem 0;

# &nbsp;           text-align: center;

# &nbsp;           position: relative;

# &nbsp;           overflow: hidden;

# &nbsp;       }

# 

# &nbsp;       header::before {

# &nbsp;           content: '';

# &nbsp;           position: absolute;

# &nbsp;           top: 0;

# &nbsp;           left: 0;

# &nbsp;           right: 0;

# &nbsp;           bottom: 0;

# &nbsp;           background: rgba(0, 0, 0, 0.1);

# &nbsp;           z-index: 1;

# &nbsp;       }

# 

# &nbsp;       header .container {

# &nbsp;           position: relative;

# &nbsp;           z-index: 2;

# &nbsp;       }

# 

# &nbsp;       .header-content {

# &nbsp;           display: flex;

# &nbsp;           align-items: center;

# &nbsp;           justify-content: center;

# &nbsp;           gap: 2rem;

# &nbsp;           flex-wrap: wrap;

# &nbsp;       }

# 

# &nbsp;       .app-icon {

# &nbsp;           width: 80px;

# &nbsp;           height: 80px;

# &nbsp;           background: white;

# &nbsp;           border-radius: 16px;

# &nbsp;           padding: 12px;

# &nbsp;           box-shadow: var(--shadow);

# &nbsp;       }

# 

# &nbsp;       .app-icon img {

# &nbsp;           width: 100%;

# &nbsp;           height: 100%;

# &nbsp;           object-fit: contain;

# &nbsp;       }

# 

# &nbsp;       h1 {

# &nbsp;           font-size: 3rem;

# &nbsp;           font-weight: 700;

# &nbsp;           margin-bottom: 1rem;

# &nbsp;           text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3);

# &nbsp;       }

# 

# &nbsp;       .subtitle {

# &nbsp;           font-size: 1.25rem;

# &nbsp;           opacity: 0.9;

# &nbsp;           max-width: 600px;

# &nbsp;           margin: 0 auto;

# &nbsp;       }

# 

# &nbsp;       .section {

# &nbsp;           padding: 4rem 0;

# &nbsp;       }

# 

# &nbsp;       .section:nth-child(even) {

# &nbsp;           background: white;

# &nbsp;       }

# 

# &nbsp;       h2 {

# &nbsp;           font-size: 2.5rem;

# &nbsp;           color: var(--primary-color);

# &nbsp;           margin-bottom: 2rem;

# &nbsp;           text-align: center;

# &nbsp;           position: relative;

# &nbsp;       }

# 

# &nbsp;       h2::after {

# &nbsp;           content: '';

# &nbsp;           display: block;

# &nbsp;           width: 80px;

# &nbsp;           height: 4px;

# &nbsp;           background: var(--accent-color);

# &nbsp;           margin: 1rem auto;

# &nbsp;           border-radius: 2px;

# &nbsp;       }

# 

# &nbsp;       h3 {

# &nbsp;           font-size: 1.5rem;

# &nbsp;           color: var(--secondary-color);

# &nbsp;           margin-bottom: 1rem;

# &nbsp;           display: flex;

# &nbsp;           align-items: center;

# &nbsp;           gap: 0.5rem;

# &nbsp;       }

# 

# &nbsp;       .card {

# &nbsp;           background: var(--card-background);

# &nbsp;           border-radius: 12px;

# &nbsp;           padding: 2rem;

# &nbsp;           box-shadow: var(--shadow);

# &nbsp;           margin-bottom: 2rem;

# &nbsp;           transition: transform 0.3s ease, box-shadow 0.3s ease;

# &nbsp;       }

# 

# &nbsp;       .card:hover {

# &nbsp;           transform: translateY(-4px);

# &nbsp;           box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);

# &nbsp;       }

# 

# &nbsp;       .features-grid {

# &nbsp;           display: grid;

# &nbsp;           grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));

# &nbsp;           gap: 2rem;

# &nbsp;           margin-bottom: 3rem;

# &nbsp;       }

# 

# &nbsp;       .feature-card {

# &nbsp;           background: var(--card-background);

# &nbsp;           border-radius: 12px;

# &nbsp;           padding: 2rem;

# &nbsp;           box-shadow: var(--shadow);

# &nbsp;           transition: all 0.3s ease;

# &nbsp;           border-left: 4px solid var(--accent-color);

# &nbsp;       }

# 

# &nbsp;       .feature-card:hover {

# &nbsp;           transform: translateY(-4px);

# &nbsp;           box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1);

# &nbsp;       }

# 

# &nbsp;       .status-indicators {

# &nbsp;           display: grid;

# &nbsp;           grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));

# &nbsp;           gap: 1.5rem;

# &nbsp;           margin: 2rem 0;

# &nbsp;       }

# 

# &nbsp;       .status-card {

# &nbsp;           background: var(--card-background);

# &nbsp;           border-radius: 8px;

# &nbsp;           padding: 1.5rem;

# &nbsp;           box-shadow: var(--shadow);

# &nbsp;           text-align: center;

# &nbsp;           transition: all 0.3s ease;

# &nbsp;       }

# 

# &nbsp;       .status-card:hover {

# &nbsp;           transform: scale(1.05);

# &nbsp;       }

# 

# &nbsp;       .status-card.whitelist { border-top: 4px solid var(--primary-color); }

# &nbsp;       .status-card.general { border-top: 4px solid var(--success-color); }

# &nbsp;       .status-card.open { border-top: 4px solid var(--warning-color); }

# &nbsp;       .status-card.local { border-top: 4px solid var(--danger-color); }

# 

# &nbsp;       .status-emoji {

# &nbsp;           font-size: 2rem;

# &nbsp;           margin-bottom: 0.5rem;

# &nbsp;           display: block;

# &nbsp;       }

# 

# &nbsp;       .screenshots-grid {

# &nbsp;           display: grid;

# &nbsp;           grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));

# &nbsp;           gap: 2rem;

# &nbsp;           margin: 2rem 0;

# &nbsp;       }

# 

# &nbsp;       .screenshot-card {

# &nbsp;           background: var(--card-background);

# &nbsp;           border-radius: 12px;

# &nbsp;           overflow: hidden;

# &nbsp;           box-shadow: var(--shadow);

# &nbsp;           transition: all 0.3s ease;

# &nbsp;       }

# 

# &nbsp;       .screenshot-card:hover {

# &nbsp;           transform: scale(1.02);

# &nbsp;           box-shadow: 0 15px 30px -5px rgba(0, 0, 0, 0.15);

# &nbsp;       }

# 

# &nbsp;       .screenshot-card img {

# &nbsp;           width: 100%;

# &nbsp;           height: 200px;

# &nbsp;           object-fit: cover;

# &nbsp;           transition: transform 0.3s ease;

# &nbsp;       }

# 

# &nbsp;       .screenshot-card:hover img {

# &nbsp;           transform: scale(1.1);

# &nbsp;       }

# 

# &nbsp;       .screenshot-info {

# &nbsp;           padding: 1.5rem;

# &nbsp;       }

# 

# &nbsp;       .screenshot-title {

# &nbsp;           font-weight: 600;

# &nbsp;           color: var(--primary-color);

# &nbsp;           margin-bottom: 0.5rem;

# &nbsp;       }

# 

# &nbsp;       .installation-steps {

# &nbsp;           counter-reset: step-counter;

# &nbsp;       }

# 

# &nbsp;       .installation-step {

# &nbsp;           counter-increment: step-counter;

# &nbsp;           background: var(--card-background);

# &nbsp;           border-radius: 8px;

# &nbsp;           padding: 1.5rem;

# &nbsp;           margin-bottom: 1rem;

# &nbsp;           box-shadow: var(--shadow);

# &nbsp;           position: relative;

# &nbsp;           padding-left: 4rem;

# &nbsp;       }

# 

# &nbsp;       .installation-step::before {

# &nbsp;           content: counter(step-counter);

# &nbsp;           position: absolute;

# &nbsp;           left: 1rem;

# &nbsp;           top: 50%;

# &nbsp;           transform: translateY(-50%);

# &nbsp;           width: 2rem;

# &nbsp;           height: 2rem;

# &nbsp;           background: var(--primary-color);

# &nbsp;           color: white;

# &nbsp;           border-radius: 50%;

# &nbsp;           display: flex;

# &nbsp;           align-items: center;

# &nbsp;           justify-content: center;

# &nbsp;           font-weight: bold;

# &nbsp;       }

# 

# &nbsp;       .requirements-list {

# &nbsp;           list-style: none;

# &nbsp;       }

# 

# &nbsp;       .requirements-list li {

# &nbsp;           background: var(--card-background);

# &nbsp;           padding: 1rem 1.5rem;

# &nbsp;           margin-bottom: 0.5rem;

# &nbsp;           border-radius: 8px;

# &nbsp;           box-shadow: var(--shadow);

# &nbsp;           display: flex;

# &nbsp;           align-items: center;

# &nbsp;           gap: 1rem;

# &nbsp;       }

# 

# &nbsp;       .requirements-list li::before {

# &nbsp;           content: '✓';

# &nbsp;           color: var(--success-color);

# &nbsp;           font-weight: bold;

# &nbsp;           font-size: 1.2rem;

# &nbsp;       }

# 

# &nbsp;       .warning-box {

# &nbsp;           background: linear-gradient(135deg, #fef3c7, #fde68a);

# &nbsp;           border: 1px solid var(--warning-color);

# &nbsp;           border-radius: 8px;

# &nbsp;           padding: 1.5rem;

# &nbsp;           margin: 2rem 0;

# &nbsp;           position: relative;

# &nbsp;       }

# 

# &nbsp;       .warning-box::before {

# &nbsp;           content: '⚠️';

# &nbsp;           font-size: 1.5rem;

# &nbsp;           margin-right: 0.5rem;

# &nbsp;       }

# 

# &nbsp;       .warning-title {

# &nbsp;           font-weight: 600;

# &nbsp;           color: #92400e;

# &nbsp;           margin-bottom: 0.5rem;

# &nbsp;       }

# 

# &nbsp;       .btn {

# &nbsp;           display: inline-block;

# &nbsp;           padding: 12px 24px;

# &nbsp;           background: var(--primary-color);

# &nbsp;           color: white;

# &nbsp;           text-decoration: none;

# &nbsp;           border-radius: 8px;

# &nbsp;           font-weight: 600;

# &nbsp;           transition: all 0.3s ease;

# &nbsp;           border: none;

# &nbsp;           cursor: pointer;

# &nbsp;       }

# 

# &nbsp;       .btn:hover {

# &nbsp;           background: var(--secondary-color);

# &nbsp;           transform: translateY(-2px);

# &nbsp;           box-shadow: 0 5px 15px rgba(37, 99, 235, 0.3);

# &nbsp;       }

# 

# &nbsp;       .btn-secondary {

# &nbsp;           background: var(--text-secondary);

# &nbsp;       }

# 

# &nbsp;       .btn-secondary:hover {

# &nbsp;           background: var(--text-primary);

# &nbsp;       }

# 

# &nbsp;       footer {

# &nbsp;           background: var(--text-primary);

# &nbsp;           color: white;

# &nbsp;           text-align: center;

# &nbsp;           padding: 3rem 0;

# &nbsp;       }

# 

# &nbsp;       .footer-content {

# &nbsp;           display: grid;

# &nbsp;           grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));

# &nbsp;           gap: 2rem;

# &nbsp;           margin-bottom: 2rem;

# &nbsp;       }

# 

# &nbsp;       .footer-section h4 {

# &nbsp;           margin-bottom: 1rem;

# &nbsp;           color: var(--accent-color);

# &nbsp;       }

# 

# &nbsp;       .footer-section a {

# &nbsp;           color: #cbd5e1;

# &nbsp;           text-decoration: none;

# &nbsp;           transition: color 0.3s ease;

# &nbsp;       }

# 

# &nbsp;       .footer-section a:hover {

# &nbsp;           color: white;

# &nbsp;       }

# 

# &nbsp;       .copyright {

# &nbsp;           border-top: 1px solid #374151;

# &nbsp;           padding-top: 2rem;

# &nbsp;           color: #9ca3af;

# &nbsp;       }

# 

# &nbsp;       @media (max-width: 768px) {

# &nbsp;           h1 {

# &nbsp;               font-size: 2rem;

# &nbsp;           }

# 

# &nbsp;           h2 {

# &nbsp;               font-size: 2rem;

# &nbsp;           }

# 

# &nbsp;           .header-content {

# &nbsp;               flex-direction: column;

# &nbsp;               gap: 1rem;

# &nbsp;           }

# 

# &nbsp;           .app-icon {

# &nbsp;               width: 60px;

# &nbsp;               height: 60px;

# &nbsp;           }

# 

# &nbsp;           .section {

# &nbsp;               padding: 2rem 0;

# &nbsp;           }

# 

# &nbsp;           .installation-step {

# &nbsp;               padding-left: 1rem;

# &nbsp;           }

# 

# &nbsp;           .installation-step::before {

# &nbsp;               display: none;

# &nbsp;           }

# &nbsp;       }

# 

# &nbsp;       .emoji {

# &nbsp;           font-style: normal;

# &nbsp;       }

# 

# &nbsp;       code {

# &nbsp;           background: #f1f5f9;

# &nbsp;           padding: 0.2rem 0.4rem;

# &nbsp;           border-radius: 4px;

# &nbsp;           font-family: 'Consolas', 'Monaco', monospace;

# &nbsp;           color: var(--primary-color);

# &nbsp;       }

# 

# &nbsp;       .highlight {

# &nbsp;           background: linear-gradient(120deg, #a78bfa 0%, #ec4899 100%);

# &nbsp;           background-clip: text;

# &nbsp;           -webkit-background-clip: text;

# &nbsp;           -webkit-text-fill-color: transparent;

# &nbsp;           font-weight: 600;

# &nbsp;       }

# &nbsp;   </style>

# </head>

# <body>

# &nbsp;   <header>

# &nbsp;       <div class="container">

# &nbsp;           <div class="header-content">

# &nbsp;               <div class="app-icon">

# &nbsp;                   <img src="Assets/remote-desktop.png" alt="RdpScopeToggler Icon">

# &nbsp;               </div>

# &nbsp;               <div>

# &nbsp;                   <h1>RdpScopeToggler</h1>

# &nbsp;                   <p class="subtitle">Take full control of Remote Desktop Protocol (RDP) access to your computer with granular control over who can connect, when they can connect, and for how long.</p>

# &nbsp;               </div>

# &nbsp;           </div>

# &nbsp;       </div>

# &nbsp;   </header>

# 

# &nbsp;   <main>

# &nbsp;       <section id="features" class="section">

# &nbsp;           <div class="container">

# &nbsp;               <h2><span class="emoji">🚀</span> Features</h2>

# &nbsp;               

# &nbsp;               <div class="features-grid">

# &nbsp;                   <div class="feature-card">

# &nbsp;                       <h3><span class="emoji">🎯</span> Smart Access Control</h3>

# &nbsp;                       <ul>

# &nbsp;                           <li><strong>Local Address List:</strong> Define addresses that should always have access (marked as "Local" but supports both local and remote addresses)</li>

# &nbsp;                           <li><strong>Whitelist Management:</strong> Flexible whitelist system with enable/disable functionality</li>

# &nbsp;                           <li><strong>Time-Based Control:</strong> Set specific time windows for RDP access</li>

# &nbsp;                           <li><strong>Duration Limits:</strong> Configure maximum connection duration with automatic disconnection</li>

# &nbsp;                       </ul>

# &nbsp;                   </div>

# 

# &nbsp;                   <div class="feature-card">

# &nbsp;                       <h3><span class="emoji">📊</span> Real-Time Status Monitoring</h3>

# &nbsp;                       <p>The application provides four distinct status indicators to keep you informed:</p>

# &nbsp;                       

# &nbsp;                       <div class="status-indicators">

# &nbsp;                           <div class="status-card whitelist">

# &nbsp;                               <span class="status-emoji">📋</span>

# &nbsp;                               <div class="status-title"><strong>Whitelist Status</strong></div>

# &nbsp;                               <p>Shows current whitelist state</p>

# &nbsp;                           </div>

# &nbsp;                           <div class="status-card general">

# &nbsp;                               <span class="status-emoji">🌐</span>

# &nbsp;                               <div class="status-title"><strong>General List Status</strong></div>

# &nbsp;                               <p>Displays general access rules status</p>

# &nbsp;                           </div>

# &nbsp;                           <div class="status-card open">

# &nbsp;                               <span class="status-emoji">🔓</span>

# &nbsp;                               <div class="status-title"><strong>Open to All</strong></div>

# &nbsp;                               <p>Indicates when RDP is open to all addresses (no filtering)</p>

# &nbsp;                           </div>

# &nbsp;                           <div class="status-card local">

# &nbsp;                               <span class="status-emoji">🏠</span>

# &nbsp;                               <div class="status-title"><strong>Local Only</strong></div>

# &nbsp;                               <p>Shows when access is restricted to local addresses only</p>

# &nbsp;                           </div>

# &nbsp;                       </div>

# &nbsp;                   </div>

# 

# &nbsp;                   <div class="feature-card">

# &nbsp;                       <h3><span class="emoji">🔔</span> Automated Notifications</h3>

# &nbsp;                       <ul>

# &nbsp;                           <li><strong>Toast Listener Service:</strong> On first run, installs <code>RdpScopeTogglerToastListener</code></li>

# &nbsp;                           <li><strong>Pre-Disconnection Alerts:</strong> Receives notifications before scheduled disconnections</li>

# &nbsp;                           <li><strong>Always Scheduled:</strong> Every operation requires a scheduled disconnection time for security</li>

# &nbsp;                       </ul>

# &nbsp;                   </div>

# &nbsp;               </div>

# &nbsp;           </div>

# &nbsp;       </section>

# 

# &nbsp;       <section id="screenshots" class="section">

# &nbsp;           <div class="container">

# &nbsp;               <h2><span class="emoji">📸</span> Screenshots</h2>

# &nbsp;               

# &nbsp;               <div class="screenshots-grid">

# &nbsp;                   <div class="screenshot-card">

# &nbsp;                       <img src="Assets/Preview%20images/Main-removebg.png" alt="Main Interface">

# &nbsp;                       <div class="screenshot-info">

# &nbsp;                           <div class="screenshot-title">Main Interface</div>

# &nbsp;                           <p>The main control panel showing access lists and status indicators</p>

# &nbsp;                       </div>

# &nbsp;                   </div>

# &nbsp;                   

# &nbsp;                   <div class="screenshot-card">

# &nbsp;                       <img src="Assets/Preview%20images/Local-removebg.png" alt="Whitelist Management">

# &nbsp;                       <div class="screenshot-info">

# &nbsp;                           <div class="screenshot-title">Whitelist Management</div>

# &nbsp;                           <p>Configure and manage your whitelist addresses</p>

# &nbsp;                       </div>

# &nbsp;                   </div>

# &nbsp;                   

# &nbsp;                   <div class="screenshot-card">

# &nbsp;                       <img src="Assets/Preview%20images/Waiting-removebg.png" alt="Waiting Page">

# &nbsp;                       <div class="screenshot-info">

# &nbsp;                           <div class="screenshot-title">Waiting Page</div>

# &nbsp;                           <p>Real-time monitoring of RDP access status</p>

# &nbsp;                       </div>

# &nbsp;                   </div>

# &nbsp;                   

# &nbsp;                   <div class="screenshot-card">

# &nbsp;                       <img src="Assets/Preview%20images/AccessEnabled-removebg.png" alt="Access Enabled Page">

# &nbsp;                       <div class="screenshot-info">

# &nbsp;                           <div class="screenshot-title">Access Enabled Page</div>

# &nbsp;                           <p>Monitor access control and disconnection scheduling</p>

# &nbsp;                       </div>

# &nbsp;                   </div>

# &nbsp;               </div>

# &nbsp;           </div>

# &nbsp;       </section>

# 

# &nbsp;       <section id="installation" class="section">

# &nbsp;           <div class="container">

# &nbsp;               <h2><span class="emoji">🛠️</span> Installation</h2>

# &nbsp;               

# &nbsp;               <div class="installation-steps">

# &nbsp;                   <div class="installation-step">

# &nbsp;                       <strong>Download the latest release</strong> from the <a href="../../releases" class="btn">Releases page</a>

# &nbsp;                   </div>

# &nbsp;                   <div class="installation-step">

# &nbsp;                       <strong>Run the installer as Administrator</strong> to ensure proper permissions

# &nbsp;                   </div>

# &nbsp;                   <div class="installation-step">

# &nbsp;                       On first launch, the application will automatically install the <code>RdpScopeTogglerToastListener</code> service

# &nbsp;                   </div>

# &nbsp;                   <div class="installation-step">

# &nbsp;                       Configure your access lists and start managing RDP connections

# &nbsp;                   </div>

# &nbsp;               </div>

# &nbsp;           </div>

# &nbsp;       </section>

# 

# &nbsp;       <section id="requirements" class="section">

# &nbsp;           <div class="container">

# &nbsp;               <h2><span class="emoji">📋</span> Requirements</h2>

# &nbsp;               

# &nbsp;               <ul class="requirements-list">

# &nbsp;                   <li>Windows 10/11</li>

# &nbsp;                   <li>Administrator privileges (required for RDP configuration)</li>

# &nbsp;                   <li>.NET Framework 4.7.2 or higher</li>

# &nbsp;               </ul>

# &nbsp;           </div>

# &nbsp;       </section>

# 

# &nbsp;       <section id="usage" class="section">

# &nbsp;           <div class="container">

# &nbsp;               <h2><span class="emoji">🚦</span> How to Use</h2>

# &nbsp;               

# &nbsp;               <div class="card">

# &nbsp;                   <h3>Setting Up Access Lists</h3>

# &nbsp;                   

# &nbsp;                   <div class="features-grid">

# &nbsp;                       <div>

# &nbsp;                           <h4><span class="highlight">Local Address List</span></h4>

# &nbsp;                           <p>Add addresses that should always have RDP access:</p>

# &nbsp;                           <ul>

# &nbsp;                               <li>Despite the "Local" name, you can add both local and remote IP addresses</li>

# &nbsp;                               <li>These addresses bypass time restrictions</li>

# &nbsp;                           </ul>

# &nbsp;                       </div>

# &nbsp;                       

# &nbsp;                       <div>

# &nbsp;                           <h4><span class="highlight">Whitelist</span></h4>

# &nbsp;                           <p>Configure addresses with time-based access control:</p>

# &nbsp;                           <ul>

# &nbsp;                               <li>Enable/disable the entire whitelist as needed</li>

# &nbsp;                               <li>Set specific time windows for access</li>

# &nbsp;                               <li>Configure automatic disconnection times</li>

# &nbsp;                           </ul>

# &nbsp;                       </div>

# &nbsp;                   </div>

# &nbsp;               </div>

# &nbsp;               

# &nbsp;               <div class="card">

# &nbsp;                   <h3>Managing RDP Access</h3>

# &nbsp;                   <ul>

# &nbsp;                       <li><strong>Monitor Status:</strong> Use the four status indicators to track current access state</li>

# &nbsp;                       <li><strong>Schedule Disconnections:</strong> All access grants must include a scheduled end time</li>

# &nbsp;                       <li><strong>Receive Notifications:</strong> Get alerts before automatic disconnections occur</li>

# &nbsp;                   </ul>

# &nbsp;               </div>

# &nbsp;           </div>

# &nbsp;       </section>

# 

# &nbsp;       <section id="security" class="section">

# &nbsp;           <div class="container">

# &nbsp;               <h2><span class="emoji">⚠️</span> Important Security Notes</h2>

# &nbsp;               

# &nbsp;               <div class="warning-box">

# &nbsp;                   <div class="warning-title">Security Best Practices</div>

# &nbsp;                   <ul>

# &nbsp;                       <li><strong>Mandatory Scheduling:</strong> Every RDP access operation requires a scheduled disconnection time</li>

# &nbsp;                       <li><strong>Administrator Rights:</strong> The application requires admin privileges to modify RDP settings</li>

# &nbsp;                       <li><strong>Automatic Notifications:</strong> The toast listener ensures you're always aware of upcoming disconnections</li>

# &nbsp;                   </ul>

# &nbsp;               </div>

# &nbsp;           </div>

# &nbsp;       </section>

# 

# &nbsp;       <section id="technical" class="section">

# &nbsp;           <div class="container">

# &nbsp;               <h2><span class="emoji">🔧</span> Technical Details</h2>

# &nbsp;               

# &nbsp;               <div class="card">

# &nbsp;                   <ul>

# &nbsp;                       <li><strong>Primary Application:</strong> <code>RdpScopeToggler.exe</code></li>

# &nbsp;                       <li><strong>Notification Service:</strong> <code>RdpScopeTogglerToastListener</code> (auto-installed)</li>

# &nbsp;                       <li><strong>Configuration:</strong> Settings are stored locally and applied to Windows RDP configuration</li>

# &nbsp;                   </ul>

# &nbsp;               </div>

# &nbsp;           </div>

# &nbsp;       </section>

# 

# &nbsp;       <section id="support" class="section">

# &nbsp;           <div class="container">

# &nbsp;               <h2><span class="emoji">📞</span> Support</h2>

# &nbsp;               

# &nbsp;               <div class="card">

# &nbsp;                   <p>If you encounter any issues or have feature requests, please:</p>

# &nbsp;                   <div class="installation-steps">

# &nbsp;                       <div class="installation-step">

# &nbsp;                           Check the <a href="../../issues" class="btn btn-secondary">Issues page</a> for existing reports

# &nbsp;                       </div>

# &nbsp;                       <div class="installation-step">

# &nbsp;                           Create a new issue with detailed information about your problem

# &nbsp;                       </div>

# &nbsp;                       <div class="installation-step">

# &nbsp;                           Include your Windows version and any error messages

# &nbsp;                       </div>

# &nbsp;                   </div>

# &nbsp;               </div>

# &nbsp;           </div>

# &nbsp;       </section>

# &nbsp;   </main>

# 

# &nbsp;   <footer>

# &nbsp;       <div class="container">

# &nbsp;           <div class="footer-content">

# &nbsp;               <div class="footer-section">

# &nbsp;                   <h4>Documentation</h4>

# &nbsp;                   <p><a href="#features">Features</a></p>

# &nbsp;                   <p><a href="#installation">Installation</a></p>

# &nbsp;                   <p><a href="#usage">Usage Guide</a></p>

# &nbsp;               </div>

# &nbsp;               <div class="footer-section">

# &nbsp;                   <h4>Support</h4>

# &nbsp;                   <p><a href="../../issues">Report Issues</a></p>

# &nbsp;                   <p><a href="../../releases">Download</a></p>

# &nbsp;                   <p><a href="#requirements">Requirements</a></p>

# &nbsp;               </div>

# &nbsp;               <div class="footer-section">

# &nbsp;                   <h4>Legal</h4>

# &nbsp;                   <p><strong>License:</strong> This project is licensed under the MIT License</p>

# &nbsp;                   <p>See the <a href="LICENSE">LICENSE</a> file for details</p>

# &nbsp;               </div>

# &nbsp;               <div class="footer-section">

# &nbsp;                   <h4>Contributing</h4>

# &nbsp;                   <p>Contributions are welcome!</p>

# &nbsp;                   <p>Please feel free to submit a Pull Request</p>

# &nbsp;               </div>

# &nbsp;           </div>

# &nbsp;           

# &nbsp;           <div class="copyright">

# &nbsp;               <p><strong>Made with <span class="emoji">❤️</span> for secure remote desktop management</strong></p>

# &nbsp;           </div>

# &nbsp;       </div>

# &nbsp;   </footer>

# </body>

# </html>

