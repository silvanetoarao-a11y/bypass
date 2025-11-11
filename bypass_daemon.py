#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Daemon para manter propriedades modificadas no BlueStacks
Mantém o bypass ativo mesmo após reiniciar aplicativos
"""

import subprocess
import time
import sys

class BypassDaemon:
    def __init__(self, device_profile):
        self.device_profile = device_profile
        self.running = True
        
        self.device_profiles = {
            "samsung_galaxy_s21": {
                "model": "SM-G991B",
                "brand": "samsung",
                "manufacturer": "samsung",
                "device": "o1s",
                "fingerprint": "samsung/o1sxxx/o1s:12/SP1A.210812.016/G991BXXU5CVB7:user/release-keys"
            },
            "xiaomi_redmi_note_11": {
                "model": "2201117TG",
                "brand": "Redmi",
                "manufacturer": "Xiaomi",
                "device": "spes",
                "fingerprint": "Redmi/spes_global/spes:12/SP1A.210812.016/V13.0.4.0.SGKMIXM:user/release-keys"
            },
            "oneplus_9": {
                "model": "LE2113",
                "brand": "OnePlus",
                "manufacturer": "OnePlus",
                "device": "OnePlus9",
                "fingerprint": "OnePlus/OnePlus9_EEA/OnePlus9:12/SKQ1.210216.001/2206171200:user/release-keys"
            },
            "google_pixel_6": {
                "model": "Pixel 6",
                "brand": "google",
                "manufacturer": "Google",
                "device": "oriole",
                "fingerprint": "google/oriole/oriole:13/TQ1A.230105.002/9325679:user/release-keys"
            }
        }
    
    def apply_properties(self):
        """Aplica propriedades via ADB"""
        if self.device_profile not in self.device_profiles:
            return False
        
        device = self.device_profiles[self.device_profile]
        
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
                subprocess.run(cmd, capture_output=True, timeout=2)
            except:
                pass
        
        return True
    
    def check_adb(self):
        """Verifica se ADB está conectado"""
        try:
            result = subprocess.run(
                ["adb", "devices"],
                capture_output=True,
                text=True,
                timeout=3
            )
            return "127.0.0.1:5555" in result.stdout or "device" in result.stdout
        except:
            return False
    
    def run(self):
        """Executa o daemon"""
        print(f"🔄 Daemon iniciado com perfil: {self.device_profile}")
        print("Pressione Ctrl+C para parar")
        
        while self.running:
            try:
                if self.check_adb():
                    self.apply_properties()
                time.sleep(5)  # Reaplica a cada 5 segundos
            except KeyboardInterrupt:
                print("\n⏹️ Parando daemon...")
                self.running = False
                break
            except Exception as e:
                print(f"Erro: {e}")
                time.sleep(5)

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Uso: python bypass_daemon.py [perfil]")
        sys.exit(1)
    
    daemon = BypassDaemon(sys.argv[1])
    daemon.run()
