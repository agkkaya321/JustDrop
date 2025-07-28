import threading
import queue
from FinderSelector import FinderSelector
from UDP import listenUDP
from VoisinApp import VoisinsApp
from Voisins import *

gui_queue = queue.Queue()

def start_udp():
    listenUDP(10123)

def start_finder():
    fS = FinderSelector(gui_queue)
    fS.run()

def gui_handler():
    app = None
    while True:
        voisins = gui_queue.get()
        if voisins is None:
            if app:
                app.quit()
            break
        if not app:
            # Lance une seule fois l'interface Tkinter
            print(voisins)
            app = VoisinsApp(voisins, gui_queue)
            app.run()
        else:
            # Actualise les voisins et réactive l'interface
            app.actualiser_et_afficher(voisins)
            app.root.deiconify()

if __name__ == "__main__":
    udp_thread = threading.Thread(target=start_udp, daemon=True)
    udp_thread.start()

    finder_thread = threading.Thread(target=start_finder, daemon=True)
    finder_thread.start()

    gui_handler()
