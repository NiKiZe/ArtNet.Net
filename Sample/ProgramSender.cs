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
        public const uint SENDER_FPS = 44; // iterations per second
        public const uint SENDER_UNIVERSES = 512; // number of universes to send
        public const uint SENDER_LENGTH = 512; // number of bytes to send
        public static byte[] s_dmxData = new byte[511];
        public static ArtNetSocket s_artnet = new ArtNetSocket();

        public static void Main(string[] args)
        {
            s_artnet.EnableBroadcast = true;

            Console.WriteLine("Broadcast address: " + s_artnet.BroadcastAddress.ToString() + "\nAt FPS=" + SENDER_FPS + "\n");
            s_artnet.Open(IPAddress.Parse("10.10.80.48"), IPAddress.Parse("255.255.255.0"));

            Timer timer = new Timer(1000/SENDER_FPS);
            timer.Elapsed += SendArtNet;
            timer.Start();
            Console.ReadLine();
        }

        // We send RGB values (1 byte/channel) via Art-Net.
        private static void SendArtNet(Object source, ElapsedEventArgs e)
        {
            ArtNetDmxPacket toSend = new ArtNetDmxPacket();
            Random rnd = new Random();

            toSend.DmxData = new byte[SENDER_LENGTH];
            for (uint universe = 0; universe < SENDER_UNIVERSES; universe++)
            {
                toSend.Universe = (short)universe;
                for (uint i = 0; i < SENDER_LENGTH; i++)
                {
                    if (i % 3 == 0)
                    {
                        var phase = 2 * Math.PI * i / SENDER_LENGTH; // radians
                        uint redValue = (uint)(Math.Cos(DateTime.Now.Millisecond + phase) * 255.0); // add phase based on i index
                        toSend.DmxData[i] = (byte)redValue; //(byte)rnd.Next(0, 255);
                    }
                    else
                    {
                        toSend.DmxData[i] = 0;
                    }
                }
                s_artnet.Send(toSend);
                Console.WriteLine("Sending [" + toSend.DmxData.Length + "] Art-Net values data on universe [" + toSend.Universe + "]");
            }
        }
    }
}
