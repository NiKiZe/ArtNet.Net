using System;
using System.Net;
using ArtNet.Sockets;
using ArtNet.Packets;

namespace Sample
{
    class MainClass
    {
        public static byte[] dmxData = new byte[511];

        public static void Main(string[] args)
        {
            var artnet = new ArtNet.Sockets.ArtNetSocket();
            artnet.EnableBroadcast = true;

            Console.WriteLine(artnet.BroadcastAddress.ToString());
            artnet.Open(IPAddress.Parse("10.10.80.48"), IPAddress.Parse("255.255.255.0"));
            
            // Create a new DMX packet
            ArtNetDmxPacket toSend = new ArtNetDmxPacket();
            toSend.DmxData = new byte[] { 127, 0, 63 };
            artnet.Send(toSend);
            
            Console.ReadLine();
        }
    }
}
