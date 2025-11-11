using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BypassBlueStacks
{
    public partial class MainForm : Form
    {
        // Cores modernas
        private readonly Color bgColor = Color.FromArgb(30, 30, 30);
        private readonly Color accentColor = Color.FromArgb(0, 120, 212);
        private readonly Color successColor = Color.FromArgb(16, 124, 16);
        private readonly Color warningColor = Color.FromArgb(255, 170, 0);
        private readonly Color errorColor = Color.FromArgb(209, 52, 56);

        // Controles
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel pnlStatus;
        private Label lblBlueStacksStatus;
        private Label lblAdbStatus;
        private Label lblBypassStatus;
        private GroupBox gbDevice;
        private ComboBox cmbDevice;
        private Label lblDeviceInfo;
        private Button btnActivate;
        private Button btnDeactivate;
        private Button btnConnectAdb;
        private GroupBox gbLog;
        private TextBox txtLog;
        private System.Windows.Forms.Timer timerMonitor;

        // Variáveis
        private bool bluestacksRunning = false;
        private bool bypassActive = false;
        private bool adbConnected = false;
        private bool isConnectingAdb = false; // Flag para evitar múltiplas tentativas
        private CancellationTokenSource daemonCts;
        private Task daemonTask;

        // Perfis de dispositivos
        private readonly Dictionary<string, DeviceProfile> deviceProfiles = new Dictionary<string, DeviceProfile>
        {
            {
                "samsung_galaxy_s21",
                new DeviceProfile
                {
                    Name = "Samsung Galaxy S21",
                    Model = "SM-G991B",
                    Brand = "samsung",
                    Manufacturer = "samsung",
                    Device = "o1s",
                    Fingerprint = "samsung/o1sxxx/o1s:12/SP1A.210812.016/G991BXXU5CVB7:user/release-keys"
                }
            },
            {
                "xiaomi_redmi_note_11",
                new DeviceProfile
                {
                    Name = "Xiaomi Redmi Note 11",
                    Model = "2201117TG",
                    Brand = "Redmi",
                    Manufacturer = "Xiaomi",
                    Device = "spes",
                    Fingerprint = "Redmi/spes_global/spes:12/SP1A.210812.016/V13.0.4.0.SGKMIXM:user/release-keys"
                }
            },
            {
                "oneplus_9",
                new DeviceProfile
                {
                    Name = "OnePlus 9",
                    Model = "LE2113",
                    Brand = "OnePlus",
                    Manufacturer = "OnePlus",
                    Device = "OnePlus9",
                    Fingerprint = "OnePlus/OnePlus9_EEA/OnePlus9:12/SKQ1.210216.001/2206171200:user/release-keys"
                }
            },
            {
                "google_pixel_6",
                new DeviceProfile
                {
                    Name = "Google Pixel 6",
                    Model = "Pixel 6",
                    Brand = "google",
                    Manufacturer = "Google",
                    Device = "oriole",
                    Fingerprint = "google/oriole/oriole:13/TQ1A.230105.002/9325679:user/release-keys"
                }
            }
        };

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
            StartMonitoring();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Bypass BlueStacks - Simular Mobile";
            this.Size = new Size(600, 550);
            this.MinimumSize = new Size(600, 550);
            this.MaximumSize = new Size(600, 550);
            this.BackColor = bgColor;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            // Title
            lblTitle = new Label
            {
                Text = "🔓 Bypass BlueStacks",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bgColor,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            this.Controls.Add(lblTitle);

            // Subtitle
            lblSubtitle = new Label
            {
                Text = "Simule um dispositivo mobile real",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(204, 204, 204),
                BackColor = bgColor,
                AutoSize = true,
                Location = new Point(20, 60)
            };
            this.Controls.Add(lblSubtitle);

            // Status Panel
            pnlStatus = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 45),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(20, 100),
                Size = new Size(560, 100)
            };

            lblBlueStacksStatus = new Label
            {
                Text = "🔍 Procurando BlueStacks...",
                Font = new Font("Segoe UI", 10),
                ForeColor = warningColor,
                BackColor = Color.FromArgb(45, 45, 45),
                Location = new Point(15, 10),
                Size = new Size(530, 20)
            };
            pnlStatus.Controls.Add(lblBlueStacksStatus);

            lblAdbStatus = new Label
            {
                Text = "📱 ADB: Não conectado",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(136, 136, 136),
                BackColor = Color.FromArgb(45, 45, 45),
                Location = new Point(15, 35),
                Size = new Size(530, 20)
            };
            pnlStatus.Controls.Add(lblAdbStatus);

            lblBypassStatus = new Label
            {
                Text = "⚙️ Bypass: Inativo",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(136, 136, 136),
                BackColor = Color.FromArgb(45, 45, 45),
                Location = new Point(15, 60),
                Size = new Size(530, 20)
            };
            pnlStatus.Controls.Add(lblBypassStatus);

            this.Controls.Add(pnlStatus);

            // Device Selection
            gbDevice = new GroupBox
            {
                Text = "Selecionar Dispositivo",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bgColor,
                Location = new Point(20, 220),
                Size = new Size(560, 100)
            };

            cmbDevice = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 30),
                Size = new Size(520, 30),
                DataSource = deviceProfiles.Keys.ToList()
            };
            cmbDevice.SelectedIndexChanged += CmbDevice_SelectedIndexChanged;
            gbDevice.Controls.Add(cmbDevice);

            lblDeviceInfo = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(204, 204, 204),
                BackColor = bgColor,
                Location = new Point(20, 65),
                Size = new Size(520, 20)
            };
            gbDevice.Controls.Add(lblDeviceInfo);

            this.Controls.Add(gbDevice);
            CmbDevice_SelectedIndexChanged(null, null);

            // Buttons
            btnActivate = new Button
            {
                Text = "🚀 Ativar Bypass",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = accentColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(20, 340),
                Size = new Size(180, 45),
                Enabled = false
            };
            btnActivate.FlatAppearance.BorderSize = 0;
            btnActivate.Click += BtnActivate_Click;
            this.Controls.Add(btnActivate);

            btnDeactivate = new Button
            {
                Text = "⏹️ Desativar Bypass",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(102, 102, 102),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(220, 340),
                Size = new Size(180, 45),
                Enabled = false
            };
            btnDeactivate.FlatAppearance.BorderSize = 0;
            btnDeactivate.Click += BtnDeactivate_Click;
            this.Controls.Add(btnDeactivate);

            btnConnectAdb = new Button
            {
                Text = "🔌 Conectar ADB",
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(68, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(420, 340),
                Size = new Size(160, 45),
            };
            btnConnectAdb.FlatAppearance.BorderSize = 0;
            btnConnectAdb.Click += BtnConnectAdb_Click;
            this.Controls.Add(btnConnectAdb);

            // Log
            gbLog = new GroupBox
            {
                Text = "Log",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bgColor,
                Location = new Point(20, 400),
                Size = new Size(560, 100)
            };

            txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(26, 26, 26),
                ForeColor = Color.Lime,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(10, 25),
                Size = new Size(540, 65)
            };
            gbLog.Controls.Add(txtLog);

            this.Controls.Add(gbLog);

            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            Log("Sistema iniciado. Aguardando BlueStacks...");
        }

        private void CmbDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDevice.SelectedItem != null)
            {
                string deviceKey = cmbDevice.SelectedItem.ToString();
                if (deviceProfiles.ContainsKey(deviceKey))
                {
                    var device = deviceProfiles[deviceKey];
                    lblDeviceInfo.Text = $"📱 {device.Name} | Modelo: {device.Model}";
                }
            }
        }

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Log), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private bool CheckBlueStacks()
        {
            string[] processes = { "HD-Player", "BlueStacks", "BlueStacksX", "BstkSVC" };
            
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    if (processes.Any(p => proc.ProcessName.Contains(p)))
                    {
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        private bool CheckAdbConnection()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "adb",
                    Arguments = "devices",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        return output.Contains("127.0.0.1:5555") || output.Contains("device");
                    }
                }
            }
            catch { }
            return false;
        }

        private void StartMonitoring()
        {
            timerMonitor = new System.Windows.Forms.Timer
            {
                Interval = 2000 // 2 segundos
            };
            timerMonitor.Tick += TimerMonitor_Tick;
            timerMonitor.Start();
            
            // Tentar conectar imediatamente se BlueStacks já estiver rodando
            if (CheckBlueStacks())
            {
                Task.Run(() => 
                {
                    Thread.Sleep(1000); // Aguardar 1 segundo para garantir que tudo está inicializado
                    ConnectAdbAutomatically();
                });
            }
        }

        private void TimerMonitor_Tick(object sender, EventArgs e)
        {
            // Verificar BlueStacks
            bool running = CheckBlueStacks();
            if (running != bluestacksRunning)
            {
                bluestacksRunning = running;
                if (running)
                {
                    lblBlueStacksStatus.Text = "✅ BlueStacks: Detectado";
                    lblBlueStacksStatus.ForeColor = successColor;
                    Log("✅ BlueStacks detectado!");
                }
                else
                {
                    lblBlueStacksStatus.Text = "🔍 Procurando BlueStacks...";
                    lblBlueStacksStatus.ForeColor = warningColor;
                }
                UpdateButtonStates();
            }

            // Verificar ADB e conectar automaticamente se necessário
            if (bluestacksRunning)
            {
                bool connected = CheckAdbConnection();
                if (connected != adbConnected)
                {
                    adbConnected = connected;
                    if (connected)
                    {
                        lblAdbStatus.Text = "📱 ADB: Conectado";
                        lblAdbStatus.ForeColor = successColor;
                    }
                    else
                    {
                        lblAdbStatus.Text = "📱 ADB: Não conectado";
                        lblAdbStatus.ForeColor = Color.FromArgb(136, 136, 136);
                        // Tentar conectar automaticamente (apenas se não estiver tentando já)
                        if (!isConnectingAdb)
                        {
                            Task.Run(() => ConnectAdbAutomatically());
                        }
                    }
                    UpdateButtonStates();
                }
            }
            else
            {
                // Se BlueStacks não está rodando, resetar status ADB
                if (adbConnected)
                {
                    adbConnected = false;
                    lblAdbStatus.Text = "📱 ADB: Não conectado";
                    lblAdbStatus.ForeColor = Color.FromArgb(136, 136, 136);
                    UpdateButtonStates();
                }
            }
        }

        private void UpdateButtonStates()
        {
            if (bluestacksRunning && adbConnected)
            {
                if (bypassActive)
                {
                    btnActivate.Enabled = false;
                    btnDeactivate.Enabled = true;
                }
                else
                {
                    btnActivate.Enabled = true;
                    btnDeactivate.Enabled = false;
                }
            }
            else
            {
                btnActivate.Enabled = false;
                btnDeactivate.Enabled = false;
            }
        }

        private void ConnectAdbAutomatically()
        {
            // Evitar múltiplas tentativas simultâneas
            if (adbConnected || isConnectingAdb) return;
            
            isConnectingAdb = true;

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "adb",
                    Arguments = "connect 127.0.0.1:5555",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit(5000); // Timeout de 5 segundos
                        
                        string fullOutput = output + error;
                        
                        if (fullOutput.ToLower().Contains("connected") || 
                            fullOutput.ToLower().Contains("already") ||
                            fullOutput.ToLower().Contains("successfully"))
                        {
                                Invoke(new Action(() =>
                                {
                                    adbConnected = true;
                                    isConnectingAdb = false; // Resetar flag
                                    lblAdbStatus.Text = "📱 ADB: Conectado";
                                    lblAdbStatus.ForeColor = successColor;
                                    Log("✅ ADB conectado automaticamente!");
                                    UpdateButtonStates();
                                }));
                        }
                        else
                        {
                            // Tentar outras portas comuns do BlueStacks
                            TryConnectOtherPorts();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    Log($"⚠️ Tentativa automática de conexão falhou: {ex.Message}");
                }));
            }
            finally
            {
                isConnectingAdb = false;
            }
        }

        private void TryConnectOtherPorts()
        {
            // Tentar portas alternativas do BlueStacks
            int[] ports = { 5556, 5557, 5558, 5559, 5560 }; // 5555 já foi tentada
            
            foreach (int port in ports)
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "adb",
                        Arguments = $"connect 127.0.0.1:{port}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    using (Process process = Process.Start(psi))
                    {
                        if (process != null)
                        {
                            string output = process.StandardOutput.ReadToEnd();
                            process.WaitForExit(3000);
                            
                            if (output.ToLower().Contains("connected") || 
                                output.ToLower().Contains("already"))
                            {
                                Invoke(new Action(() =>
                                {
                                    adbConnected = true;
                                    isConnectingAdb = false; // Resetar flag
                                    lblAdbStatus.Text = $"📱 ADB: Conectado (porta {port})";
                                    lblAdbStatus.ForeColor = successColor;
                                    Log($"✅ ADB conectado automaticamente na porta {port}!");
                                    UpdateButtonStates();
                                }));
                                return;
                            }
                        }
                    }
                }
                catch { }
            }
            
            // Se nenhuma porta funcionou, resetar flag
            Invoke(new Action(() =>
            {
                isConnectingAdb = false;
            }));
        }

        private void BtnConnectAdb_Click(object sender, EventArgs e)
        {
            Log("Tentando conectar ao BlueStacks via ADB...");
            
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "adb",
                    Arguments = "connect 127.0.0.1:5555",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        
                        string fullOutput = output + error;
                        
                        if (fullOutput.ToLower().Contains("connected") || 
                            fullOutput.ToLower().Contains("already") ||
                            fullOutput.ToLower().Contains("successfully"))
                        {
                            adbConnected = true;
                            lblAdbStatus.Text = "📱 ADB: Conectado";
                            lblAdbStatus.ForeColor = successColor;
                            Log("✅ ADB conectado com sucesso!");
                            UpdateButtonStates();
                        }
                        else
                        {
                            Log("⚠️ Não foi possível conectar na porta 5555. Tentando outras portas...");
                            TryConnectOtherPorts();
                            
                            if (!adbConnected)
                            {
                                Log("⚠️ Não foi possível conectar. Verifique se a depuração USB está habilitada.");
                                MessageBox.Show(
                                    "Não foi possível conectar ao BlueStacks.\n\n" +
                                    "Certifique-se de:\n" +
                                    "1. BlueStacks está aberto\n" +
                                    "2. Depuração USB está habilitada\n" +
                                    "3. ADB está instalado no sistema",
                                    "ADB não conectado",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao conectar: {ex.Message}");
                MessageBox.Show(
                    "ADB não está instalado ou não está no PATH.\n\n" +
                    "Baixe o Android SDK Platform Tools:\n" +
                    "https://developer.android.com/studio/releases/platform-tools",
                    "ADB não encontrado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            if (cmbDevice.SelectedItem == null) return;

            string deviceKey = cmbDevice.SelectedItem.ToString();
            if (!deviceProfiles.ContainsKey(deviceKey)) return;

            var device = deviceProfiles[deviceKey];
            Log($"🚀 Ativando bypass com perfil: {device.Name}");

            Task.Run(() => ApplyBypass(device, deviceKey));
        }

        private void ApplyBypass(DeviceProfile device, string deviceKey)
        {
            try
            {
                Log("🔧 Aplicando propriedades do dispositivo...");
                ApplyPropertiesOnce(device);

                Log("🔍 Verificando propriedades...");
                VerifyBypass(device);

                StartDaemon(deviceKey);

                bypassActive = true;
                Invoke(new Action(() =>
                {
                    lblBypassStatus.Text = "⚙️ Bypass: ATIVO";
                    lblBypassStatus.ForeColor = successColor;
                    UpdateButtonStates();
                }));
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao aplicar bypass: {ex.Message}");
                Invoke(new Action(() =>
                {
                    MessageBox.Show($"Falha ao aplicar bypass:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        private void ApplyPropertiesOnce(DeviceProfile device)
        {
            string[][] commands = new string[][]
            {
                new[] { "adb", "shell", "setprop", "ro.kernel.qemu", "0" },
                new[] { "adb", "shell", "setprop", "ro.hardware", "qcom" },
                new[] { "adb", "shell", "setprop", "ro.product.model", device.Model },
                new[] { "adb", "shell", "setprop", "ro.product.brand", device.Brand },
                new[] { "adb", "shell", "setprop", "ro.product.manufacturer", device.Manufacturer },
                new[] { "adb", "shell", "setprop", "ro.product.device", device.Device },
                new[] { "adb", "shell", "setprop", "ro.build.fingerprint", device.Fingerprint }
            };

            foreach (var cmd in commands)
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = cmd[0],
                        Arguments = string.Join(" ", cmd.Skip(1)),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    using (Process process = Process.Start(psi))
                    {
                        if (process != null)
                        {
                            process.WaitForExit(2000);
                            if (process.ExitCode == 0)
                            {
                                Log($"✅ {cmd[2]} = {cmd[3]}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"⚠️ Erro ao definir {cmd[2]}: {ex.Message}");
                }
            }
        }

        private void VerifyBypass(DeviceProfile device)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "adb",
                    Arguments = "shell getprop ro.product.model",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        
                        if (output.Contains(device.Model))
                        {
                            Log($"✅ Modelo verificado: {output.Trim()}");
                        }
                        else
                        {
                            Log($"⚠️ Modelo não corresponde: {output.Trim()}");
                        }
                    }
                }
            }
            catch { }
        }

        private void StartDaemon(string deviceKey)
        {
            if (daemonCts != null && !daemonCts.IsCancellationRequested)
            {
                return; // Daemon já está rodando
            }

            daemonCts = new CancellationTokenSource();
            var device = deviceProfiles[deviceKey];

            daemonTask = Task.Run(() =>
            {
                while (!daemonCts.Token.IsCancellationRequested && bypassActive)
                {
                    try
                    {
                        if (CheckAdbConnection())
                        {
                            ApplyPropertiesOnce(device);
                        }
                        Thread.Sleep(5000); // Reaplica a cada 5 segundos
                    }
                    catch
                    {
                        Thread.Sleep(5000);
                    }
                }
            }, daemonCts.Token);

            Log("🔄 Daemon iniciado para manter bypass ativo");
        }

        private void BtnDeactivate_Click(object sender, EventArgs e)
        {
            Log("⏹️ Desativando bypass...");

            bypassActive = false;
            daemonCts?.Cancel();

            Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "adb",
                        Arguments = "shell setprop ro.kernel.qemu \"\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = Process.Start(psi))
                    {
                        if (process != null)
                        {
                            process.WaitForExit(5000);
                        }
                    }

                    Log("✅ Bypass desativado");
                    Log("🛑 Daemon parado");

                    Invoke(new Action(() =>
                    {
                        lblBypassStatus.Text = "⚙️ Bypass: Inativo";
                        lblBypassStatus.ForeColor = Color.FromArgb(136, 136, 136);
                        UpdateButtonStates();
                    }));
                }
                catch (Exception ex)
                {
                    Log($"❌ Erro ao desativar: {ex.Message}");
                }
            });
        }
    }

    public class DeviceProfile
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public string Manufacturer { get; set; }
        public string Device { get; set; }
        public string Fingerprint { get; set; }
    }
}
