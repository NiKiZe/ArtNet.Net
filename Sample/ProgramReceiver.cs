using System;
using System.Drawing;
using System.Net;
using ArtNet.Sockets;
using ArtNet.Packets;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Sample
{
    class MainClass
    {
        public static byte[] dmxData = new byte[511];
        public const uint MAX_SUPPORTED_UNIVERSES = 512;
        public static int s_currentIndex = 0;

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
                //Console.Clear();

                if (packet.DmxData != dmxData)
                {
                    // Data has changed
                    Console.WriteLine("New Packet of length " + packet.DmxData.Length.ToString() + " in universe " + packet.Universe.ToString());
                    for (var i = 0; i < packet.DmxData.Length; i++)
                    {
                        if (packet.DmxData[i] != 0)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " " + i + " = " + packet.DmxData[i]);
                        }
                    };
                    dmxData = packet.DmxData;

                    // Initialize the byte array with zeros
                    var byteArray = new byte[MAX_SUPPORTED_UNIVERSES * 512];
                    for (int i = 0; i < byteArray.Length; i++)
                    {
                        byteArray[i] = 0;
                    }
                    // Populate with incoming dmx Data
                    for (int i = 0; i < packet.DmxData.Length; i++)
                    {
                        byteArray[i] = packet.DmxData[i];
                    }

                    // Generate an image
                    DirectoryInfo di = new DirectoryInfo(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\sequence");
                    if (!di.Exists)
                    {
                        di.Create();
                    }
                    string fileNameNoExt = "ImageSequence_" + s_currentIndex.ToString("D8");
                    FileInfo fi = new FileInfo(di.FullName + "\\" + fileNameNoExt + ".png");
                    var im = new Bitmap(512, (int)MAX_SUPPORTED_UNIVERSES, 512, PixelFormat.Format8bppIndexed, Marshal.UnsafeAddrOfPinnedArrayElement(byteArray, 0));
                    im.Save(fi.FullName, ImageFormat.Png);

                    s_currentIndex++;
                }
            }
        }
    }
}
