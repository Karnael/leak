﻿using Leak.Events;
using Leak.Retriever.Components;
using Leak.Tasks;

namespace Leak.Retriever.Tasks
{
    public class HandleBitfieldTask : LeakTask<RetrieverContext>
    {
        private readonly PeerChanged data;

        public HandleBitfieldTask(PeerChanged data)
        {
            this.data = data;
        }

        public void Execute(RetrieverContext context)
        {
            context.Dependencies.Omnibus.Handle(data);
            //context.Collector.SendBitfield(peer, new Bitfield(bitfield.Length));
            //context.Collector.SendLocalInterested(peer);
        }
    }
}