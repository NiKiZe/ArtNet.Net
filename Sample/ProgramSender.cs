using System;
using System.Net;
using ArtNet.Sockets;
using ArtNet.Packets;
using System.Threading.Tasks;
using System.Timers;

namespace Sample
{
    class MainClass
    {
        public const uint SENDER_FPS = 40; // iterations per second
        public const uint SENDER_LENGTH = 512; // number of bytes to send
        public static byte[] dmxData = new byte[511];
        public static ArtNetSocket artnet = new ArtNetSocket();

        public static void Main(string[] args)
        {
            artnet.EnableBroadcast = true;

            Console.WriteLine("Broadcast address: " + artnet.BroadcastAddress.ToString() + "\nAt FPS=" + SENDER_FPS + "\n");
            artnet.Open(IPAddress.Parse("10.10.80.48"), IPAddress.Parse("255.255.255.0"));

            Timer timer = new Timer(1000/SENDER_FPS);
            timer.Elapsed += SendArtNet;
            timer.Start();
            Console.ReadLine();
        }

        private static void SendArtNet(Object source, ElapsedEventArgs e)
        {
            ArtNetDmxPacket toSend = new ArtNetDmxPacket();
            Random rnd = new Random();
            toSend.Universe = 0; //(short)rnd.Next(0, 500);

            toSend.DmxData = new byte[SENDER_LENGTH];
            for (int i = 0; i < SENDER_LENGTH; i++ )
            {
                toSend.DmxData[i] = (byte)rnd.Next(0, 255);
            }
            artnet.Send(toSend);
            Console.WriteLine("Sending [" + toSend.DmxData.Length + "] values Art-Net data on universe [" + toSend.Universe + "]");
        }
    }
}
