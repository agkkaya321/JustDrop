import socket
import time
from typing import List
from Voisins import Voisins
from Voisins import *
from utils import *
from TCP import listen_tcp

PORT = 10123  

def listenUDP(port, buffer_size=4096):
    # Création du socket UDP
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    # Liaison sur toutes les interfaces à l'écoute du port
    sock.bind(('', port))
    #print(f"[Serveur] En écoute sur le port UDP {port}...")

    try:
        while True:
            # Bloquant : attend un message
            data, addr = sock.recvfrom(buffer_size)
            message = data.decode('utf-8', errors='replace')

            print(f"[Reçu] {len(data)} octets de {addr} : {message}")


            # Conditions addr / message
            
            n, mn = check_n(message)

            if n:
                ip = str(addr[0])
                if not any(v.ip == ip for v in Voisins._voisins):
                    Voisins.add_voisin(ip, mn)
            elif message == 'PPP':
                listen_tcp()
            elif message == 'tesqui?':
                sendUDP("NNNMacDeXXX", addr[0], 10123)


    except KeyboardInterrupt:
        print("\n[Serveur] Arrêt de l'écoute (interrompu par l'utilisateur).")
    except OSError as e:
        print(f"[Erreur OS] {e}")
    finally:
        sock.close()
        print("[Serveur] Socket UDP fermé.")

def sendUDP(message, ip, port=10123):
    client = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    # Désactiver tout blocage (envoi et tentative de réception non–bloquants)
    client.setblocking(False)
    try:
        client.sendto(message.encode(), (ip, port))
    except OSError as e:
        print(f"[Erreur OS] {e}")
    finally:
        client.close()
        print("[Client] Socket UDP fermé.")

def ip_subnet(ip: str) -> str:
    parts = ip.split('.')
    if len(parts) == 4:
        return f"{parts[0]}.{parts[1]}.{parts[2]}."
    return ""

def get_ip() -> List[str]:
    ips = []
    hostname = socket.gethostname()
    try:
        host_ips = socket.gethostbyname_ex(hostname)[2]
        for ip in host_ips:
            if '.' in ip:
                ips.append(ip)
    except socket.gaierror:
        print("[Erreur] Impossible de résoudre le nom d'hôte.")
    return ips

def ping_name(self_ip: str):
    message = "tesqui?"
    subnet = ip_subnet(self_ip)
    data = message.encode()

    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as udp:
        udp.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)

        for i in range(1, 255):
            target_ip = f"{subnet}{i}"
            if target_ip == self_ip:
                continue
            try:
                udp.sendto(data, (target_ip, PORT))
                time.sleep(0.002)  # Pause de 2ms
            except Exception as e:
                print(f"[Erreur] Envoi vers {target_ip} échoué : {e}")

def actualiseVoisin():
    Voisins.clear_voisins()
    ips = get_ip()
    for ip in ips:
        ipSubNet = ip_subnet(ip)
        ping_name(ipSubNet)