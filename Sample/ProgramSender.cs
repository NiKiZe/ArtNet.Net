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

            toSend.DmxData = new byte[] { (byte)(rnd.Next(0, 255)), (byte)(rnd.Next(0, 255)), (byte)(rnd.Next(0, 255)) };
            artnet.Send(toSend);
            Console.WriteLine("Sending Art-Net data: " + toSend.DmxData[0] + " " + toSend.DmxData[1] + " " + toSend.DmxData[2]);
        }
    }
}
