using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private string adbPath = "adb"; // Caminho padrão, será substituído pelo ADB embutido
        private string tempAdbFolder = string.Empty;

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
                "samsung_galaxy_s21_ultra",
                new DeviceProfile
                {
                    Name = "Samsung Galaxy S21 Ultra",
                    Model = "SM-G998B",
                    Brand = "samsung",
                    Manufacturer = "samsung",
                    Device = "p3s",
                    Fingerprint = "samsung/p3sxxx/p3s:13/TP1A.220624.014/G998BXXU6EWB7:user/release-keys"
                }
            },
            {
                "asus_rog_phone_2",
                new DeviceProfile
                {
                    Name = "ASUS ROG Phone 2",
                    Model = "ASUS_I001DC",
                    Brand = "asus",
                    Manufacturer = "asus",
                    Device = "ASUS_I001_1",
                    Fingerprint = "asus/WW_ASUS_I001_1/ASUS_I001_1:11/RKQ1.201022.002/18.0610.2201.200-0:user/release-keys"
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
            },
            {
                "samsung_galaxy_a52_freefire",
                new DeviceProfile
                {
                    Name = "Samsung Galaxy A52 (Free Fire)",
                    Model = "SM-A525F",
                    Brand = "samsung",
                    Manufacturer = "samsung",
                    Device = "a52x",
                    Fingerprint = "samsung/a52xinsxx/a52x:13/TP1A.220624.014/A525FXXU6DWB1:user/release-keys"
                }
            },
            {
                "xiaomi_redmi_note_10_freefire",
                new DeviceProfile
                {
                    Name = "Xiaomi Redmi Note 10 (Free Fire)",
                    Model = "M2101K9G",
                    Brand = "Redmi",
                    Manufacturer = "Xiaomi",
                    Device = "mojito",
                    Fingerprint = "Redmi/mojito_global/mojito:12/SKQ1.210908.001/V13.0.2.0.SKGMIXM:user/release-keys"
                }
            },
            {
                "oppo_a94_lastisland",
                new DeviceProfile
                {
                    Name = "OPPO A94 (Last Island)",
                    Model = "CPH2211",
                    Brand = "OPPO",
                    Manufacturer = "OPPO",
                    Device = "CPH2211",
                    Fingerprint = "OPPO/CPH2211EEA/OP4F81L1:12/SP1A.210812.016/CPH2211_11_A.20:user/release-keys"
                }
            },
            {
                "vivo_y20_lastisland",
                new DeviceProfile
                {
                    Name = "Vivo Y20 (Last Island)",
                    Model = "V2027",
                    Brand = "vivo",
                    Manufacturer = "vivo",
                    Device = "2027",
                    Fingerprint = "vivo/2027/2027:11/RP1A.200720.012/compiler08032130:user/release-keys"
                }
            }
        };

        public MainForm()
        {
            InitializeComponent();
            ExtractEmbeddedAdb();
            SetupUI();
            StartMonitoring();
        }

        private void ExtractEmbeddedAdb()
        {
            try
            {
                // Criar pasta temporária para ADB
                tempAdbFolder = Path.Combine(Path.GetTempPath(), "BypassBlueStacks_ADB", Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempAdbFolder);

                // Arquivos do ADB que precisam ser extraídos
                string[] adbFiles = { "adb.exe", "AdbWinApi.dll", "AdbWinUsbApi.dll" };
                bool allFilesFound = true;

                foreach (string fileName in adbFiles)
                {
                    string resourceName = $"BypassBlueStacks.Resources.{fileName}";
                    string outputPath = Path.Combine(tempAdbFolder, fileName);

                    // Tentar extrair do assembly
                    using (Stream resourceStream = typeof(MainForm).Assembly.GetManifestResourceStream(resourceName))
                    {
                        if (resourceStream != null)
                        {
                            using (FileStream fileStream = new FileStream(outputPath, FileMode.Create))
                            {
                                resourceStream.CopyTo(fileStream);
                            }
                        }
                        else
                        {
                            // Se não encontrar no assembly, tentar procurar na pasta do executável
                            string localPath = Path.Combine(Application.StartupPath, fileName);
                            if (File.Exists(localPath))
                            {
                                File.Copy(localPath, outputPath, true);
                            }
                            else
                            {
                                allFilesFound = false;
                                break;
                            }
                        }
                    }
                }

                if (allFilesFound)
                {
                    // Usar ADB extraído
                    adbPath = Path.Combine(tempAdbFolder, "adb.exe");
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => Log($"✅ ADB embutido extraído para: {tempAdbFolder}")));
                    }
                    else
                    {
                        Log($"✅ ADB embutido extraído para: {tempAdbFolder}");
                    }
                }
                else
                {
                    // Se não encontrar arquivos embutidos, usar ADB do sistema
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => Log("⚠️ ADB embutido não encontrado. Usando ADB do sistema (se disponível).")));
                    }
                    else
                    {
                        Log("⚠️ ADB embutido não encontrado. Usando ADB do sistema (se disponível).");
                    }
                    adbPath = "adb";
                }
            }
            catch (Exception ex)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => Log($"⚠️ Erro ao extrair ADB embutido: {ex.Message}. Usando ADB do sistema.")));
                }
                else
                {
                    Log($"⚠️ Erro ao extrair ADB embutido: {ex.Message}. Usando ADB do sistema.");
                }
                adbPath = "adb";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Limpar arquivos temporários do ADB
            CleanupTempAdb();
            base.OnFormClosing(e);
        }

        private void CleanupTempAdb()
        {
            try
            {
                if (!string.IsNullOrEmpty(tempAdbFolder) && Directory.Exists(tempAdbFolder))
                {
                    // Tentar deletar arquivos
                    try
                    {
                        string[] files = Directory.GetFiles(tempAdbFolder);
                        foreach (string file in files)
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch { }
                        }
                        Directory.Delete(tempAdbFolder);
                    }
                    catch
                    {
                        // Se não conseguir deletar agora, marcar para deletar na próxima inicialização
                        // (Windows pode estar usando os arquivos ainda)
                    }
                }
            }
            catch { }
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

            // Botão Verificar Root
            Button btnCheckRoot = new Button
            {
                Text = "🔓 Verificar Root",
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(68, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(420, 390),
                Size = new Size(160, 30),
            };
            btnCheckRoot.FlatAppearance.BorderSize = 0;
            btnCheckRoot.Click += BtnCheckRoot_Click;
            this.Controls.Add(btnCheckRoot);

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
            if (adbPath != "adb" && File.Exists(adbPath))
            {
                Log($"✅ ADB embutido carregado: {Path.GetDirectoryName(adbPath)}");
            }
            else
            {
                Log("⚠️ ADB embutido não encontrado. Tentando usar ADB do sistema...");
            }
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
                    FileName = adbPath,
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
                    FileName = adbPath,
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
                        FileName = adbPath,
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
                    FileName = adbPath,
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
                            
                            // Verificar root automaticamente após conectar
                            Task.Run(() =>
                            {
                                Thread.Sleep(500);
                                CheckRoot();
                            });
                            
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

                // Verificar root antes de aplicar propriedades específicas
                bool rootAvailable = CheckRoot();
                if (rootAvailable)
                {
                    Log("✅ Root detectado! Modificações permanentes serão aplicadas.");
                }
                
                Log("🔧 Aplicando propriedades adicionais para jogos...");
                ApplyGameSpecificProperties(deviceKey);

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

        private void ApplyGameSpecificProperties(string deviceKey)
        {
            // Propriedades específicas para Last Island Survival
            if (deviceKey.Contains("lastisland"))
            {
                string[][] lastIslandCommands = new string[][]
                {
                    // Propriedades adicionais específicas do Last Island
                    new[] { adbPath, "shell", "setprop", "ro.product.mod_device", "CPH2211_11" },
                    new[] { adbPath, "shell", "setprop", "ro.product.odm.brand", "OPPO" },
                    new[] { adbPath, "shell", "setprop", "ro.product.odm.manufacturer", "OPPO" },
                    new[] { adbPath, "shell", "setprop", "ro.product.odm.model", "CPH2211" },
                    new[] { adbPath, "shell", "setprop", "ro.product.odm.device", "CPH2211" },
                    new[] { adbPath, "shell", "setprop", "ro.product.vendor.brand", "OPPO" },
                    new[] { adbPath, "shell", "setprop", "ro.product.vendor.manufacturer", "OPPO" },
                    new[] { adbPath, "shell", "setprop", "ro.product.vendor.model", "CPH2211" },
                    new[] { adbPath, "shell", "setprop", "ro.product.vendor.device", "CPH2211" },
                    // Propriedades de segurança (Last Island verifica)
                    new[] { adbPath, "shell", "setprop", "ro.vendor.build.security_patch", "2023-03-01" },
                    new[] { adbPath, "shell", "setprop", "ro.build.version.security_patch", "2023-03-01" },
                    // Propriedades de boot (importante)
                    new[] { adbPath, "shell", "setprop", "ro.bootmode", "unknown" },
                    new[] { adbPath, "shell", "setprop", "ro.boot.hardware", "qcom" },
                    new[] { adbPath, "shell", "setprop", "ro.boot.serialno", "R58M123456" },
                    // Propriedades de sistema (Last Island verifica extensivamente)
                    new[] { adbPath, "shell", "setprop", "ro.serialno", "R58M123456" },
                    new[] { adbPath, "shell", "setprop", "sys.usb.state", "mtp,adb" },
                    new[] { adbPath, "shell", "setprop", "ro.adb.secure", "1" },
                    // Propriedades críticas que Last Island verifica
                    new[] { adbPath, "shell", "setprop", "ro.build.flavor", "a52xinsxx-user" },
                    new[] { adbPath, "shell", "setprop", "ro.product.board", "lahaina" },
                    new[] { adbPath, "shell", "setprop", "ro.product.vendor.board", "lahaina" },
                    new[] { adbPath, "shell", "setprop", "ro.product.vendor.name", "CPH2211" },
                    new[] { adbPath, "shell", "setprop", "ro.product.vendor.model", "CPH2211" },
                    // Propriedades de inicialização
                    new[] { adbPath, "shell", "setprop", "ro.boot.verifiedbootstate", "green" },
                    new[] { adbPath, "shell", "setprop", "ro.boot.veritymode", "enforcing" },
                    new[] { adbPath, "shell", "setprop", "ro.boot.flash.locked", "1" },
                    // Propriedades de hardware específicas
                    new[] { adbPath, "shell", "setprop", "ro.hardware.vulkan", "pastel" },
                    new[] { adbPath, "shell", "setprop", "ro.hardware.audio.primary", "lahaina" },
                    new[] { adbPath, "shell", "setprop", "ro.hardware.bluetooth", "lahaina" },
                    // Propriedades de DRM (jogos verificam)
                    new[] { adbPath, "shell", "setprop", "ro.vendor.drm", "1" },
                    new[] { adbPath, "shell", "setprop", "ro.vendor.media", "1" },
                    // Propriedades de telemetria (Last Island pode verificar)
                    new[] { adbPath, "shell", "setprop", "ro.telephony.default_network", "22" },
                    new[] { adbPath, "shell", "setprop", "ro.telephony.call_ring.multiple", "false" }
                };

                foreach (var cmd in lastIslandCommands)
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = cmd[0],
                            Arguments = string.Join(" ", cmd.Skip(1)),
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        using (Process process = Process.Start(psi))
                        {
                            if (process != null)
                            {
                                process.WaitForExit(2000);
                            }
                        }
                    }
                    catch { }
                }
                Log("✅ Propriedades específicas do Last Island aplicadas");
                
                // Tentar modificar arquivos do sistema (pode não funcionar sem root)
                TryModifySystemFiles();
            }

            // Propriedades específicas para Free Fire
            if (deviceKey.Contains("freefire"))
            {
                string[][] freeFireCommands = new string[][]
                {
                    // Propriedades específicas do Free Fire
                    new[] { adbPath, "shell", "setprop", "ro.product.board", "lahaina" },
                    new[] { adbPath, "shell", "setprop", "ro.board.platform", "lahaina" },
                    new[] { adbPath, "shell", "setprop", "ro.chipname", "lahaina" },
                    new[] { adbPath, "shell", "setprop", "ro.product.board.platform", "lahaina" }
                };

                foreach (var cmd in freeFireCommands)
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = cmd[0],
                            Arguments = string.Join(" ", cmd.Skip(1)),
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        using (Process process = Process.Start(psi))
                        {
                            if (process != null)
                            {
                                process.WaitForExit(2000);
                            }
                        }
                    }
                    catch { }
                }
                Log("✅ Propriedades específicas do Free Fire aplicadas");
            }
        }

        private bool hasRoot = false;

        private bool CheckRoot()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = "shell su -c \"id\"",
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
                        process.WaitForExit(3000);
                        
                        bool rootAvailable = output.Contains("uid=0") || output.Contains("root");
                        if (rootAvailable != hasRoot)
                        {
                            hasRoot = rootAvailable;
                            if (hasRoot)
                            {
                                Log("✅ Root detectado! Usando métodos avançados de bypass.");
                            }
                            else
                            {
                                Log("⚠️ Root não disponível. Bypass limitado a propriedades temporárias.");
                            }
                        }
                        return rootAvailable;
                    }
                }
            }
            catch { }
            return false;
        }

        private void TryModifySystemFiles()
        {
            // Verificar se root está disponível
            bool rootAvailable = CheckRoot();
            
            if (rootAvailable)
            {
                Log("🔧 Root detectado! Modificando arquivos do sistema permanentemente...");
                
                // Comandos que requerem root para modificar arquivos do sistema
                string[][] rootCommands = new string[][]
                {
                    // Modificar build.prop permanentemente
                    new[] { adbPath, "shell", "su", "-c", "mount -o remount,rw /system" },
                    new[] { adbPath, "shell", "su", "-c", "sed -i 's/ro.kernel.qemu=1/ro.kernel.qemu=0/g' /system/build.prop" },
                    new[] { adbPath, "shell", "su", "-c", "sed -i '/ro.kernel.qemu=/d' /system/build.prop" },
                    new[] { adbPath, "shell", "su", "-c", "echo 'ro.kernel.qemu=0' >> /system/build.prop" },
                    new[] { adbPath, "shell", "su", "-c", "mount -o remount,ro /system" },
                    // Criar arquivo que indica dispositivo real
                    new[] { adbPath, "shell", "su", "-c", "touch /data/local/tmp/.real_device" },
                    new[] { adbPath, "shell", "su", "-c", "echo '0' > /data/local/tmp/qemu_flag" }
                };

                foreach (var cmd in rootCommands)
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = cmd[0],
                            Arguments = string.Join(" ", cmd.Skip(1)),
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        using (Process process = Process.Start(psi))
                        {
                            if (process != null)
                            {
                                process.WaitForExit(3000);
                            }
                        }
                    }
                    catch { }
                }
                
                Log("✅ Arquivos do sistema modificados permanentemente!");
                Log("⚠️ REINICIE o BlueStacks para aplicar mudanças permanentes!");
            }
            else
            {
                Log("⚠️ Root não disponível. Tentando métodos sem root...");
                
                // Comandos que podem funcionar sem root (limitados)
                string[][] noRootCommands = new string[][]
                {
                    // Tentar criar arquivo que indica dispositivo real (pode funcionar sem root)
                    new[] { adbPath, "shell", "touch", "/data/local/tmp/.real_device" },
                    new[] { adbPath, "shell", "echo", "real_device", ">", "/data/local/tmp/device_type" }
                };

                foreach (var cmd in noRootCommands)
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = cmd[0],
                            Arguments = string.Join(" ", cmd.Skip(1)),
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        using (Process process = Process.Start(psi))
                        {
                            if (process != null)
                            {
                                process.WaitForExit(2000);
                            }
                        }
                    }
                    catch { }
                }
                
                Log("⚠️ IMPORTANTE: Para bypass completo do Last Island, ative ROOT no BlueStacks!");
                Log("📖 Veja o guia: COMO_ATIVAR_ROOT.md");
            }
        }

        private void ApplyPropertiesOnce(DeviceProfile device)
        {
            string[][] commands = new string[][]
            {
                new[] { adbPath, "shell", "setprop", "ro.kernel.qemu", "0" },
                new[] { adbPath, "shell", "setprop", "ro.kernel.qemu.gles", "0" },
                new[] { adbPath, "shell", "setprop", "ro.hardware", "qcom" },
                new[] { adbPath, "shell", "setprop", "ro.hardware.egl", "adreno" },
                new[] { adbPath, "shell", "setprop", "ro.product.model", device.Model },
                new[] { adbPath, "shell", "setprop", "ro.product.brand", device.Brand },
                new[] { adbPath, "shell", "setprop", "ro.product.manufacturer", device.Manufacturer },
                new[] { adbPath, "shell", "setprop", "ro.product.device", device.Device },
                new[] { adbPath, "shell", "setprop", "ro.product.name", device.Device },
                new[] { adbPath, "shell", "setprop", "ro.build.fingerprint", device.Fingerprint },
                new[] { adbPath, "shell", "setprop", "ro.build.product", device.Device },
                // Propriedades de CPU (importante para Free Fire)
                new[] { adbPath, "shell", "setprop", "ro.product.cpu.abi", "arm64-v8a" },
                new[] { adbPath, "shell", "setprop", "ro.product.cpu.abilist", "arm64-v8a,armeabi-v7a,armeabi" },
                new[] { adbPath, "shell", "setprop", "ro.product.cpu.abilist32", "armeabi-v7a,armeabi" },
                new[] { adbPath, "shell", "setprop", "ro.product.cpu.abilist64", "arm64-v8a" },
                // Propriedades de build (verificadas por jogos)
                new[] { adbPath, "shell", "setprop", "ro.build.id", "SP1A.210812.016" },
                new[] { adbPath, "shell", "setprop", "ro.build.version.release", "12" },
                new[] { adbPath, "shell", "setprop", "ro.build.version.sdk", "31" },
                new[] { adbPath, "shell", "setprop", "ro.build.version.codename", "REL" },
                new[] { adbPath, "shell", "setprop", "ro.build.type", "user" },
                new[] { adbPath, "shell", "setprop", "ro.build.tags", "release-keys" },
                // Propriedades de hardware (importante para Last Island)
                new[] { adbPath, "shell", "setprop", "ro.board.platform", "lahaina" },
                new[] { adbPath, "shell", "setprop", "ro.chipname", "lahaina" },
                // Propriedades de display
                new[] { adbPath, "shell", "setprop", "ro.sf.lcd_density", "420" },
                // Remover flags de debug/emulador
                new[] { adbPath, "shell", "setprop", "ro.debuggable", "0" },
                new[] { adbPath, "shell", "setprop", "ro.secure", "1" },
                new[] { adbPath, "shell", "setprop", "ro.allow.mock.location", "0" },
                // Propriedades de característica (Free Fire verifica)
                new[] { adbPath, "shell", "setprop", "ro.product.characteristics", "phone,tablet" },
                // Propriedades específicas para Last Island Survival
                new[] { adbPath, "shell", "setprop", "ro.build.description", "a52xinsxx-user 13 TP1A.220624.014 A525FXXU6DWB1 release-keys" },
                new[] { adbPath, "shell", "setprop", "ro.build.display.id", "TP1A.220624.014.A525FXXU6DWB1" },
                new[] { adbPath, "shell", "setprop", "ro.build.version.incremental", "A525FXXU6DWB1" },
                new[] { adbPath, "shell", "setprop", "ro.build.date", "Mon Mar 20 12:00:00 KST 2023" },
                new[] { adbPath, "shell", "setprop", "ro.build.date.utc", "1679284800" },
                new[] { adbPath, "shell", "setprop", "ro.build.user", "dpi" },
                new[] { adbPath, "shell", "setprop", "ro.build.host", "SWDD6123" },
                // Propriedades de sistema (Last Island verifica)
                new[] { adbPath, "shell", "setprop", "ro.system.build.fingerprint", device.Fingerprint },
                new[] { adbPath, "shell", "setprop", "ro.vendor.build.fingerprint", device.Fingerprint },
                new[] { adbPath, "shell", "setprop", "ro.bootimage.build.fingerprint", device.Fingerprint },
                // Propriedades de rede (importante para jogos online)
                new[] { adbPath, "shell", "setprop", "net.dns1", "8.8.8.8" },
                new[] { adbPath, "shell", "setprop", "net.dns2", "8.8.4.4" },
                // Propriedades de localização (Last Island verifica)
                new[] { adbPath, "shell", "setprop", "ro.com.google.locationfeatures", "1" },
                new[] { adbPath, "shell", "setprop", "ro.com.google.gmsversion", "12_202301" },
                // Propriedades de sensor (jogos verificam sensores)
                new[] { adbPath, "shell", "setprop", "ro.hardware.sensors", "1" },
                new[] { adbPath, "shell", "setprop", "ro.hardware.gps", "1" },
                // Propriedades de memória (Last Island verifica)
                new[] { adbPath, "shell", "setprop", "ro.config.low_ram", "false" },
                new[] { adbPath, "shell", "setprop", "ro.config.ram_size", "8GB" },
                // Propriedades de GPU (importante para jogos)
                new[] { adbPath, "shell", "setprop", "ro.opengles.version", "196610" },
                new[] { adbPath, "shell", "setprop", "ro.gpu.rendering", "1" },
                // Remover mais flags de emulador
                new[] { adbPath, "shell", "setprop", "ro.bootloader", "unknown" },
                new[] { adbPath, "shell", "setprop", "ro.hardware.keystore", "software" },
                new[] { adbPath, "shell", "setprop", "ro.product.first_api_level", "30" },
                new[] { adbPath, "shell", "setprop", "ro.product.locale", "en-US" },
                new[] { adbPath, "shell", "setprop", "ro.product.locale.language", "en" },
                new[] { adbPath, "shell", "setprop", "ro.product.locale.region", "US" }
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
                    FileName = adbPath,
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
                            ApplyGameSpecificProperties(deviceKey);
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

        private void BtnCheckRoot_Click(object sender, EventArgs e)
        {
            Log("🔍 Verificando status do root...");
            
            Task.Run(() =>
            {
                bool rootAvailable = CheckRoot();
                
                Invoke(new Action(() =>
                {
                    if (rootAvailable)
                    {
                        Log("✅ Root está ATIVO! Bypass avançado disponível.");
                        MessageBox.Show(
                            "✅ Root está ATIVO!\n\n" +
                            "O bypass agora pode modificar arquivos do sistema permanentemente.\n\n" +
                            "Isso melhora significativamente a compatibilidade com jogos como Last Island Survival.",
                            "Root Ativo",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                    else
                    {
                        Log("⚠️ Root NÃO está disponível.");
                        var result = MessageBox.Show(
                            "⚠️ Root não está disponível no BlueStacks.\n\n" +
                            "Para ativar root:\n" +
                            "1. Abra BlueStacks\n" +
                            "2. Vá em Configurações > Avançado\n" +
                            "3. Ative 'Root' ou 'Enable Root'\n" +
                            "4. Reinicie o BlueStacks\n\n" +
                            "Deseja abrir o guia completo?",
                            "Root Não Disponível",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );
                        
                        if (result == DialogResult.Yes)
                        {
                            try
                            {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = "notepad.exe",
                                    Arguments = Path.Combine(Application.StartupPath, "COMO_ATIVAR_ROOT.md"),
                                    UseShellExecute = true
                                });
                            }
                            catch
                            {
                                Log("📖 Veja o arquivo COMO_ATIVAR_ROOT.md para instruções detalhadas");
                            }
                        }
                    }
                }));
            });
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
                        FileName = adbPath,
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
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public string Fingerprint { get; set; } = string.Empty;
    }
}
