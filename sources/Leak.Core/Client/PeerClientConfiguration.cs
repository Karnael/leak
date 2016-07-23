﻿using Leak.Core.Common;

namespace Leak.Core.Client
{
    public class PeerClientConfiguration
    {
        public PeerHash Peer { get; set; }

        public string Destination { get; set; }

        public PeerClientCallback Callback { get; set; }
    }
}