using System;
using System.Drawing;
using System.Net;
using ArtNet.Sockets;
using ArtNet.Packets;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading;

namespace Sample
{
    class MainClass
    {
        public const uint MAX_SUPPORTED_UNIVERSES = 512;
        public static byte[,] s_dmxValues = new byte[MAX_SUPPORTED_UNIVERSES, MAX_SUPPORTED_UNIVERSES]; // rows = universes, columns = values
        public static Color[,] s_dmxColours = new Color[MAX_SUPPORTED_UNIVERSES, (int)Math.Ceiling((double)MAX_SUPPORTED_UNIVERSES / 3)]; // rows = universes, columns = colours (512 channels gives 171 colours)
        public const uint IMAGE_SEQUENCE_FPS = 60; // iterations per second
        public static int s_currentIndex = 0;
        public static bool s_writeImageSequence = true;
        public static Mutex s_mutex = new Mutex(); // a mutex to prevent concurrent read/write of the DMX data

        public static void Main(string[] args)
        {
            var artnet = new ArtNet.Sockets.ArtNetSocket();
            artnet.EnableBroadcast = true;

            Console.WriteLine(artnet.BroadcastAddress.ToString());
            artnet.Open(IPAddress.Parse("10.10.80.48"), IPAddress.Parse("255.255.255.0"));

            // Initialize DMX values and colours to zero
            for (int i = 0; i < MAX_SUPPORTED_UNIVERSES; i++)
            {
                for (int j = 0; j < MAX_SUPPORTED_UNIVERSES; j++)
                {
                    s_dmxValues[i, j] = 0;
                }
            }

            // Register a packet listener that will write the data to the array at the incoming frame rate
            artnet.NewPacket += artnet_NewPacket;

            if (s_writeImageSequence)
            {
                // Launch an image writer
                System.Timers.Timer timer = new System.Timers.Timer(1000 / IMAGE_SEQUENCE_FPS);
                timer.Elapsed += WriteImage;
                timer.Start();
            }

            Console.ReadLine();
        }

        private static void WriteImage(Object source, ElapsedEventArgs e)
        {
            s_mutex.WaitOne();
            // Generate an image with the current state of the DMX values
            DirectoryInfo di = new DirectoryInfo(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\sequence");
            if (!di.Exists)
            {
                di.Create();
            }
            string fileNameNoExt = "ImageSequence_" + s_currentIndex.ToString("D8");
            FileInfo fi = new FileInfo(di.FullName + "\\" + fileNameNoExt + ".png");
            int imgWidth = (int)Math.Ceiling((double)MAX_SUPPORTED_UNIVERSES / 3);
            int imgHeight = (int)MAX_SUPPORTED_UNIVERSES;

            var im = new Bitmap(imgWidth, imgHeight, PixelFormat.Format24bppRgb);
            for (int row = 0; row < s_dmxColours.GetLength(0); row++)
            {
                for (int col = 0; col < s_dmxColours.GetLength(1); col++)
                {
                    im.SetPixel(col, row, s_dmxColours[row, col]);
                }
            }
            im.Save(fi.FullName, ImageFormat.Png);

            s_currentIndex++;
            s_mutex.ReleaseMutex();
        }

        static void artnet_NewPacket(object sender, NewPacketEventArgs<ArtNetPacket> e)
        {
            if (e.Packet.OpCode == ArtNet.Enums.ArtNetOpCodes.Dmx)
            {
                var packet = e.Packet as ArtNet.Packets.ArtNetDmxPacket;
                Console.Clear();

                s_mutex.WaitOne();
                Console.WriteLine("New Packet of length " + packet.DmxData.Length.ToString() + " in universe " + packet.Universe.ToString());
                // We expect RGB values (1 byte/channel)
                for (var i = 0; i < packet.DmxData.Length / 3; i++)
                {
                    s_dmxValues[packet.Universe, i*3] = packet.DmxData[i];
                    s_dmxValues[packet.Universe, i*3 + 1] = packet.DmxData[i*3 + 1];
                    s_dmxValues[packet.Universe, i*3 + 2] = packet.DmxData[i*3 + 2];
                    Color current = Color.FromArgb(packet.DmxData[i*3], packet.DmxData[i*3 + 1], packet.DmxData[i*3 + 2]);
                    s_dmxColours[packet.Universe, i] = current;
                };
                s_mutex.ReleaseMutex();
            }
        }
    }
}
