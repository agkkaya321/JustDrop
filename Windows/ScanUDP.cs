using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.Swift;
using System.Text;
using System.Threading;

public class ScanUDP
{
    readonly int PORT = 10123;

    public static string NAME = "WindowsDeAli";

    public static List<Voisin> VOISINS = new List<Voisin>();


    private string IpSubnet(string ip)
    {
        string ipString = ip;
        string[] parts = ipString.Split('.');

        if (parts.Length == 4)
        {
            string partialIP = $"{parts[0]}.{parts[1]}.{parts[2]}.";
            return partialIP;
        }
        else return " ";
    }

    public List<string> GetIP()
    {
        List<string> ips = new List<string>();

        string hostName = Dns.GetHostName();
        IPHostEntry hostEntry = Dns.GetHostEntry(hostName);

        foreach (IPAddress ip in hostEntry.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                string ipString = ip.ToString();
                ips.Add(ipString);
            }
        }

        return ips;
    }

    public void PingName(string selfIP)
    {
        string message = "tesqui?";
        string ipSubnet = IpSubnet(selfIP);
        byte[] data = Encoding.UTF8.GetBytes(message);

        using UdpClient udpClient = new UdpClient();
        for (int i = 1; i < 255; i++)
        {
            string ip = ipSubnet + i;
            try
            {
                if (ip != selfIP)
                {
                    IPEndPoint endPoint = new(IPAddress.Parse(ip), PORT);
                    udpClient.Send(data, data.Length, endPoint);
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur en envoyant vers {ip} : {ex.Message}");
            }
        }
    }

    public void Start()
    {
        Thread listenerThread = new(ListenUDP) { IsBackground = false };
        listenerThread.Start();
        Console.WriteLine("[Listener] Démarré sur un autre thread.");
    }

    public void SearchVoisin()
    {
        // delete all Voisin
        VOISINS.Clear();
        // get self ips
        List<string> ips = GetIP();

        foreach (string ip in ips)
        {
            PingName(ip);
        }
    }

    private void ListenUDP()
    {
        using UdpClient udpClient = new UdpClient(PORT);
        Console.WriteLine($"[ListenUDP] En écoute sur le port {PORT}");

        while (true)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, PORT);
            byte[] buffer = udpClient.Receive(ref remoteEP);
            string receivedMessage = Encoding.UTF8.GetString(buffer);
            Console.WriteLine($"[ListenUDP] Message reçu de {remoteEP}: '{receivedMessage}'");

            if (CheckN(receivedMessage, out string namePart))
            {
                // Vérification des doublons avant ajout
                if (!VOISINS.Any(v => v.Ip == remoteEP.Address.ToString()))
                {
                    Voisin v = new(remoteEP.Address.ToString(), namePart);
                    VOISINS.Add(v);
                    Console.WriteLine($"[ListenUDP] Nouveau voisin ajouté : {namePart} ({remoteEP.Address})");
                }
                else
                {
                    Console.WriteLine($"[ListenUDP] Voisin déjà enregistré : {remoteEP.Address}");
                }
            }
            else if (receivedMessage == "tesqui?")
            {
                // Réponse : "NNN" + NAME
                string response = "NNN" + NAME;
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);

                IPEndPoint responseEndPoint = new(remoteEP.Address, PORT);
                udpClient.Send(responseBytes, responseBytes.Length, responseEndPoint);

                Console.WriteLine($"[ListenUDP] Réponse envoyée à {responseEndPoint} : '{response}'");
            }
            else if (CheckP(receivedMessage, out string fileName))
            {
                Console.WriteLine("[ListenUDP] Reçu demande de permission PPPxxxxx");
                ScanTCP scanTCP = new();
                scanTCP.ListenTCP();

            }
            else
            {
                Console.WriteLine($"[ListenUDP] Message inconnu reçu : '{receivedMessage}'");
            }
        }
    }

    private bool CheckN(string? input, out string value)
    {
        const string Prefix = "NNN";

        if (!string.IsNullOrEmpty(input) &&
            input.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            value = input[Prefix.Length..];   // sans le préfixe
            return true;
        }

        value = input ?? string.Empty;        // chaîne d'origine
        return false;
    }

    private bool CheckP(string? input, out string value)
    {
        if (input == "PPP")
        {
            value = "PPP";
            return true;
        }
        else
        {
            value = string.Empty;
            return false;
        }
    }

    public void ListVoisin()
    {
        Console.WriteLine("Liste des voisins :");

        if (VOISINS.Count == 0)
        {
            Console.WriteLine("(Aucun voisin disponible)");
            return;
        }

        foreach (var voisin in VOISINS)
        {
            Console.WriteLine($"- IP : {voisin.Ip}, Name : {voisin.Name}");
        }
    }

    public void sendUDP(string message, string ipString)
    {
        try
        {
            // 1. Conversion du string en IPAddress (si hostname, résolution DNS)
            IPAddress ip;
            if (!IPAddress.TryParse(ipString, out ip))
            {
                Console.WriteLine($"[Info] '{ipString}' n'est pas une IP valide, tentative de résolution DNS…");
                var addresses = Dns.GetHostAddresses(ipString);
                if (addresses == null || addresses.Length == 0)
                {
                    Console.Error.WriteLine($"[Erreur] Impossible de résoudre '{ipString}'.");
                    return;
                }
                ip = addresses[0];
                Console.WriteLine($"[Info] Host '{ipString}' résolu en {ip}.");
            }

            // 2. Encodage du message
            byte[] data = Encoding.UTF8.GetBytes(message);

            // 3. Envoi via UdpClient
            using (var udp = new UdpClient())
            {
                var endpoint = new IPEndPoint(ip, PORT);
                Console.WriteLine($"[UDP] Envoi de '{message}' à {endpoint}…");
                udp.Send(data, data.Length, endpoint);
                Console.WriteLine("[UDP] Message envoyé avec succès.");
            }
        }
        catch (SocketException sockEx)
        {
            Console.Error.WriteLine($"[Erreur UDP] Problème réseau : {sockEx.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Erreur UDP] Échec de l'envoi : {ex.Message}");
        }
    }
    public List<Voisin> GetVoisins() {
    return VOISINS;
    }
}

   