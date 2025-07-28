using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // Objet Scan
        ScanUDP scan = new();
        scan.Start();

        using var svc = new ExplorerSelectionService();
        svc.SelectionRequested += (s, e) =>
        {
            // Trouver les voisins
            scan.SearchVoisin();
            // choisir voisin
            Voisin? choisi = VoisinSelectorForm.AfficherEtChoisir(scan.GetVoisins());
            ScanTCP scanTCP = new();

           foreach (var path in e.Paths)
            {
                scan.sendUDP("PPP", choisi.Ip);
                Thread.Sleep(2000);  // pause 0.5 s
                scanTCP.sendTCP(path, choisi.Ip);
            }

        };

        svc.Start();
        Console.WriteLine("Appuyez sur Entrée pour quitter…");
        Console.ReadLine();
        svc.Stop();

    }
}
