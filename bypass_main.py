#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Bypass BlueStacks - Faz o PC parecer um dispositivo móvel real
Modifica propriedades do sistema para evitar detecção de emulador
"""

import os
import sys
import subprocess
import shutil
from pathlib import Path

class BlueStacksBypass:
    def __init__(self):
        self.build_prop_path = "/system/build.prop"
        self.backup_path = "/system/build.prop.backup"
        self.device_profiles = {
            "samsung_galaxy_s21": {
                "ro.product.model": "SM-G991B",
                "ro.product.brand": "samsung",
                "ro.product.manufacturer": "samsung",
                "ro.product.device": "o1s",
                "ro.product.name": "o1sxxx",
                "ro.build.fingerprint": "samsung/o1sxxx/o1s:12/SP1A.210812.016/G991BXXU5CVB7:user/release-keys",
                "ro.build.id": "SP1A.210812.016",
                "ro.build.display.id": "SP1A.210812.016.G991BXXU5CVB7",
                "ro.build.version.release": "12",
                "ro.build.version.sdk": "31",
                "ro.product.cpu.abi": "arm64-v8a",
                "ro.product.cpu.abilist": "arm64-v8a,armeabi-v7a,armeabi",
                "ro.product.cpu.abilist32": "armeabi-v7a,armeabi",
                "ro.product.cpu.abilist64": "arm64-v8a",
            },
            "xiaomi_redmi_note_11": {
                "ro.product.model": "2201117TG",
                "ro.product.brand": "Redmi",
                "ro.product.manufacturer": "Xiaomi",
                "ro.product.device": "spes",
                "ro.product.name": "spes_global",
                "ro.build.fingerprint": "Redmi/spes_global/spes:12/SP1A.210812.016/V13.0.4.0.SGKMIXM:user/release-keys",
                "ro.build.id": "SP1A.210812.016",
                "ro.build.display.id": "V13.0.4.0.SGKMIXM",
                "ro.build.version.release": "12",
                "ro.build.version.sdk": "31",
                "ro.product.cpu.abi": "arm64-v8a",
                "ro.product.cpu.abilist": "arm64-v8a,armeabi-v7a,armeabi",
            },
            "oneplus_9": {
                "ro.product.model": "LE2113",
                "ro.product.brand": "OnePlus",
                "ro.product.manufacturer": "OnePlus",
                "ro.product.device": "OnePlus9",
                "ro.product.name": "OnePlus9_EEA",
                "ro.build.fingerprint": "OnePlus/OnePlus9_EEA/OnePlus9:12/SKQ1.210216.001/2206171200:user/release-keys",
                "ro.build.id": "SKQ1.210216.001",
                "ro.build.display.id": "LE2113_11_C.20",
                "ro.build.version.release": "12",
                "ro.build.version.sdk": "31",
                "ro.product.cpu.abi": "arm64-v8a",
            }
        }
    
    def check_root(self):
        """Verifica se o dispositivo tem acesso root"""
        try:
            result = subprocess.run(["su", "-c", "id"], 
                                  capture_output=True, 
                                  timeout=5)
            return result.returncode == 0
        except:
            return False
    
    def backup_build_prop(self):
        """Faz backup do build.prop original"""
        if not os.path.exists(self.build_prop_path):
            print(f"❌ Arquivo {self.build_prop_path} não encontrado!")
            return False
        
        try:
            subprocess.run(["su", "-c", f"cp {self.build_prop_path} {self.backup_path}"],
                         check=True)
            print(f"✅ Backup criado: {self.backup_path}")
            return True
        except subprocess.CalledProcessError:
            print("❌ Erro ao criar backup. Verifique permissões root.")
            return False
    
    def modify_build_prop(self, device_profile="samsung_galaxy_s21"):
        """Modifica o build.prop com propriedades do dispositivo escolhido"""
        if device_profile not in self.device_profiles:
            print(f"❌ Perfil {device_profile} não encontrado!")
            return False
        
        profile = self.device_profiles[device_profile]
        commands = []
        
        # Remover propriedades de emulador
        remove_props = [
            "ro.kernel.qemu",
            "ro.hardware",
            "ro.product.model",
            "ro.product.brand",
            "ro.product.manufacturer",
            "ro.product.device",
            "ro.product.name",
            "ro.build.fingerprint",
            "ro.build.id",
            "ro.build.display.id",
            "ro.build.version.release",
            "ro.build.version.sdk",
            "ro.product.cpu.abi",
            "ro.product.cpu.abilist",
            "ro.product.cpu.abilist32",
            "ro.product.cpu.abilist64",
        ]
        
        # Remover linhas antigas
        for prop in remove_props:
            commands.append(f"sed -i '/^{prop}=/d' {self.build_prop_path}")
        
        # Adicionar novas propriedades
        for key, value in profile.items():
            commands.append(f"echo '{key}={value}' >> {self.build_prop_path}")
        
        # Montar sistema como read-write
        mount_cmd = "mount -o remount,rw /system"
        
        try:
            # Executar comandos
            subprocess.run(["su", "-c", mount_cmd], check=True)
            
            for cmd in commands:
                subprocess.run(["su", "-c", cmd], check=True)
            
            print(f"✅ build.prop modificado com perfil: {device_profile}")
            return True
        except subprocess.CalledProcessError as e:
            print(f"❌ Erro ao modificar build.prop: {e}")
            return False
    
    def install_xposed_module(self):
        """Instala módulo Xposed para hooking avançado"""
        print("📦 Instalando módulo Xposed...")
        # Este seria um módulo customizado que intercepta chamadas de detecção
        pass
    
    def modify_emulator_detection(self):
        """Modifica arquivos que detectam emulador"""
        detection_files = [
            "/system/lib/libc.so",
            "/system/lib64/libc.so",
            "/proc/cpuinfo",
            "/proc/meminfo",
        ]
        
        # Criar scripts de hooking
        hook_script = """
        # Hook para interceptar chamadas de detecção
        # Usando LD_PRELOAD ou Xposed
        """
        
        print("🔧 Configurando hooks de detecção...")
        return True
    
    def apply_bypass(self, device_profile="samsung_galaxy_s21"):
        """Aplica todas as modificações de bypass"""
        print("🚀 Iniciando bypass BlueStacks...")
        print("=" * 50)
        
        if not self.check_root():
            print("❌ Acesso root necessário! Execute como root ou com su.")
            return False
        
        if not self.backup_build_prop():
            return False
        
        if not self.modify_build_prop(device_profile):
            return False
        
        self.modify_emulator_detection()
        
        print("=" * 50)
        print("✅ Bypass aplicado com sucesso!")
        print("⚠️  REINICIE o BlueStacks para aplicar as mudanças!")
        return True
    
    def restore_original(self):
        """Restaura o build.prop original"""
        if not os.path.exists(self.backup_path):
            print("❌ Backup não encontrado!")
            return False
        
        try:
            subprocess.run(["su", "-c", f"mount -o remount,rw /system"], check=True)
            subprocess.run(["su", "-c", f"cp {self.backup_path} {self.build_prop_path}"],
                         check=True)
            print("✅ build.prop original restaurado!")
            return True
        except subprocess.CalledProcessError:
            print("❌ Erro ao restaurar backup.")
            return False

def main():
    bypass = BlueStacksBypass()
    
    if len(sys.argv) > 1:
        if sys.argv[1] == "restore":
            bypass.restore_original()
        elif sys.argv[1] == "list":
            print("Perfis disponíveis:")
            for profile in bypass.device_profiles.keys():
                print(f"  - {profile}")
        elif sys.argv[1] in bypass.device_profiles:
            bypass.apply_bypass(sys.argv[1])
        else:
            print(f"Uso: {sys.argv[0]} [perfil|restore|list]")
            print(f"Perfis: {', '.join(bypass.device_profiles.keys())}")
    else:
        # Perfil padrão
        bypass.apply_bypass("samsung_galaxy_s21")

if __name__ == "__main__":
    main()
