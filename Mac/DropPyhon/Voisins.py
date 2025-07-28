import tkinter as tk
from tkinter import messagebox
import queue
from UDP import *

class Voisin:
    def __init__(self, ip: str, name: str):
        self.ip = ip
        self.name = name

    def __repr__(self):
        return f"Voisin(ip='{self.ip}', name='{self.name}')"


class Voisins:
    _voisins = []
    _selected = None
    _selected_path = None

    @staticmethod
    def add_voisin(ip: str, name: str):
        voisin = Voisin(ip, name)
        Voisins._voisins.append(voisin)

    @staticmethod
    def clear_voisins():
        Voisins._voisins.clear()

    @staticmethod
    def get_voisins():
        return Voisins._voisins
    
    @staticmethod
    def set_path(path):
        Voisins._selected_path = path
        print(Voisins.get_path())
    
    @staticmethod
    def get_path():
        return Voisins._selected_path 

