﻿using Leak.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Leak.Core.Tracker
{
    public class TrackerClientToUdp : TrackerClient
    {
        private readonly string host;
        private readonly int port;

        public TrackerClientToUdp(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public TrackerAnnounce Announce(Action<TrackerAnnounceConfiguration> configurer)
        {
            TrackerAnnounceConfiguration configuration = Configure(configurer);

            int received;

            byte[] connect = new byte[16];
            byte[] request = new byte[98];
            byte[] response = new byte[4000];
            byte[] transaction = Bytes.Random(4);

            Array.Copy(Bytes.Parse("0000041727101980"), connect, 8);

            Array.Copy(Bytes.Parse("00000001"), 0, request, 8, 4);
            Array.Copy(transaction, 0, request, 12, 4);
            Array.Copy(configuration.Hash.ToBytes(), 0, request, 16, 20);
            Array.Copy(configuration.Peer.ToBytes(), 0, request, 36, 20);

            Array.Copy(Bytes.Parse("0000000000010000"), 0, request, 64, 8);
            Array.Copy(Bytes.Parse("00000001"), 0, request, 80, 4);
            Array.Copy(Bytes.Parse("ffff"), 0, request, 92, 2);
            Array.Copy(Bytes.Parse("1f90"), 0, request, 96, 2);

            using (Socket socket = new Socket(SocketType.Dgram, ProtocolType.Udp))
            {
                socket.SendTimeout = 15000;
                socket.ReceiveTimeout = 15000;
                socket.Connect(host, port);

                socket.Send(connect);
                received = socket.Receive(response);

                Array.Copy(response, 8, request, 0, 8);

                socket.Send(request);
                received = socket.Receive(response);
            }

            return new TrackerAnnounce(FindPeers(response, received));
        }

        private static TrackerPeer[] FindPeers(byte[] data, int count)
        {
            List<TrackerPeer> peers = new List<TrackerPeer>();

            for (int i = 20; i < count; i += 6)
            {
                string host = String.Join(".", data.Skip(i).Take(4));
                int port = 256 * data[i + 4] + data[i + 5];

                peers.Add(new TrackerPeer(host, port));
            }

            return peers.ToArray();
        }

        private static TrackerAnnounceConfiguration Configure(Action<TrackerAnnounceConfiguration> configurer)
        {
            TrackerAnnounceConfiguration configuration = new TrackerAnnounceConfiguration
            {
                Peer = PeerHash.Random()
            };

            configurer.Invoke(configuration);
            return configuration;
        }
    }
}