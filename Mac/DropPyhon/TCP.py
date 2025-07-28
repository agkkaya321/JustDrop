import socket
import os
import struct
import sys

# Configuration
PORT = 10124
HEADER_SIZE = 1024               # taille fixe de l'en‑tête en octets
MAX_FILESIZE = 10 * 1024**3      # 10 Go
RECEIVE_FOLDER = os.path.expanduser("~/Desktop/ReceivedFiles")

def read_exact(sock: socket.socket, num_bytes: int) -> bytes:
    """Lit exactement num_bytes octets depuis le socket (ou lève EOFError)."""
    buf = b''
    while len(buf) < num_bytes:
        chunk = sock.recv(num_bytes - len(buf))
        if not chunk:
            raise EOFError("Connexion interrompue avant réception complète.")
        buf += chunk
    return buf

def listen_tcp():
    # 1. Prépare le dossier de réception
    os.makedirs(RECEIVE_FOLDER, exist_ok=True)
    print(f"En attente de connexion sur le port {PORT}...")

    # 2. Création et démarrage du listener
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as listener:
        listener.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        listener.bind(('0.0.0.0', PORT))
        listener.listen(1)

        try:
            conn, addr = listener.accept()
            with conn:
                print(f"Connecté par {addr}")

                # 3. Lecture de l'en‑tête fixe
                header_buf = read_exact(conn, HEADER_SIZE)
                # On supprime les \0 de fin
                header_str = header_buf.rstrip(b'\x00').decode('utf-8', errors='strict')
                parts = header_str.split('|', maxsplit=1)
                if len(parts) != 2:
                    raise ValueError("En‑tête invalide, pas de séparateur '|' trouvé.")

                file_name, size_str = parts
                file_name = file_name.strip()
                if not file_name:
                    raise ValueError("Nom de fichier vide dans l'en‑tête.")

                # Vérification de la taille
                try:
                    file_size = int(size_str)
                except ValueError:
                    raise ValueError(f"Taille de fichier invalide ({size_str}).")
                if file_size < 0 or file_size > MAX_FILESIZE:
                    raise ValueError(f"Taille de fichier hors limites ({file_size}).")

                # 4. Prépare le chemin de destination (et évite les chemins relatifs)
                safe_name = os.path.basename(file_name)
                dest_path = os.path.join(RECEIVE_FOLDER, safe_name)
                print(f"Réception de «{safe_name}» ({file_size} octets)...")

                # 5. Lecture et écriture du payload
                with open(dest_path, 'wb') as f:
                    remaining = file_size
                    buf_size = 8192
                    while remaining > 0:
                        chunk = conn.recv(min(buf_size, remaining))
                        if not chunk:
                            raise EOFError("Connexion interrompue pendant le transfert.")
                        f.write(chunk)
                        remaining -= len(chunk)

                    f.flush()
                    os.fsync(f.fileno())

                print(f"Fichier reçu et enregistré dans : {dest_path}")

        except Exception as e:
            print(f"Erreur lors de la réception : {e}", file=sys.stderr)

def send_file_tcp(file_path, ip, port):
    """Envoie un fichier via TCP à l'adresse IP et au port spécifiés."""
    if not os.path.isfile(file_path):
        print(f"[Erreur] Le fichier '{file_path}' n'existe pas.")
        return

    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            print(f"[Connexion] Tentative de connexion à {ip}:{port}...")
            s.connect((ip, port))
            print(f"[Connexion] Connexion établie.")

            # Envoyer le nom et la taille du fichier
            filename = os.path.basename(file_path)
            filesize = os.path.getsize(file_path)
            header = f"{filename}|{filesize}".encode().ljust(1024, b'\0')
            s.sendall(header)

            # Envoyer le contenu du fichier
            with open(file_path, "rb") as f:
                while chunk := f.read(4096):
                    s.sendall(chunk)
            print(f"[Envoi terminé] Fichier '{filename}' envoyé avec succès.")

    except Exception as e:
        print(f"[Erreur] Échec de l'envoi : {e}")
