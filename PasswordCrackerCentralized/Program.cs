using PasswordCrackerCentralized.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace PasswordCrackerCentralized
{
    class Program
    {
        static void Main()
        {
            const string masterServerIp = "172.20.10.2";
            const int masterServerPort = 12345;

            Cracking cracker = new Cracking();

            try
            {
                using (TcpClient client = new TcpClient(masterServerIp, masterServerPort))
                using (NetworkStream stream = client.GetStream())
                {
                    while (true)
                    {
                        // Read dictionary chunk from master server
                        byte[] buffer = new byte[1024];
                        StringBuilder dataBuilder = new StringBuilder();

                        int bytesRead;
                        do
                        {
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                            dataBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                        }
                        while (stream.DataAvailable);

                        string dictionaryChunk = dataBuilder.ToString();
                        if (string.IsNullOrEmpty(dictionaryChunk)) break;

                        File.WriteAllText("temp_chunk.txt", dictionaryChunk);

                        List<UserInfoClearText> results = cracker.RunCracking(dictionaryChunk); // get the results

                        // Convert results to a string and send them to the master
                        string resultsString = string.Join("\n", results);
                        byte[] resultsBytes = Encoding.UTF8.GetBytes(resultsString);
                        stream.Write(resultsBytes, 0, resultsBytes.Length);
                    }
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine($"SocketException: {se.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
        }
    }
}
