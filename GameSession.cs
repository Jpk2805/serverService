using ServicesA07;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServicesA07
{
    internal class GameSession
    {

        private TcpClient client;
        private string clientId;
        private NetworkStream stream;

        private string characterString;
        private int totalWords;
        private HashSet<string> validWords;
        private HashSet<string> guessedWords;

        private bool isPlaying;

        public GameSession(TcpClient client, string clientId)
        {
            this.client = client;
            this.clientId = clientId;
            this.stream = client.GetStream();
            this.guessedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.validWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.isPlaying = true;
        }



        internal async Task StartSession()
        {
            try
            {
                LoadGameData();
                Logger.Log($"Game session started for client {clientId}.");

                await SendMessage($"START_GAME|{this.characterString}|{this.totalWords}");

                while (isPlaying)
                {
                    string message = await ReceiveMessage();

                    if (message.StartsWith("GUESS|"))
                    {
                        string word = message.Substring(6).Trim();
                        HandleGuess(word);
                    }
                    else if (message.StartsWith("END_GAME"))
                    {
                        await ConfirmEndGame();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error with client {clientId}: {ex.Message}");
            }
        }


        private void LoadGameData()
        {
            string gameDataDirectory = ConfigurationManager.AppSettings["GameDataDirectory"];
            if (string.IsNullOrEmpty(gameDataDirectory))
            {
                throw new Exception("GameDataDirectory is not configured in app.config.");
            }

            var files = Directory.GetFiles(gameDataDirectory);
            if (files.Length == 0)
            {
                throw new Exception($"No game data files found in the '{gameDataDirectory}' directory.");
            }

            var random = new Random();
            var file = files[random.Next(files.Length)];

            var lines = File.ReadAllLines(file);
            if (lines.Length < 3)
            {
                throw new Exception($"Game data file {file} is not in the correct format.");
            }

            characterString = lines[0];
            if (characterString.Length != 80)
            {
                throw new Exception($"Character string in {file} does not have 80 characters.");
            }

            if (!int.TryParse(lines[1], out totalWords))
            {
                throw new Exception($"Total words count in {file} is not a valid integer.");
            }

            for (int i = 2; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    validWords.Add(lines[i].Trim());
                }
            }

            Logger.Log($"Game data loaded from '{file}' for client {clientId}.");
        }


        private void HandleGuess(string word)
        {
            string status;
            if (validWords.Contains(word) && !guessedWords.Contains(word))
            {
                guessedWords.Add(word);
                status = "correct";
            }
            else
            {
                status = "incorrect";
            }

            SendMessage($"FEEDBACK|{status}|{guessedWords.Count}|{totalWords - guessedWords.Count}").Wait();

            if (guessedWords.Count >= totalWords)
            {
                EndGame(true).Wait();
            }
        }
        private async Task ConfirmEndGame()
        {
            await SendMessage("CONFIRM_END");
            string response = await ReceiveMessage();

            if (response.StartsWith("CONFIRM_END|"))
            {
                string confirmed = response.Substring(12).Trim().ToLower();
                if (confirmed == "yes")
                {
                    await EndGame(false);
                }
            }
        }
        private async Task EndGame(bool won)
        {
            await SendMessage($"GAME_OVER|{won}|{guessedWords.Count}");

            await SendMessage("PLAY_AGAIN_PROMPT");

            string response = await ReceiveMessage();
            if (response.StartsWith("PLAY_AGAIN|"))
            {
                string playAgain = response.Substring(10).Trim().ToLower();
                if (playAgain == "yes")
                {
                    guessedWords.Clear();
                    validWords.Clear();
                    LoadGameData();

                    await SendMessage($"START_GAME|{this.characterString}|{this.totalWords}");
                }
                else
                {
                    isPlaying = false;
                }
            }
        }

        public async void NotifyShutdown()
        {
            await SendMessage("SERVER_SHUTDOWN");
        }

        private async Task<string> ReceiveMessage()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
                throw new Exception("Client disconnected.");

            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
            Logger.Log($"Received from {clientId}: {message}");
            return message;
        }

        private async Task SendMessage(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message + "\n");
            await stream.WriteAsync(data, 0, data.Length);
            Logger.Log($"Sent to {clientId}: {message}");
        }
    }
}