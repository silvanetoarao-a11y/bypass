#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Bypass BlueStacks - Interface Gráfica
Aplicação com GUI para aplicar bypass sem root
"""

import tkinter as tk
from tkinter import ttk, messagebox
import subprocess
import threading
import time
import psutil
import os
import sys
from pathlib import Path

class BlueStacksBypassGUI:
    def __init__(self, root):
        self.root = root
        self.root.title("Bypass BlueStacks - Simular Mobile")
        self.root.geometry("600x500")
        self.root.resizable(False, False)
        
        # Cores modernas
        self.bg_color = "#1e1e1e"
        self.accent_color = "#0078d4"
        self.success_color = "#107c10"
        self.warning_color = "#ffaa00"
        self.error_color = "#d13438"
        
        self.root.configure(bg=self.bg_color)
        
        # Variáveis
        self.bluestacks_running = False
        self.bypass_active = False
        self.selected_device = tk.StringVar(value="samsung_galaxy_s21")
        self.adb_connected = False
        self.daemon_process = None
        self.daemon_thread = None
        
        # Perfis de dispositivos
        self.device_profiles = {
            "samsung_galaxy_s21": {
                "name": "Samsung Galaxy S21",
                "model": "SM-G991B",
                "brand": "samsung",
                "manufacturer": "samsung",
                "device": "o1s",
                "fingerprint": "samsung/o1sxxx/o1s:12/SP1A.210812.016/G991BXXU5CVB7:user/release-keys"
            },
            "xiaomi_redmi_note_11": {
                "name": "Xiaomi Redmi Note 11",
                "model": "2201117TG",
                "brand": "Redmi",
                "manufacturer": "Xiaomi",
                "device": "spes",
                "fingerprint": "Redmi/spes_global/spes:12/SP1A.210812.016/V13.0.4.0.SGKMIXM:user/release-keys"
            },
            "oneplus_9": {
                "name": "OnePlus 9",
                "model": "LE2113",
                "brand": "OnePlus",
                "manufacturer": "OnePlus",
                "device": "OnePlus9",
                "fingerprint": "OnePlus/OnePlus9_EEA/OnePlus9:12/SKQ1.210216.001/2206171200:user/release-keys"
            },
            "google_pixel_6": {
                "name": "Google Pixel 6",
                "model": "Pixel 6",
                "brand": "google",
                "manufacturer": "Google",
                "device": "oriole",
                "fingerprint": "google/oriole/oriole:13/TQ1A.230105.002/9325679:user/release-keys"
            }
        }
        
        self.setup_ui()
        self.start_monitoring()
    
    def setup_ui(self):
        # Header
        header_frame = tk.Frame(self.root, bg=self.bg_color, pady=20)
        header_frame.pack(fill=tk.X)
        
        title_label = tk.Label(
            header_frame,
            text="🔓 Bypass BlueStacks",
            font=("Segoe UI", 24, "bold"),
            fg="white",
            bg=self.bg_color
        )
        title_label.pack()
        
        subtitle_label = tk.Label(
            header_frame,
            text="Simule um dispositivo mobile real",
            font=("Segoe UI", 11),
            fg="#cccccc",
            bg=self.bg_color
        )
        subtitle_label.pack()
        
        # Status Frame
        status_frame = tk.Frame(self.root, bg="#2d2d2d", relief=tk.RAISED, bd=1)
        status_frame.pack(fill=tk.X, padx=20, pady=10)
        
        # Status BlueStacks
        self.bluestacks_status = tk.Label(
            status_frame,
            text="🔍 Procurando BlueStacks...",
            font=("Segoe UI", 10),
            fg=self.warning_color,
            bg="#2d2d2d",
            anchor="w"
        )
        self.bluestacks_status.pack(fill=tk.X, padx=15, pady=8)
        
        # Status ADB
        self.adb_status = tk.Label(
            status_frame,
            text="📱 ADB: Não conectado",
            font=("Segoe UI", 10),
            fg="#888888",
            bg="#2d2d2d",
            anchor="w"
        )
        self.adb_status.pack(fill=tk.X, padx=15, pady=5)
        
        # Status Bypass
        self.bypass_status = tk.Label(
            status_frame,
            text="⚙️ Bypass: Inativo",
            font=("Segoe UI", 10),
            fg="#888888",
            bg="#2d2d2d",
            anchor="w"
        )
        self.bypass_status.pack(fill=tk.X, padx=15, pady=5)
        
        # Device Selection Frame
        device_frame = tk.LabelFrame(
            self.root,
            text="Selecionar Dispositivo",
            font=("Segoe UI", 11, "bold"),
            fg="white",
            bg=self.bg_color,
            padx=20,
            pady=15
        )
        device_frame.pack(fill=tk.X, padx=20, pady=10)
        
        device_combo = ttk.Combobox(
            device_frame,
            textvariable=self.selected_device,
            values=list(self.device_profiles.keys()),
            state="readonly",
            font=("Segoe UI", 10),
            width=40
        )
        device_combo.pack(pady=10)
        
        # Mostrar nome do dispositivo selecionado
        self.device_info_label = tk.Label(
            device_frame,
            text="",
            font=("Segoe UI", 9),
            fg="#cccccc",
            bg=self.bg_color
        )
        self.device_info_label.pack()
        device_combo.bind("<<ComboboxSelected>>", self.on_device_change)
        self.on_device_change()
        
        # Buttons Frame
        button_frame = tk.Frame(self.root, bg=self.bg_color)
        button_frame.pack(fill=tk.X, padx=20, pady=20)
        
        # Botão Ativar Bypass
        self.activate_button = tk.Button(
            button_frame,
            text="🚀 Ativar Bypass",
            font=("Segoe UI", 12, "bold"),
            bg=self.accent_color,
            fg="white",
            activebackground="#005a9e",
            activeforeground="white",
            relief=tk.FLAT,
            padx=30,
            pady=15,
            cursor="hand2",
            command=self.activate_bypass,
            state=tk.DISABLED
        )
        self.activate_button.pack(side=tk.LEFT, padx=5)
        
        # Botão Desativar Bypass
        self.deactivate_button = tk.Button(
            button_frame,
            text="⏹️ Desativar Bypass",
            font=("Segoe UI", 12, "bold"),
            bg="#666666",
            fg="white",
            activebackground="#555555",
            activeforeground="white",
            relief=tk.FLAT,
            padx=30,
            pady=15,
            cursor="hand2",
            command=self.deactivate_bypass,
            state=tk.DISABLED
        )
        self.deactivate_button.pack(side=tk.LEFT, padx=5)
        
        # Botão Conectar ADB
        self.connect_button = tk.Button(
            button_frame,
            text="🔌 Conectar ADB",
            font=("Segoe UI", 10),
            bg="#444444",
            fg="white",
            activebackground="#333333",
            activeforeground="white",
            relief=tk.FLAT,
            padx=20,
            pady=10,
            cursor="hand2",
            command=self.connect_adb
        )
        self.connect_button.pack(side=tk.RIGHT, padx=5)
        
        # Log Frame
        log_frame = tk.LabelFrame(
            self.root,
            text="Log",
            font=("Segoe UI", 10, "bold"),
            fg="white",
            bg=self.bg_color,
            padx=20,
            pady=10
        )
        log_frame.pack(fill=tk.BOTH, expand=True, padx=20, pady=10)
        
        self.log_text = tk.Text(
            log_frame,
            height=8,
            font=("Consolas", 9),
            bg="#1a1a1a",
            fg="#00ff00",
            wrap=tk.WORD,
            relief=tk.FLAT
        )
        self.log_text.pack(fill=tk.BOTH, expand=True)
        
        scrollbar = tk.Scrollbar(self.log_text)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.log_text.config(yscrollcommand=scrollbar.set)
        scrollbar.config(command=self.log_text.yview)
        
        self.log("Sistema iniciado. Aguardando BlueStacks...")
    
    def on_device_change(self, event=None):
        device_key = self.selected_device.get()
        if device_key in self.device_profiles:
            device = self.device_profiles[device_key]
            info_text = f"📱 {device['name']} | Modelo: {device['model']}"
            self.device_info_label.config(text=info_text)
    
    def log(self, message):
        timestamp = time.strftime("%H:%M:%S")
        self.log_text.insert(tk.END, f"[{timestamp}] {message}\n")
        self.log_text.see(tk.END)
        self.root.update_idletasks()
    
    def check_bluestacks(self):
        """Verifica se BlueStacks está rodando"""
        bluestacks_processes = [
            "HD-Player.exe",
            "BlueStacks.exe",
            "BlueStacksX.exe",
            "BstkSVC.exe"
        ]
        
        for proc in psutil.process_iter(['name']):
            try:
                if proc.info['name'] in bluestacks_processes:
                    return True
            except (psutil.NoSuchProcess, psutil.AccessDenied):
                continue
        return False
    
    def check_adb_connection(self):
        """Verifica conexão ADB"""
        try:
            result = subprocess.run(
                ["adb", "devices"],
                capture_output=True,
                text=True,
                timeout=5
            )
            if "127.0.0.1:5555" in result.stdout or "emulator" in result.stdout:
                return True
        except:
            pass
        return False
    
    def connect_adb(self):
        """Conecta ao BlueStacks via ADB"""
        self.log("Tentando conectar ao BlueStacks via ADB...")
        
        try:
            # Tentar conectar na porta padrão
            result = subprocess.run(
                ["adb", "connect", "127.0.0.1:5555"],
                capture_output=True,
                text=True,
                timeout=10
            )
            
            if "connected" in result.stdout.lower() or "already connected" in result.stdout.lower():
                self.adb_connected = True
                self.adb_status.config(text="📱 ADB: Conectado", fg=self.success_color)
                self.log("✅ ADB conectado com sucesso!")
                self.update_button_states()
                return True
            else:
                self.log("⚠️ Não foi possível conectar. Verifique se a depuração USB está habilitada.")
                messagebox.showwarning(
                    "ADB não conectado",
                    "Não foi possível conectar ao BlueStacks.\n\n"
                    "Certifique-se de:\n"
                    "1. BlueStacks está aberto\n"
                    "2. Depuração USB está habilitada\n"
                    "3. ADB está instalado no sistema"
                )
        except FileNotFoundError:
            self.log("❌ ADB não encontrado! Instale o Android SDK Platform Tools.")
            messagebox.showerror(
                "ADB não encontrado",
                "ADB não está instalado ou não está no PATH.\n\n"
                "Baixe o Android SDK Platform Tools:\n"
                "https://developer.android.com/studio/releases/platform-tools"
            )
        except Exception as e:
            self.log(f"❌ Erro ao conectar: {str(e)}")
        
        return False
    
    def activate_bypass(self):
        """Ativa o bypass"""
        if not self.adb_connected:
            if not self.connect_adb():
                return
        
        device_key = self.selected_device.get()
        device = self.device_profiles[device_key]
        
        self.log(f"🚀 Ativando bypass com perfil: {device['name']}")
        
        # Executar em thread separada para não travar a UI
        thread = threading.Thread(target=self._apply_bypass, args=(device,))
        thread.daemon = True
        thread.start()
    
    def _start_daemon(self, device_key):
        """Inicia daemon para manter propriedades ativas"""
        def daemon_loop():
            device = self.device_profiles[device_key]
            while self.bypass_active:
                try:
                    if self.check_adb_connection():
                        self._apply_properties_once(device)
                    time.sleep(5)  # Reaplica a cada 5 segundos
                except:
                    time.sleep(5)
        
        if self.daemon_thread is None or not self.daemon_thread.is_alive():
            self.daemon_thread = threading.Thread(target=daemon_loop)
            self.daemon_thread.daemon = True
            self.daemon_thread.start()
            self.log("🔄 Daemon iniciado para manter bypass ativo")
    
    def _apply_properties_once(self, device):
        """Aplica propriedades uma vez"""
        commands = [
            ["adb", "shell", "setprop", "ro.kernel.qemu", "0"],
            ["adb", "shell", "setprop", "ro.hardware", "qcom"],
            ["adb", "shell", "setprop", "ro.product.model", device["model"]],
            ["adb", "shell", "setprop", "ro.product.brand", device["brand"]],
            ["adb", "shell", "setprop", "ro.product.manufacturer", device["manufacturer"]],
            ["adb", "shell", "setprop", "ro.product.device", device["device"]],
            ["adb", "shell", "setprop", "ro.build.fingerprint", device["fingerprint"]],
        ]
        
        for cmd in commands:
            try:
                result = subprocess.run(cmd, capture_output=True, text=True, timeout=2)
                if result.returncode == 0:
                    self.log(f"✅ {cmd[2]} = {cmd[3]}")
            except Exception as e:
                self.log(f"⚠️ Erro ao definir {cmd[2]}: {str(e)}")
    
    def _apply_bypass(self, device):
        """Aplica o bypass via ADB (sem root)"""
        try:
            device_key = self.selected_device.get()
            
            # Aplicar propriedades inicialmente
            self.log("🔧 Aplicando propriedades do dispositivo...")
            self._apply_properties_once(device)
            
            # Verificar propriedades
            self._verify_bypass(device)
            
            # Iniciar daemon para manter propriedades ativas
            self._start_daemon(device_key)
            
            self.bypass_active = True
            self.root.after(0, self._update_bypass_status, True)
            self.root.after(0, self.update_button_states)
            
        except Exception as e:
            self.log(f"❌ Erro ao aplicar bypass: {str(e)}")
            self.root.after(0, lambda: messagebox.showerror("Erro", f"Falha ao aplicar bypass:\n{str(e)}"))
    
    def _verify_bypass(self, device):
        """Verifica se o bypass foi aplicado"""
        self.log("🔍 Verificando propriedades...")
        
        try:
            result = subprocess.run(
                ["adb", "shell", "getprop", "ro.product.model"],
                capture_output=True,
                text=True,
                timeout=5
            )
            
            if device["model"] in result.stdout:
                self.log(f"✅ Modelo verificado: {result.stdout.strip()}")
            else:
                self.log(f"⚠️ Modelo não corresponde: {result.stdout.strip()}")
        except:
            pass
    
    def deactivate_bypass(self):
        """Desativa o bypass"""
        self.log("⏹️ Desativando bypass...")
        
        thread = threading.Thread(target=self._remove_bypass)
        thread.daemon = True
        thread.start()
    
    def _remove_bypass(self):
        """Remove o bypass"""
        try:
            # Parar daemon
            self.bypass_active = False
            
            # Resetar propriedades (vai voltar ao padrão do BlueStacks)
            commands = [
                ["adb", "shell", "setprop", "ro.kernel.qemu", ""],
            ]
            
            for cmd in commands:
                subprocess.run(cmd, capture_output=True, timeout=5)
            
            self.log("✅ Bypass desativado")
            self.log("🛑 Daemon parado")
            
            self.root.after(0, self._update_bypass_status, False)
            self.root.after(0, self.update_button_states)
            
        except Exception as e:
            self.log(f"❌ Erro ao desativar: {str(e)}")
    
    def _update_bypass_status(self, active):
        """Atualiza status do bypass na UI"""
        if active:
            self.bypass_status.config(
                text="⚙️ Bypass: ATIVO",
                fg=self.success_color
            )
        else:
            self.bypass_status.config(
                text="⚙️ Bypass: Inativo",
                fg="#888888"
            )
    
    def update_button_states(self):
        """Atualiza estados dos botões"""
        if self.bluestacks_running and self.adb_connected:
            if self.bypass_active:
                self.activate_button.config(state=tk.DISABLED)
                self.deactivate_button.config(state=tk.NORMAL)
            else:
                self.activate_button.config(state=tk.NORMAL)
                self.deactivate_button.config(state=tk.DISABLED)
        else:
            self.activate_button.config(state=tk.DISABLED)
            self.deactivate_button.config(state=tk.DISABLED)
    
    def start_monitoring(self):
        """Inicia monitoramento de BlueStacks e ADB"""
        def monitor():
            while True:
                # Verificar BlueStacks
                running = self.check_bluestacks()
                if running != self.bluestacks_running:
                    self.bluestacks_running = running
                    if running:
                        self.root.after(0, lambda: self.bluestacks_status.config(
                            text="✅ BlueStacks: Detectado",
                            fg=self.success_color
                        ))
                        self.root.after(0, lambda: self.log("✅ BlueStacks detectado!"))
                    else:
                        self.root.after(0, lambda: self.bluestacks_status.config(
                            text="🔍 Procurando BlueStacks...",
                            fg=self.warning_color
                        ))
                    self.root.after(0, self.update_button_states)
                
                # Verificar ADB
                if self.bluestacks_running:
                    adb_connected = self.check_adb_connection()
                    if adb_connected != self.adb_connected:
                        self.adb_connected = adb_connected
                        if adb_connected:
                            self.root.after(0, lambda: self.adb_status.config(
                                text="📱 ADB: Conectado",
                                fg=self.success_color
                            ))
                        else:
                            self.root.after(0, lambda: self.adb_status.config(
                                text="📱 ADB: Não conectado",
                                fg="#888888"
                            ))
                        self.root.after(0, self.update_button_states)
                
                time.sleep(2)
        
        thread = threading.Thread(target=monitor)
        thread.daemon = True
        thread.start()

def main():
    # Verificar se está no Windows
    if sys.platform != "win32":
        print("Este aplicativo é destinado ao Windows.")
        return
    
    root = tk.Tk()
    app = BlueStacksBypassGUI(root)
    root.mainloop()

if __name__ == "__main__":
    main()
