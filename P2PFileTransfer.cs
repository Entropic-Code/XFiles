using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
public class P2PFileTransfer
{

    static string FormatFileSize(long fileSize)
    {
        return fileSize >= 1024L * 1024L * 1024L
            ? $"{fileSize / (1024.0 * 1024.0 * 1024.0):F2}GB"
            : fileSize >= 1024L * 1024L
                ? $"{fileSize / (1024.0 * 1024.0):F2}MB"
                : fileSize >= 1024L
                    ? $"{fileSize / 1024.0:F2}KB"
                    : $"{fileSize}B";
    }

    //public string PeerIpAddressBase64()
    //{
    //    return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Dns.GetHostName()));
    //}

    //public string ReturnPeerIpFromBase64(string base64String)
    //{
    //    byte[] data = Convert.FromBase64String(base64String);
    //    return System.Text.Encoding.UTF8.GetString(data);
    //}
    public async Task SendFileAsync(string peerIp, int port, string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException("The specified file does not exist.", filePath);

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        TcpClient client = new TcpClient();
        Console.Write("Waiting for Peer...");

        await client.ConnectAsync(peerIp, port);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("\rPeer connected.    ");
        Console.WriteLine();

        NetworkStream ns = client.GetStream();

        FileInfo fi = new FileInfo(filePath);
        string metadata = $"{fi.Name}|{fi.Length}";
        byte[] metadataBytes = System.Text.Encoding.UTF8.GetBytes(metadata + "\n");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Sending File: {fi.Name}, Size: {FormatFileSize(fi.Length)}");

        await ns.WriteAsync(metadataBytes, 0, metadataBytes.Length);

        using (StreamReader nsr = new StreamReader(ns, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Waiting for peer to accept file transfer...");
            string? response = await nsr.ReadLineAsync();
            if (response == "REJECT")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Peer rejected the file transfer.");
                client.Close();
                return;
            }
            else if (response != "ACCEPT")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unexpected response from peer.");
                client.Close();
                return;
            }
        }

        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Sending Data.");

        byte[] buffer = new byte[8192];
        long totalBytesSent = 0;
        int bytesSent;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        int barLength = 50;
        char[] spinner = new char[] { '|', '/', '-', '\\' };

        while ((bytesSent = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await ns.WriteAsync(buffer, 0, bytesSent);

            totalBytesSent += bytesSent;
            Console.ForegroundColor = ConsoleColor.Yellow;
            float speedmbs = (float)(((totalBytesSent + 1) / (1024.0 * 1024.0)) / (stopwatch.Elapsed.TotalSeconds + 0.1));
            int progress = (int)((totalBytesSent * 100) / fi.Length);
            TimeSpan elapsed = stopwatch.Elapsed;
            string elapsedFormatted = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            double remainingSeconds = (fi.Length - totalBytesSent) / ((speedmbs * 1024 * 1024) + 1);
            TimeSpan remaining = TimeSpan.FromSeconds(remainingSeconds);
            string remainingFormatted = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            int filledLength = (int)((progress / 100.0) * barLength);
            string bar = new string('#', filledLength) + new string('-', barLength - filledLength);

            Console.Write($"\r  Elapsed: {elapsedFormatted} | Speed: {speedmbs:F2} MB/s | Remaining: {remainingFormatted}     ");
            Console.WriteLine();
            Console.Write($"[{bar}] {progress}% Complete {spinner[(int)(stopwatch.ElapsedMilliseconds / 100) % spinner.Length]}   ");
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("\r100% Complete                                                                                        ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        //await fs.CopyToAsync(ns);

        Console.WriteLine("File sent successfully.                                                                      ");
        client.Dispose();
        ns.Dispose();
        fs.Dispose();
    }

    public async Task ReceiveFileAsync(int port, string outputPath)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.Write($"Listening on port {port}...");

        TcpClient client = await listener.AcceptTcpClientAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"\rClient connected on port {port}.");
        Console.WriteLine();
        NetworkStream ns = client.GetStream();
        string fileName = "";
        long fileSize = 0;

        using (StreamReader nsr = new StreamReader(ns, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            string? metadataline = await nsr.ReadLineAsync();
            if (metadataline == null)
                throw new IOException("Failed to read metadata from the network stream.");
            string[] metadataParts = metadataline.Split('|');
            fileName = metadataParts[0];
            fileSize = long.Parse(metadataParts[1]);
        }

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Incoming file: {fileName}, Size: {FormatFileSize(fileSize)}");

        using (StreamWriter nsw = new StreamWriter(ns, System.Text.Encoding.UTF8, leaveOpen: true) { AutoFlush = true })
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Do you want to accept this file? (y/n): ");
            //Console.WriteLine();
            for (int i = 0; i < 3; i++) Console.Beep(); Thread.Sleep(100);
            string? response = Console.ReadLine();

            if (response?.ToLower() != "y")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await nsw.WriteLineAsync("REJECT");
                Console.WriteLine("File transfer rejected.");
                client.Close();
                listener.Stop();
                throw new OperationCanceledException("File transfer rejected.");
                //return;
            }

            await nsw.WriteLineAsync("ACCEPT");
        }

        Console.ForegroundColor = ConsoleColor.Blue;
        FileStream fs = new FileStream($"{outputPath}\\{fileName}", FileMode.Create, FileAccess.Write);
        Console.WriteLine("Receiving Data.");
        //await ns.CopyToAsync(fs);
        byte[] buffer = new byte[8192];
        long totalBytesRead = 0;
        int bytesRead;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        int barLength = 50;
        char[] spinner = new char[] { '|', '/', '-', '\\' };

        while ((bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fs.WriteAsync(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;
            Console.ForegroundColor = ConsoleColor.Yellow;
            float speedmbs = (float)(((totalBytesRead + 1) / (1024.0 * 1024.0)) / (stopwatch.Elapsed.TotalSeconds + 0.1));
            int progress = (int)((totalBytesRead * 100) / fileSize);
            TimeSpan elapsed = stopwatch.Elapsed;
            string elapsedFormatted = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            double remainingSeconds = (fileSize - totalBytesRead) / ((speedmbs * 1024 * 1024) + 1);
            TimeSpan remaining = TimeSpan.FromSeconds(remainingSeconds);
            string remainingFormatted = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            int filledLength = (int)((progress / 100.0) * barLength);
            string bar = new string('#', filledLength) + new string('-', barLength - filledLength);


            Console.Write($"\r  Elapsed: {elapsedFormatted} | Speed: {speedmbs:F2} MB/s | Remaining: {remainingFormatted}     ");
            Console.WriteLine();
            Console.Write($"[{bar}] {progress}% Complete {spinner[(int)(stopwatch.ElapsedMilliseconds / 100) % spinner.Length]}   ");
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("\r100% Complete                                                                                        ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        Console.WriteLine("File received successfully                                                                         ");
        Console.WriteLine($"at {outputPath}\\{fileName}");

        listener.Stop();
        listener.Dispose();
        client.Dispose();
        ns.Dispose();
        fs.Dispose();
    }
}


