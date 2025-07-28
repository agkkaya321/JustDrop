using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ScanTCP
{
    private const int PORT = 10124;
    private const string RECEIVE_FOLDER = "./recivedFile";
    private const int HEADER_SIZE = 1024;
    private const long MAX_FILESIZE = 10L * 1024 * 1024 * 1024; // 10 GiB cap

      public void sendTCP(string filePath, string ipAddress)
    {
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"[Erreur] Le fichier '{filePath}' n'existe pas.");
            return;
        }

        try
        {
            // 1. Préparation du nom et de la taille
            string fileName = Path.GetFileName(filePath);
            long fileSize = new FileInfo(filePath).Length;

            if (fileSize < 0 || fileSize > MAX_FILESIZE)
            {
                Console.Error.WriteLine($"[Erreur] Taille de fichier invalide : {fileSize} octets.");
                return;
            }

            // 2. Construction du header fixe de HEADER_SIZE octets
            string headerStr = $"{fileName}|{fileSize}";
            byte[] headerBuf = Encoding.UTF8.GetBytes(headerStr);
            if (headerBuf.Length > HEADER_SIZE)
                throw new InvalidDataException("Le header est trop volumineux.");
            Array.Resize(ref headerBuf, HEADER_SIZE); // pad avec des 0

            // 3. Connexion au serveur
            using (var client = new TcpClient())
            {
                Console.WriteLine($"[Connexion] Tentative de connexion à {ipAddress}:{PORT}...");
                client.Connect(ipAddress, PORT);
                Console.WriteLine("[Connexion] Établie.");

                using (NetworkStream netStream = client.GetStream())
                {
                    // 4. Envoi du header
                    netStream.Write(headerBuf, 0, HEADER_SIZE);

                    // 5. Envoi du contenu du fichier par morceaux
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[8192];
                        int read;
                        long totalSent = 0;
                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            netStream.Write(buffer, 0, read);
                            totalSent += read;
                        }
                    }

                    netStream.Flush();
                    Console.WriteLine($"[Envoi terminé] «{fileName}» ({fileSize} octets) envoyé avec succès.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Erreur] Échec de l'envoi : {ex.Message}");
        }
    }
    public void ListenTCP()
    {
        // 1. Ensure receive folder exists
        Directory.CreateDirectory(RECEIVE_FOLDER);

        var listener = new TcpListener(IPAddress.Any, PORT);
        listener.Start();

        try
        {
            using (var client = listener.AcceptTcpClient())
            using (var netStream = client.GetStream())
            {
                // 2. Read fixed‐size header
                byte[] headerBuf = ReadExact(netStream, HEADER_SIZE);
                string header = Encoding.UTF8.GetString(headerBuf)
                                           .TrimEnd('\0');
                var parts = header.Split('|');
                if (parts.Length != 2)
                    throw new InvalidDataException("Header invalide.");

                string fileName = parts[0];
                if (string.IsNullOrWhiteSpace(fileName))
                    throw new InvalidDataException("Nom de fichier vide.");

                if (!long.TryParse(parts[1], out long fileSize)
                    || fileSize < 0
                    || fileSize > MAX_FILESIZE)
                {
                    throw new InvalidDataException($"Taille de fichier invalide ({parts[1]}).");
                }

                // 3. Prepare destination path
                string destPath = Path.Combine(RECEIVE_FOLDER, fileName);

                // 4. Read file payload
                using (var fs = new FileStream(destPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[8192];
                    long totalRead = 0;
                    while (totalRead < fileSize)
                    {
                        int toRead = (int)Math.Min(buffer.Length, fileSize - totalRead);
                        int read = netStream.Read(buffer, 0, toRead);
                        if (read == 0)
                            throw new EndOfStreamException("Connexion interrompue.");
                        fs.Write(buffer, 0, read);
                        totalRead += read;
                    }
                    fs.Flush(true);
                }

                Console.WriteLine($"Fichier «{fileName}» ({fileSize} octets) reçu dans {RECEIVE_FOLDER}.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Erreur lors de la réception : {ex.Message}");
        }
        finally
        {
            listener.Stop();
        }
    }

    private static byte[] ReadExact(NetworkStream stream, int count)
    {
        byte[] buf = new byte[count];
        int offset = 0;
        while (offset < count)
        {
            int n = stream.Read(buf, offset, count - offset);
            if (n == 0)
                throw new EndOfStreamException($"Impossible de lire {count} octets (lu {offset}).");
            offset += n;
        }
        return buf;
    }
}
