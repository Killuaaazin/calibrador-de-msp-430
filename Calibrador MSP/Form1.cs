using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Forms;

namespace Embedded__MSP430
{
    public partial class Form1 : Form
    {
        private string serialBuffer = string.Empty;
        private readonly System.Windows.Forms.Timer portsRefreshTimer;

        public Form1()
        {
            InitializeComponent();
            Baud_comboBox.Text = "2400";

            portsRefreshTimer = new System.Windows.Forms.Timer();
            portsRefreshTimer.Interval = 1000;
            portsRefreshTimer.Tick += portsRefreshTimer_Tick;
            portsRefreshTimer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AtualizarPortasSeriais();
            Connect_bt.Enabled = true;
            Disconnect_bt.Enabled = false;
        }

        private void portsRefreshTimer_Tick(object sender, EventArgs e)
        {
            AtualizarPortasSeriais();
        }

        private void AtualizarPortasSeriais()
        {
            try
            {
                string selectedPort = Ports_comboBox.Text;
                string[] ports = SerialPort.GetPortNames();

                bool changed = ports.Length != Ports_comboBox.Items.Count;
                if (!changed)
                {
                    for (int i = 0; i < ports.Length; i++)
                    {
                        if (!Ports_comboBox.Items.Contains(ports[i]))
                        {
                            changed = true;
                            break;
                        }
                    }
                }

                if (!changed) return;

                Ports_comboBox.BeginUpdate();
                try
                {
                    Ports_comboBox.Items.Clear();
                    Ports_comboBox.Items.AddRange(ports);

                    if (Array.IndexOf(ports, selectedPort) >= 0)
                        Ports_comboBox.Text = selectedPort;
                    else if (ports.Length > 0 && string.IsNullOrEmpty(selectedPort))
                        Ports_comboBox.SelectedIndex = 0;
                    else if (Array.IndexOf(ports, selectedPort) < 0)
                        Ports_comboBox.SelectedIndex = -1;
                }
                finally
                {
                    Ports_comboBox.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao atualizar portas seriais: " + ex.Message);
            }
        }

        private void Connect_bt_Click(object sender, EventArgs e)
        {
            if (ConnectPort())
            {
                Connect_bt.Enabled = false;
                Disconnect_bt.Enabled = true;
                Ports_comboBox.Enabled = false;
                Baud_comboBox.Enabled = false;
                serialPort.DataReceived -= serialPort_DataReceived;
                serialPort.DataReceived += serialPort_DataReceived;
            }
        }

        private void Disconnect_bt_Click(object sender, EventArgs e)
        {
            serialPort.Close();
            Ports_comboBox.Enabled = true;
            Baud_comboBox.Enabled = true;
            Connect_bt.Enabled = true;
            Disconnect_bt.Enabled = false;
        }

        private bool ConnectPort()
        {
            try
            {
                if (Ports_comboBox.Text != "" && Baud_comboBox.Text != "")
                {
                    serialPort.PortName = Ports_comboBox.Text;
                    serialPort.BaudRate = int.Parse(Baud_comboBox.Text);
                    serialPort.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Lê dados recebidos e tenta extrair pares de valores de calibração
            try
            {
                string data = serialPort.ReadExisting();
                if (string.IsNullOrEmpty(data)) return;

                serialBuffer += data;

                // Encontra todos os inteiros no buffer
                var matches = Regex.Matches(serialBuffer, "-?\\d+");
                if (matches.Count >= 2)
                {
                    int lastIdx = matches.Count - 1;
                    int secondLastIdx = matches.Count - 2;
                    int val1, val2;
                    if (int.TryParse(matches[secondLastIdx].Value, out val1) && int.TryParse(matches[lastIdx].Value, out val2))
                    {
                        int removeUpTo = matches[lastIdx].Index + matches[lastIdx].Length;
                        if (removeUpTo <= serialBuffer.Length)
                            serialBuffer = serialBuffer.Substring(removeUpTo);

                        this.BeginInvoke(new Action(() =>
                        {
                            calib1TextBox.Text = val1.ToString();
                            calib2TextBox.Text = val2.ToString();
                        }));
                    }
                }
            }
            catch
            {
                // Ignora erros leves
            }
        }

        
        public async void calibrarBtn_Click(object sender, EventArgs e)
        {
            if (serialPort == null || !serialPort.IsOpen) return;

            calibrarBtn.Enabled = false;
            serialPort.DataReceived -= serialPort_DataReceived;

            // Limpa os buffers para garantir que não há lixo de calibrações anteriores
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();

            decimal freqMHz;
            string frequencyText = Frequency_comboBox.Text.Trim().Replace("MHz", "").Trim();
            if (!decimal.TryParse(frequencyText, NumberStyles.AllowDecimalPoint,
                CultureInfo.CurrentCulture, out freqMHz) &&
                !decimal.TryParse(frequencyText.Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out freqMHz))
            {
                MessageBox.Show("Informe uma frequência válida entre 1 e 16 MHz.", "Frequência inválida",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                calibrarBtn.Enabled = true;
                serialPort.DataReceived += serialPort_DataReceived;
                return;
            }

            if (freqMHz < 1m || freqMHz > 16m)
            {
                MessageBox.Show("A frequência deve estar entre 1 e 16 MHz.", "Frequência inválida",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                calibrarBtn.Enabled = true;
                serialPort.DataReceived += serialPort_DataReceived;
                return;
            }

            int baudRate = serialPort.BaudRate > 0 ? serialPort.BaudRate : 2400;

            // CORREÇÃO: Adicionado o operador de multiplicação (*) que faltava no seu código
            decimal freqHz = freqMHz * 1000000m;

            int reload = (int)(freqHz / baudRate);
            if (reload < 1) reload = 1;

            byte byte_alto = (byte)((reload >> 8) & 0xFF);
            byte byte_baixo = (byte)(reload & 0xFF);
            byte[] pulso = new byte[] { 0x55 };

            try
            {
                // Dispara ambos os pinos para garantir que um deles faz o reset
                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;

                await Task.Delay(100);  // Aumentado para 100ms para o hardware "sentir" bem o pulso

                serialPort.DtrEnable = false;
                serialPort.RtsEnable = false;   

                // Aguarda o arranque do chip
                await Task.Delay(250);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao enviar comando de reset: " + ex.Message);
                calibrarBtn.Enabled = true;
                serialPort.DataReceived += serialPort_DataReceived;
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                   // ETAPA 1: Aguardar o 'R' do MSP430 confirmando que o reset foi bem sucedido
            bool msp_pronto = false;
            int timeout = 0;
            
            while (!msp_pronto && timeout < 200)
            {
                // CORREÇÃO: É OBRIGATÓRIO enviar o pulso 0x55 aqui. 
                // O MSP430 está preso no Set_DCO(416) a usar o TimerA para ler 
                // estes pulsos e calibrar os 1 MHz iniciais!
                serialPort.Write(pulso, 0, 1); 
                Thread.Sleep(50);
                
                if (serialPort.BytesToRead > 0)
                {
                    if (serialPort.ReadExisting().Contains("R"))
                        msp_pronto = true;
                }
                timeout++;
            }
            if (!msp_pronto) throw new Exception("O MSP430 não respondeu ao reset/pré-calibração.");

                    // ETAPA 2: Envio da frequência alvo com chave anti-lixo (0xAA)
                    byte[] config_msg = new byte[] { 0xAA, byte_alto, byte_baixo };
                    serialPort.Write(config_msg, 0, 3);
                    Thread.Sleep(100);

                    // ETAPA 3: Calibração Final e Leitura dos Registros de Retorno
                    bool calibrado = false;
                    timeout = 0;
                    byte val1Bytes = 0;
                    byte val2Bytes = 0;

                    // Aqui enviamos o 0x55 repetidamente para servir de referência ao Temporizador do MSP
                    while (!calibrado && timeout < 1000)
                    {
                        serialPort.Write(pulso, 0, 1);
                        Thread.Sleep(1);

                        if (serialPort.BytesToRead >= 3)
                        {
                            byte resp1 = (byte)serialPort.ReadByte();

                            // Verifica se o pacote está alinhado com a chave 'q' enviada pelo MSP
                            if (resp1 == 'q')
                            {
                                byte resp2 = (byte)serialPort.ReadByte(); // DCOCTL calibrado
                                byte resp3 = (byte)serialPort.ReadByte(); // BCSCTL1 calibrado

                                val1Bytes = resp2;
                                val2Bytes = resp3;
                                calibrado = true;
                            }
                        }
                        timeout++;
                    }

                    this.Invoke(new Action(() =>
                    {
                        if (calibrado)
                        {
                            calib1TextBox.Text = val1Bytes.ToString();
                            calib2TextBox.Text = val2Bytes.ToString();
                            MessageBox.Show("Calibração concluída com sucesso!");
                        }
                        else
                        {
                            MessageBox.Show("Falha na calibração final. Tempo esgotado.");
                        }
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() => MessageBox.Show("Erro: " + ex.Message)));
                }
                finally
                {
                    this.Invoke(new Action(() =>
                    {
                        // Reativa a receção normal da UART e liberta o botão
                        serialPort.DataReceived += serialPort_DataReceived;
                        calibrarBtn.Enabled = true;
                    }));
                }
            });
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (portsRefreshTimer != null)
            {
                portsRefreshTimer.Stop();
                portsRefreshTimer.Dispose();
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //Pega a porta do ComboBox para funcionar mesmo se o app não estiver "Conectado"
            string comPort = Ports_comboBox.Text;
            if (string.IsNullOrEmpty(comPort))
            {
                MessageBox.Show("Selecione a porta COM primeiro!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //Liberação da porta serial
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.DataReceived -= serialPort_DataReceived;
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();
                serialPort.DtrEnable = false;
                serialPort.RtsEnable = false;
                serialPort.Close();
            }

            // Aguarda de forma assíncrona para o Windows libertar o driver USB 
            await Task.Delay(800);

            try
            {
                string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                string bslPath = System.IO.Path.Combine(appFolder, "BSLDEMO-2.01c.exe");
                string txtPath = System.IO.Path.Combine(appFolder, "firmware.txt");

                if (!System.IO.File.Exists(bslPath))
                {
                    MessageBox.Show("BSLDEMO não encontrado em:\n" + bslPath, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!System.IO.File.Exists(txtPath))
                {
                    MessageBox.Show("Arquivo não encontrado!\nRenomeie seu código compilado para 'firmware.txt' e coloque na pasta do programa.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string arguments = $"-c{comPort} -m1 -ij -s2 +epr \"{txtPath}\"";

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = bslPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                // Lê a saída sem travar o aplicativo
                string output = await process.StandardOutput.ReadToEndAsync();

                // Aguarda o gravador terminar em background
                await Task.Run(() => process.WaitForExit());

                // Verifica o sucesso
                if (process.ExitCode == 0)
                {
                    System.Media.SystemSounds.Exclamation.Play();
                    MessageBox.Show("Firmware gravado com sucesso!\nO MSP430 já vai reiniciar.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    System.Media.SystemSounds.Hand.Play();
                    MessageBox.Show("Falha na gravação. O chip respondeu?\n\nLOG:\n" + output, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro de execução: " + ex.Message, "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reconecta automaticamente caso o aplicativo estivesse conectado antes de gravar
                if (Connect_bt.Enabled == false)
                {
                    try
                    {
                        serialPort.Open();
                        serialPort.DataReceived += serialPort_DataReceived;
                    }
                    catch { }
                }
            }
        }

        private async void resetBtn_Click(object sender, EventArgs e)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                MessageBox.Show("Conecte-se à porta COM antes de resetar o MSP430.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            resetBtn.Enabled = false;
            serialPort.DataReceived -= serialPort_DataReceived;

            try
            {
                serialBuffer = string.Empty;
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();
                calib1TextBox.Clear();
                calib2TextBox.Clear();

                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;
                await Task.Delay(100);
                serialPort.DtrEnable = false;
                serialPort.RtsEnable = false;

                await Task.Delay(250);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao resetar o MSP430: " + ex.Message, "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                serialPort.DataReceived += serialPort_DataReceived;
                resetBtn.Enabled = true;
            }
        }
    }
}