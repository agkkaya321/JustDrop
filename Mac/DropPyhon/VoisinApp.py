import tkinter as tk
from Voisins import *
from UDP import *
from TCP import send_file_tcp

class VoisinsApp:
    def __init__(self, voisins, gui_queue):
        self.voisins = voisins
        self.gui_queue = gui_queue
        self.selected = None
        self.selected_file = None

        self.root = tk.Tk()
        self.root.title("Liste des voisins")
        self.root.protocol("WM_DELETE_WINDOW", self.hide_window)

        self.listbox = tk.Listbox(self.root, width=40, height=10)
        self.listbox.pack(padx=10, pady=10)
        self.remplir_liste()

        self.listbox.bind("<<ListboxSelect>>", self.on_select)

        tk.Button(self.root, text="🔄 Actualiser", command=self.actualise_voisins).pack(pady=5)
        tk.Button(self.root, text="Sélectionner", command=self.make_selection).pack(pady=5)

        # Appelle la fonction périodique pour vérifier les nouveaux voisins
        self.check_queue()
    
    def set_path(self, str):
        self.selected_file = str

    def remplir_liste(self):
        self.listbox.delete(0, tk.END)
        for voisin in self.voisins:
            self.listbox.insert(tk.END, voisin.name)

    def actualise_voisins(self):
        actualiseVoisin()
        self.voisins = Voisins.get_voisins()
        self.remplir_liste()

    def on_select(self, event):
        selection = event.widget.curselection()
        if selection:
            selected = self.listbox.get(selection)
            for v in Voisins.get_voisins():
                if v.name == selected:
                    self.selected = v
        
        print(self.selected)
        
        

    def make_selection(self):
        if self.selected:
            Voisins._selected = self.selected
            self.hide_window()

            if Voisins._selected != None:
                print("[SELECTED VOISIN]")
                print(Voisins._selected.ip)

                print("[SELECTED PATH]")
                print(Voisins._selected_path)

                #print("sleep")
                #time.sleep(10)

                for p in Voisins._selected_path:
                    sendUDP("PPP", Voisins._selected.ip, 10123)
                    time.sleep(3)
                    send_file_tcp(p, Voisins._selected.ip, 10124)
                else:
                    print("⚠️ Aucun voisin sélectionné.")

    def hide_window(self):
        self.root.withdraw()

    def check_queue(self):
        try:
            voisins = self.gui_queue.get_nowait()
            if voisins is None:
                self.quit()
            else:
                self.actualiser_et_afficher(voisins)
        except queue.Empty:
            pass
        finally:
            self.root.after(100, self.check_queue)  # vérifie toutes les 100 ms

    def actualiser_et_afficher(self, voisins):
        self.voisins = voisins
        self.remplir_liste()
        self.root.deiconify()  # Affiche la fenêtre

    def run(self):
        self.root.mainloop()

    def quit(self):
        self.root.destroy()
