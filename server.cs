using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;


namespace ServicesA07
{
    public class ServerService : ServiceBase
    {
        private TcpListener listener;
        private bool isRunning;
        private ConcurrentDictionary<string, GameSession> sessions;

        public ServerService()
        {
            ServiceName = "GameServerService";
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                string ipAddressString = ConfigurationManager.AppSettings["IPAddress"];
                string port_ = ConfigurationManager.AppSettings["Port"];

                if (!IPAddress.TryParse(ipAddressString, out IPAddress ipAddress))
                {
                    Logger.Log("Invalid IP address in configuration.");
                    throw new Exception("Invalid IP address in configuration.");
                }

                if (!int.TryParse(port_, out int port))
                {
                    Logger.Log("Invalid port in configuration.");
                    throw new Exception("Invalid port in configuration.");
                }

                listener = new TcpListener(ipAddress, port);
                sessions = new ConcurrentDictionary<string, GameSession>();

                listener.Start();
                isRunning = true;
                Logger.Log($"Server started on IP {ipAddress} and port {port}.");

                Task.Run(() => AcceptClients());
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in OnStart: {ex.Message}");
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                isRunning = false;
                listener.Stop();
                Logger.Log("Server is stopping...");

                foreach (var session in sessions.Values)
                {
                    session.NotifyShutdown();
                }

                sessions.Clear();
                Logger.Log("Server has stopped successfully.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in OnStop: {ex.Message}");
                throw;
            }
        }

        private async Task AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync();
                    Task.Run(() => HandleClient(client));
                }
                catch
                {
                    if (!isRunning)
                        Logger.Log("Listener stopped.");
                    else
                        Logger.Log("Error in AcceptClients.");
                    break;
                }
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            var endpoint = client.Client.RemoteEndPoint.ToString();
            Logger.Log($"Client connected: {endpoint}");

            var session = new GameSession(client, endpoint);
            sessions[endpoint] = session;
            await session.StartSession();

            sessions.TryRemove(endpoint, out _);
            client.Close();
            Logger.Log($"Client disconnected: {endpoint}");
        }

        public static void Main(string[] args)
        {
            ServiceBase.Run(new ServerService());
        }
    }
}
