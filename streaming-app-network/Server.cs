using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing.Imaging;

namespace streaming_app_network
{
    public partial class Server : Form
    {
        private UdpClient udpClient;
        private IPEndPoint broadcastEndpoint;
        private Thread streamingThread;
        private bool isStreaming = false;

        private VideoCaptureDevice videoSource;
        private FilterInfoCollection videoDevices;
        public Server()
        {
            InitializeComponent();
            InitializeCamera();
        }
        private void InitializeCamera()
        {
            // Lấy danh sách các thiết bị video (camera)
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("No camera devices found!");
                return;
            }

            // Chọn camera đầu tiên (hoặc bạn có thể thêm giao diện chọn camera)
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

            // Gắn sự kiện xử lý khi có frame mới
            videoSource.NewFrame += VideoSource_NewFrame;
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // Sao chép frame để tránh xung đột
                Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

                // Hiển thị frame trên PictureBox (luồng UI)
                if (stream_screen.InvokeRequired)
                {
                    stream_screen.Invoke(new Action(() =>
                    {
                        stream_screen.Image?.Dispose(); // Giải phóng ảnh cũ để tránh rò rỉ bộ nhớ
                        stream_screen.Image = (Bitmap)frame.Clone();
                    }));
                }
                else
                {
                    stream_screen.Image?.Dispose(); // Giải phóng ảnh cũ để tránh rò rỉ bộ nhớ
                    stream_screen.Image = (Bitmap)frame.Clone();
                }

                // Truyền frame qua UDP
                if (isStreaming)
                {
                    byte[] videoData = ConvertFrameToBytes(frame);
                    SendFrameInChunks(videoData, 8000);
                }

                // Giải phóng frame sau khi xử lý
                frame.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VideoSource_NewFrame: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isStreaming = true;

            //Cấu hình UDP broadcast
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, 12345);

            //Gửi thông báo server online
            SendBroadcast("Server is online");

            // Bắt đầu stream video
            if (videoSource != null)
            {
                videoSource.Start(); // Bắt đầu lấy dữ liệu từ camera
            }

            // Bắt đầu ghi âm
            var audioCapture = new WaveInEvent
            {
                WaveFormat = new WaveFormat(44100, 16, 1)
            };

            audioCapture.DataAvailable += (s, a) =>
            {
                if (isStreaming)
                {
                    int chunkSize = 8000; //chia nhỏ gói âm thanh
                    for (int i = 0; i < a.BytesRecorded; i += chunkSize)
                    {
                        int size = Math.Min(chunkSize, a.BytesRecorded - i);
                        byte[] chunk = new byte[size];
                        Array.Copy(a.Buffer, i, chunk, 0, size);
                        try
                        {
                            udpClient.Send(chunk, chunk.Length, broadcastEndpoint);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending audio: {ex.Message}");
                        }
                    }
                }
            };
            audioCapture.StartRecording();
        }

        private void SendBroadcast(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, broadcastEndpoint);
        }

        private byte[] ConvertFrameToBytes(Bitmap frame)
        {
            using (var memoryStream = new MemoryStream())
            {
                var encoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                encoderParameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality, 50L // Giảm chất lượng xuống 50%
                );

                var jpegCodec = ImageCodecInfo.GetImageEncoders()
                    .First(codec => codec.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);

                frame.Save(memoryStream, jpegCodec, encoderParameters);
                return memoryStream.ToArray();
            }
        }
        private void SendFrameInChunks(byte[] frameData, int chunkSize)
        {
            int totalChunks = (int)Math.Ceiling((double)frameData.Length / chunkSize);
            for (int i = 0; i < totalChunks; i++)
            {
                int offset = i * chunkSize;
                int size = Math.Min(chunkSize, frameData.Length - offset);
                byte[] chunk = new byte[size + 4];

                // Gắn chỉ số gói vào đầu gói (4 byte)
                BitConverter.GetBytes(i).CopyTo(chunk, 0);
                Array.Copy(frameData, offset, chunk, 4, size);

                // Gửi gói
                udpClient.Send(chunk, chunk.Length, broadcastEndpoint);

                Thread.Sleep(50);
            }

            // Gửi gói đặc biệt để báo kết thúc (index = -1)
            byte[] endChunk = BitConverter.GetBytes(-1);
            udpClient.Send(endChunk, endChunk.Length, broadcastEndpoint);
        }

    }
}
