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

            // Register a packet listener
            artnet.NewPacket += artnet_NewPacket;
            Console.ReadLine();
        }

        static void artnet_NewPacket(object sender, NewPacketEventArgs<ArtNetPacket> e)
        {
            if (e.Packet.OpCode == ArtNet.Enums.ArtNetOpCodes.Dmx)
            {
                var packet = e.Packet as ArtNet.Packets.ArtNetDmxPacket;
                Console.Clear();

                if (packet.DmxData != dmxData)
                {
                    // Data has changed
                    Console.WriteLine("New Packet of length " + packet.DmxData.Length.ToString() + " in universe " + packet.Universe.ToString());
                    for (var i = 0; i < packet.DmxData.Length; i++)
                    {
                        if (packet.DmxData[i] != 0)
                        {
                            Console.WriteLine(i + " = " + packet.DmxData[i]);
                        }

                    };
                    dmxData = packet.DmxData;
                }
            }
        }
    }
}
