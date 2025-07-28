import sys
import re
import os
from pynput import keyboard
from appscript import app, mactypes
from UDP import *
from Voisins import *
from VoisinApp import * 
from TCP import send_file_tcp

class FinderSelector:
    def __init__(self, gui_queue):
        self.selectedFiles = []
        self.gui_queue = gui_queue

        self.hotkeys = {
            '<ctrl>+s': self.on_activate,
            '<ctrl>+q': self.on_quit
        }

    def extract_absolute_path(self, path_string: str):
        pattern = re.compile(r"\.(?:folders|document_files|files|file)\['([^']+)'\]")
        matches = pattern.findall(path_string)
        if matches:
            return os.path.join(os.sep, *matches)
        else:
            return None

    def get_selection(self):
        finder = app('Finder')
        finder.activate()
        selection = finder.selection.get()

        paths = []
        for item in selection:
            path_string = str(item)
            abs_path = self.extract_absolute_path(path_string)
            if abs_path:
                paths.append(abs_path)
        
        return paths

    def clear_selection(self):
        finder = app('Finder')
        finder.selection.set([])

    def on_activate(self):
        print("on activate")
        paths = self.get_selection()
        if paths:
            #print(paths)                                       # PATH PRINT !!!
            Voisins.set_path(paths)

            actualiseVoisin()
            voisins = Voisins.get_voisins()
            self.gui_queue.put(voisins)
        else:
            print("⚠️ Aucun fichier sélectionné.")
        self.clear_selection()

    def on_quit(self):
        print("\n👋 Arrêt du programme.")
        self.gui_queue.put(None)
        sys.exit(0)

    def run(self):
        with keyboard.GlobalHotKeys(self.hotkeys) as listener:
            listener.join()
