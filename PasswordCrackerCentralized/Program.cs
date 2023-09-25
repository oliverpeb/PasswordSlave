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

            using (TcpClient client = new TcpClient(masterServerIp, masterServerPort))
            using (NetworkStream stream = client.GetStream())
            {
                while (true)
                {
                    // Read dictionary chunk from master server
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        // No more data, or connection closed by master
                        break;
                    }

                    string dictionaryChunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    File.WriteAllText("temp_chunk.txt", dictionaryChunk);

                    List<UserInfoClearText> results = cracker.RunCracking(); // get the results

                    // Convert results to a string and send them to the master
                    string resultsString = string.Join("\n", results);
                    byte[] resultsBytes = Encoding.UTF8.GetBytes(resultsString);
                    stream.Write(resultsBytes, 0, resultsBytes.Length);
                }
            }
        }
    }

}
