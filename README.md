🖥️ Server Service – Word Guessing Game Server

📘 Overview

Server Service is a .NET Framework project designed to run as a Windows Service on a local or cloud server.
It acts as a centralized multiplayer word guessing game server, allowing multiple clients to connect, play, and compete in real time.

Players connect to the service through client applications, where they can guess words, receive feedback, and view game progress. The server manages all active sessions, tracks scores, and ensures synchronized gameplay across connected clients.

🚀 Features

🧩 Multiplayer Word Guessing Game — supports multiple simultaneous client connections.

⚙️ Windows Service Support — runs silently in the background as a managed Microsoft service.

☁️ Cloud-Ready Deployment — can be hosted on a cloud-based Windows Server instance.

🔐 Connection Handling — manages client sessions, message parsing, and graceful disconnects.

🗃️ Centralized Game Logic — server determines valid guesses, word validation, and scoring.

🧠 Configurable Word List — customizable dictionary or word source.

🛠️ Requirements

Operating System: Windows Server 2016 or later

Framework: .NET Framework 4.7.2 or later

Privileges: Administrator rights to install/start the service

Optional: Cloud hosting (e.g., AWS EC2, Azure VM, or Google Cloud Compute Engine)

⚙️ Installation
1. Build the Project

Open the solution in Visual Studio.

Set the configuration to Release.

Build the project to generate the .exe file in the bin/Release folder.

2. Install the Windows Service

Run the following command in PowerShell or Command Prompt (Admin):

sc create "ServerService" binPath= "C:\Path\To\ServerService.exe" start= auto


To start the service:

net start ServerService


To stop or remove:

net stop ServerService
sc delete ServerService

🌐 Connecting Clients

Clients connect to the server via TCP/IP sockets on the configured port (default: 5000).

Once connected, clients can:

Join an active game

Submit word guesses

Receive real-time feedback and score updates

🧩 Game Flow

Server selects a random secret word.

Players submit guesses over the network.

Server validates guesses and sends back hints or score updates.

The game continues until a player correctly guesses the word.

Results are broadcast to all clients.

🧾 Configuration

Configuration options are typically stored in App.config:

<appSettings>
  <add key="Port" value="5000"/>
  <add key="WordListPath" value="C:\ServerService\words.txt"/>
</appSettings>

🔍 Logging

Logs are stored locally or remotely depending on configuration:

Service startup/shutdown events

Client connection/disconnection

Game session events (round start, guess attempts, results)

☁️ Deployment on Cloud

To run on the cloud:

Deploy the compiled service to your Windows Server instance.

Open the configured TCP port in your firewall/security group.

Start the service and verify logs for successful initialization.

🧪 Testing

You can test the server locally by:

Running the service

Connecting via a simple TCP client or your custom game client

Sending mock game requests and verifying responses

👨‍💻 Author

Jaykumar Patel
📧 Jaykumar2005patel@gmail.com
