# 🔍📤 UDP Scanner & TCP File Sender

Ce projet permet de **scanner les ordinateurs présents sur un réseau local via UDP**, puis **d'envoyer des fichiers entre machines via TCP**. Il est compatible **Windows (.NET)** et **macOS (Python)**.

---

## 🧩 Fonctionnalités

- 📡 Détection des machines sur le réseau local via UDP
- 📁 Transfert de fichiers en TCP avec en-tête contenant le nom et la taille
- 🔗 Compatibilité multiplateforme : .NET (Windows) ↔ Python (macOS)
- 🖥️ Interface simple : sélection du fichier et de la machine cible

---

## ⚙️ Technologies utilisées

- **Windows (.NET)** : C#, UdpClient, TcpClient
- **macOS (Python)** : `socket`, `appscript`, `pynput`, `tkinter`

---

## 🖼️ Architecture réseau

[Machine A - Windows/.NET] <-- UDP --> [Machine B - Mac/Python]
│ │
└------------ TCP (fichier) -------->┘

---

## 🚀 Démarrage rapide

### 🪟 Windows

1. Ouvrir la solution dans **Visual Studio**
2. Lancer le projet (`Program.cs`)
3. La machine écoute sur :
   - UDP : `port 10123` (découverte réseau)
   - TCP : `port 10124` (réception de fichiers)

### 🍎 macOS

1. Installer les dépendances Python (si non installées) :
   ```bash
   pip install appscript pynput
   python Main.py

## 🔒 Sécurité

Ce projet est conçu pour des réseaux locaux uniquement.
Aucune authentification n’est encore mise en place (peut être ajoutée en future version). C'est conçu pour une utilisation personelle et il y a des failles de sécurité non-patché connu.


🛠️ À faire

 - Support multi-fichier(present pour windows mais pas MacOS)
 - Historique des transferts
 - Optimisation 

