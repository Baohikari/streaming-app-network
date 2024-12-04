using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using System.Threading;
using System.IO;

namespace streaming_app_network
{
    public partial class Client : Form
    {
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private Thread receiveThread;
        private bool isReceiving = false;

        private BufferedWaveProvider waveProvider;
        private WaveOutEvent waveOut;
        public Client()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Khởi tạo UDP client để nhận dữ liệu
            udpClient = new UdpClient(12345);
            serverEndPoint = new IPEndPoint(IPAddress.Any, 0);

            //Lắng nghe broadcast từ server
            ListenForBroadcast();

            //Bắt đầu nhận dữ liệu hình ảnh và âm thanh
            isReceiving = true;
            receiveThread = new Thread(ReceiveMedia);
            receiveThread.Start();
        }

        private void ListenForBroadcast()
        {
            Task.Run(() =>
            {
                while (isReceiving)
                {
                    try
                    {
                        byte[] data = udpClient.Receive(ref serverEndPoint);
                        string message = Encoding.UTF8.GetString(data);

                        if (message == "Server is online")
                        {
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show("Server is online. Starting stream...");
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in ListenForBroadcast: {ex.Message}");
                    }
                }
            });
        }
        private void ReceiveMedia()
        {
            try
            {
                // Khởi tạo NAudio để phát âm thanh
                waveProvider = new BufferedWaveProvider(new WaveFormat(44100, 16, 1));
                waveOut = new WaveOutEvent();
                waveOut.Init(waveProvider);
                waveOut.Play();

                // Bắt đầu nhận dữ liệu bất đồng bộ
                udpClient.BeginReceive(ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReceiveMedia: {ex.Message}");
            }
        }

        private bool IsImageData(byte[] data)
        {
            // Kiểm tra dữ liệu có phải là ảnh (JPEG header: FF D8)
            return data.Length > 2 && data[0] == 0xFF && data[1] == 0xD8;
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                byte[] receivedData = udpClient.EndReceive(ar, ref serverEndPoint);

                // Kiểm tra nếu có dữ liệu mới
                if (receivedData == null || receivedData.Length == 0)
                    return;

                // Phân loại dữ liệu hình ảnh và âm thanh
                if (IsImageData(receivedData))
                {
                    // Xử lý và hiển thị hình ảnh
                    using (var memoryStream = new MemoryStream(receivedData))
                    {
                        try
                        {
                            Bitmap image = new Bitmap(memoryStream);
                            // Kiểm tra và cập nhật giao diện
                            if (watch_screen.InvokeRequired)
                            {
                                watch_screen.Invoke(new Action(() =>
                                {
                                    watch_screen.Image?.Dispose();
                                    watch_screen.Image = image;
                                }));
                            }
                            else
                            {
                                watch_screen.Image?.Dispose();
                                watch_screen.Image = image;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing image: {ex.Message}");
                        }
                    }
                }
                else
                {
                    // Xử lý và phát âm thanh
                    if (waveProvider.BufferLength - waveProvider.BufferedBytes > receivedData.Length)
                    {
                        waveProvider.AddSamples(receivedData, 0, receivedData.Length);
                    }
                    else
                    {
                        // Nếu buffer đầy, chờ và thử lại sau
                        Console.WriteLine("Audio buffer is full. Dropping audio data.");
                    }
                }

                // Tiếp tục nhận dữ liệu
                udpClient.BeginReceive(ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReceiveCallback: {ex.Message}");
            }
        }


        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            isReceiving = false;
            udpClient?.Close();

            waveOut?.Stop();
            waveOut?.Dispose();
        }
    }
}
