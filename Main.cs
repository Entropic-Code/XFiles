using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;



{

    if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XFiles"))
    {
        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XFiles");
    }
    string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XFiles\\Nicknames.cfg";
    if (!File.Exists(filePath))
    {
        File.Create(filePath).Close();
        FileSaveHandler handler = new FileSaveHandler();
        Dictionary<string, string> nicknames = new Dictionary<string, string>();
        handler.SaveDictionary(filePath, nicknames);
    }
    filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XFiles\\Options.cfg";
    if (!File.Exists(filePath))
    {
        File.Create(filePath).Close();
        FileSaveHandler handler = new FileSaveHandler();
        Dictionary<string, string> options = new Dictionary<string, string>();
        options.Add("coolboot", "true");
        options.Add("port", "9000");
        handler.SaveDictionary(filePath, options);
    }
    {
        FileSaveHandler handler = new FileSaveHandler();
        handler.LoadDictionary(filePath);
        Dictionary<string, string> options = handler.LoadDictionary(filePath);

        if (options.Count < 2)
        {
            options.Clear();
            options.Add("coolboot", "true");
            options.Add("port", "9000");
            handler.SaveDictionary(filePath, options);
        }

        if (options.ContainsKey("coolboot"))
        {
            if (options["coolboot"].ToLower() == "true")
            {
                RunBootSequence();
            }
        }
    }

}

Console.Title = "XFiles";
while (true)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Welcome to XFiles-P2P file sharing solution!");
    Console.WriteLine("Type 'help' for commands");
    var Input = Console.ReadLine();
    if (Input != null)
    {
        switch (Input.ToLower())
        {
            case "help":
                {
                    Console.Clear();
                    Console.WriteLine("Exit     [alias: e] - closes the program");
                    Console.WriteLine("Send     [alias: s] - sends a file to a peer");
                    Console.WriteLine("Receive  [alias: r] - receives a file from a client");
                    Console.WriteLine("Address  [alias: a] - shows your public device address");
                    Console.WriteLine("Nickname [alias: n] - creates a alias given device address");
                    Console.WriteLine("options  [alias: o] - displays all current configs");
                    Console.ReadKey();
                }
                break;
            case "exit":
            case "e":
                {
                    Console.Clear();
                    Console.WriteLine("Closing...");
                    System.Environment.Exit(0);
                }
                break;
            case "send":
            case "s":
                {
                    Send();
                }
                break;
            case "receive":
            case "r":
                {
                    Receive();
                }
                break;
            case "address":
            case "a":
                {
                    Address().Wait();
                }
                break;
            case "nickname":
            case "n":
                {
                    Nickname();
                }
                break;
            case "options":
            case "o":
                {
                    Options();
                }
                break;
        }
    }
}

static void Options()
{
    Console.Clear();
    FileSaveHandler handler = new FileSaveHandler();
    string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XFiles\\Options.cfg";
    Dictionary<string, string> options = handler.LoadDictionary(filePath);
    Console.WriteLine("Configurable settings:");
    Console.WriteLine();

    foreach (KeyValuePair<string, string> op in options)
    {
        {
            Console.WriteLine($"- {op.Key} = {op.Value}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("Enter the name of the option to edit:");
    string Input = Console.ReadLine() + "";
    Input = Input.ToLower();

    if (options.ContainsKey(Input))
    {
        Console.WriteLine("Enter the new value:");
        string Input2 = Console.ReadLine() + "";
        Input2 = Input2.ToLower();
        options[Input] = Input2;
    }
    else
    {
        Console.WriteLine("Invalid option. Press any key to continue...");
        Console.ReadKey();
        return;
    }
    handler.SaveDictionary(filePath, options);
}
static void Nickname()
{
    string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Xfiles\\Nicknames.cfg";
    FileSaveHandler handler = new FileSaveHandler();
    Dictionary<string, string> nicknames = handler.LoadDictionary(filePath);
    Console.Clear();

    if (nicknames.Count > 0)
    {
        Console.WriteLine("Current Nicknames:");
        Console.WriteLine();

        foreach (KeyValuePair<string, string> n in nicknames)
        {
            Console.WriteLine($"- {n.Key} = {n.Value}");
        }

        Console.WriteLine();
    }

    Console.Write("Add or Remove (a/r): ");
    bool add = true;
    string AddRemove = Console.ReadLine() + "";
    if (AddRemove.ToLower() == "r")
    {
        add = false;
    }
    else if (AddRemove.ToLower() == "a")
    { }
    else
    {
        Console.WriteLine("Invalid option. Press any key to continue...");
        Console.ReadKey();
        return;
    }

    Console.WriteLine("Enter the nickname: ");
    string nicknameInput = Console.ReadLine() + "";

    if (add)
    {
        Console.WriteLine("Enter the device address: ");
        string addressInput = Console.ReadLine() + "";

        if (addressInput.Contains('.'))
        {
            SimpleAES AES = new SimpleAES();
            addressInput = AES.Encrypt(addressInput, "key");
        }
        if (!nicknames.ContainsKey(nicknameInput))
        {
            nicknames.Add(nicknameInput.ToLower(), addressInput);
        }
        else
        {
            Console.WriteLine("Nickname already exists. Overwrite? (y/n): ");
            string OverwriteInput = Console.ReadLine() + "";
            if (OverwriteInput.ToLower() != "y")
            {
                Console.WriteLine("Operation cancelled. Press any key to continue...");
                Console.ReadKey();
                return;
            }
            nicknames.Remove(nicknameInput);
            nicknames.Add(nicknameInput.ToLower(), addressInput);
        }
    }
    else
    {
        if (nicknames.ContainsKey(nicknameInput))
        {
            nicknames.Remove(nicknameInput);
        }
        else
        {
            Console.WriteLine("Nickname not found. Press any key to continue...");
            Console.ReadKey();
            return;
        }
    }

    handler.SaveDictionary(filePath, nicknames);
}

static void Receive()
{
    Console.Clear();
    try
    {
        P2PFileTransfer p2p = new P2PFileTransfer();
        p2p.ReceiveFileAsync(9000, getDownloadsPath()).Wait();
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Open downloads folder (y/n): ");
        Console.Beep();
        string key = Console.ReadLine() + "";
        if (key.ToLower() == "y")
        {
            System.Diagnostics.Process.Start("cmd", $"/C start {getDownloadsPath()}");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine(ex.Message);
        Console.ForegroundColor = ConsoleColor.White;
        for (int i = 0; i < 3; i++) Console.Beep();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}

static void Send()
{
    Console.Clear();
    Console.WriteLine("Enter the peer ip, device address or nickname:");
    string peerIp = Console.ReadLine() + "";
    Console.WriteLine("Enter the file path to send:");
    string filePath = Console.ReadLine() + "";

    if (filePath.StartsWith("\"") && filePath.EndsWith("\""))
    {
        filePath = filePath.Substring(1, filePath.Length - 2);
    }

    P2PFileTransfer p2p = new P2PFileTransfer();
    try
    {

        FileSaveHandler handler = new FileSaveHandler();
        string configfilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Xfiles\\Nicknames.cfg";

        {

            Dictionary<string, string> nicknames = handler.LoadDictionary(configfilePath);

            if (nicknames.ContainsKey(peerIp.ToLower()))
            {
                SimpleAES AES = new SimpleAES();
                peerIp = AES.Decrypt(nicknames[peerIp.ToLower()], "key");
            }
            else if (!peerIp.Contains('.'))
            {
                SimpleAES AES = new SimpleAES();
                peerIp = AES.Decrypt(peerIp, "key");
            }
        }
        
        configfilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Xfiles\\Options.cfg";
        Dictionary<string, string> options = handler.LoadDictionary(configfilePath);
        p2p.SendFileAsync(peerIp, Convert.ToInt32(options["port"]), filePath).Wait();
        Console.Beep();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine(ex.Message);
        for (int i = 0; i < 3; i++) Console.Beep();
    }
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}
async Task Address()
{
    Console.Clear();
    Console.WriteLine("Public or Local Address");
    Console.Write("Type P for Public or L for Local: ");
    string key = Console.ReadLine() + "";
    SimpleAES AES = new SimpleAES();

    if (key.ToLower() == "l")
    {
        Console.Clear();

        Console.WriteLine("Do you use Wired(LAN) or Wireless connection");
        Console.Write("Type L for LAN or W for Wireless: ");
        key = Console.ReadLine() + "";
        Console.Clear();
        if (key.ToLower() == "l")
        {
            Console.WriteLine("Your device address is:");
            Console.WriteLine(AES.Encrypt(GetLocalLANIPAddress(), "key"));
        }
        else if (key.ToLower() == "w")
        {
            Console.WriteLine("Your device address is:");
            Console.WriteLine(AES.Encrypt(GetLocalWirelessIPAddress(), "key"));
        }
        else
        {
            Console.WriteLine("Invalid option. Press any key to continue...");
        }
        Console.ReadLine();
    }
    else if (key.ToLower() == "p")
    {
        Console.Clear();
        HttpClient client = new HttpClient();
        string ip = await client.GetStringAsync("https://api.ipify.org");
        Console.WriteLine("Your device address is:");
        Console.WriteLine(AES.Encrypt(ip, "key"));
        Console.ReadLine();
    }
    else
    {
        Console.WriteLine("Invalid option. Press any key to continue...");
        Console.ReadLine();
    }
}

static void RunBootSequence()
{
    string[] initMessages = {
            "Initializing core modules...",
            "Checking peer-to-peer protocols...",
            "Verifying encryption keys...",
            "Establishing secure sockets...",
            "System integrity: OK"
        };

    foreach (string msg in initMessages)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("[XFILES] ");
        Console.ResetColor();
        Console.WriteLine(msg);
        Thread.Sleep(60);
    }

    char[] spinner = new char[] { '|', '/', '-', '\\' };

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n[XFILES] Scanning for peers...");
    Console.ResetColor();

    Random rand = new Random();
    int peersFound = rand.Next(1, 3);
    for (int i = 0; i < peersFound; i++)
    {
        string ip = $"{rand.Next(1, 255)}.{rand.Next(0, 255)}.{rand.Next(0, 255)}.{rand.Next(1, 255)}";
        for (int s = 0; s < spinner.Length; s++)
        {
            Console.Write($"\r[XFILES] Detecting node at {ip} {spinner[s]}");
            Thread.Sleep(25);
        }
        Console.WriteLine($"\r[XFILES] Detected node at {ip} ... OK");
    }

    Console.WriteLine("\n[XFILES] Loading subsystems...");
    int total = 30; 
    for (int i = 0; i <= total; i++)
    {
        Console.Write("\r[" + new string('#', i) + new string('-', total - i) + $"] {i * 100 / total}%");
        Thread.Sleep(10);
    }

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n[XFILES] cs8602 Dereference of a possibly null reference.");
    Console.ResetColor();

    Console.Clear();
    Thread.Sleep(500);

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("     ########         #######                                                         ");
    Thread.Sleep(10);
    Console.WriteLine("      ########      ########                                                     ");
    Thread.Sleep(10);
    Console.WriteLine("       ########    ########                                                      ");
    Thread.Sleep(10);
    Console.Write("        ########  #######"); Console.ForegroundColor = ConsoleColor.DarkBlue; Console.WriteLine("########## ########  ####        ###########    ########"); Console.ForegroundColor = ConsoleColor.DarkGray;
    Thread.Sleep(10);
    Console.Write("          ##############"); Console.ForegroundColor = ConsoleColor.DarkBlue; Console.WriteLine("########### ########  ####        ###########  ###########"); Console.ForegroundColor = ConsoleColor.DarkGray;
    Thread.Sleep(10);
    Console.Write("           ############ "); Console.ForegroundColor = ConsoleColor.DarkBlue; Console.WriteLine("####          ####    ####        ####        ####      ##"); Console.ForegroundColor = ConsoleColor.DarkGray;
    Thread.Sleep(10);
    Console.Write("            ########## "); Console.ForegroundColor = ConsoleColor.DarkBlue; Console.WriteLine(" ##########    ####    ####        ##########  ########   "); Console.ForegroundColor = ConsoleColor.DarkGray;
    Thread.Sleep(10);
    Console.Write("            ########## "); Console.ForegroundColor = ConsoleColor.DarkBlue; Console.WriteLine(" ##########    ####    ####        ##########    ##########"); Console.ForegroundColor = ConsoleColor.DarkGray;
    Thread.Sleep(10);
    Console.Write("           ############"); Console.ForegroundColor = ConsoleColor.DarkBlue; Console.WriteLine(" ####          ####    ####        ####               #####"); Console.ForegroundColor = ConsoleColor.DarkGray;
    Thread.Sleep(10);
    Console.Write("          ##############"); Console.ForegroundColor = ConsoleColor.DarkBlue; Console.WriteLine("####          ####    ####        ####        ##      ####"); Console.ForegroundColor = ConsoleColor.DarkGray;
    Thread.Sleep(10);
    Console.Write("         #######  #######"); Console.ForegroundColor = ConsoleColor.DarkBlue; Console.WriteLine("###        ########  ########### ########### ########### "); Console.ForegroundColor = ConsoleColor.DarkGray;
    Thread.Sleep(10);
    Console.Write("       ########    ########                                             "); Console.ForegroundColor = ConsoleColor.DarkBlue; Console.WriteLine("  ###    "); Console.ForegroundColor = ConsoleColor.DarkGray;
    Thread.Sleep(10);
    Console.WriteLine("      ########      ###########################Version - 1.00####################");
    Thread.Sleep(10);
    Console.WriteLine("     ########        ########                                                 ");
    Thread.Sleep(1000);
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.White;

    for (int i = 0; i < 20; i++)
    {
        Console.Write($"\r{spinner[i % spinner.Length]}");
        Thread.Sleep(100);
    }
    //cts.Cancel();
}

static string GetLocalLANIPAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    var localIp = host.AddressList
        .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?
        .ToString();
    if (localIp == null)
        throw new InvalidOperationException("No IPv4 address found");
    return localIp;
}
static string GetLocalWirelessIPAddress()
{
    var wifiInterface = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(ni =>
                ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                ni.OperationalStatus == OperationalStatus.Up);

    if (wifiInterface == null)
        throw new InvalidOperationException("No active Wi-Fi interface found");

    var ipProps = wifiInterface.GetIPProperties();
    var wifiIp = ipProps.UnicastAddresses
        .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)?
        .Address.ToString();

    if (wifiIp == null)
        throw new InvalidOperationException("No IPv4 address found on Wi-Fi interface");

    return wifiIp;
}

static string getDownloadsPath()
{
    IntPtr pszPath;
    int hr = SHGetKnownFolderPath(KnownFolder.Downloads, 0, IntPtr.Zero, out pszPath);
    if (hr != 0)
    {
        throw new ExternalException("Failed to get known folder path", hr);
    }
    string? path = Marshal.PtrToStringUni(pszPath);
    if (path == null)
        throw new InvalidOperationException("Failed to convert path pointer to string");
    Marshal.FreeCoTaskMem(pszPath);
    return path;
}


[DllImport("shell32.dll")]
static extern int SHGetKnownFolderPath(
    [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
    uint dwFlags,
    IntPtr hToken,
    out IntPtr pszPath);
public static class KnownFolder
{
    public static readonly Guid Downloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
}

//########         #######                                                         
// ########      ########                                                          
//  ########    ########                                                           
//   ########  ################# ########  ####        ###########    ########     
//     ######################### ########  ####        ###########  ###########    
//      ############ ####          ####    ####        ####        ####      ##    
//       ##########  ##########    ####    ####        ##########  ########        
//       ##########  ##########    ####    ####        ##########    ##########    
//      ############ ####          ####    ####        ####               #####    
//     ##################          ####    ####        ####        ##      ####    
//    #######  ##########        ########  ########### ########### ###########     
//  ########    ########                                               ###         
// ########      #############################################################     
//########        ########                                                         
