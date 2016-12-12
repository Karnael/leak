﻿using Leak.Common;
using Leak.Sockets;
using System;

namespace Leak.Networking
{
    /// <summary>
    /// Describes the network socket wrapper with built-in data buffering
    /// designed only to receive data from the remote endpoint.
    /// </summary>
    public class NetworkBuffer
    {
        private readonly NetworkPoolListener listener;
        private readonly NetworkDecryptor decryptor;

        private readonly TcpSocket socket;
        private readonly long identifier;
        private readonly byte[] data;

        private int offset;
        private int length;

        /// <summary>
        /// Creates a new instance of the network buffer relying on the already
        /// connected socket instance and configuration defining the buffer size
        /// and how the incoming data should be decrypted.
        /// </summary>
        /// <param name="listener">The listener who knows the pool.</param>
        /// <param name="socket">The already connected socket.</param>
        /// <param name="identifier">The unique connection identifier.</param>
        public NetworkBuffer(NetworkPoolListener listener, TcpSocket socket, long identifier)
        {
            this.listener = listener;
            this.socket = socket;
            this.identifier = identifier;

            data = new byte[40000];
            decryptor = NetworkDecryptor.Nothing;
        }

        /// <summary>
        /// Creates a new instance of the network buffer from the existing instance.
        /// The inner socket and the already downloaded and waiting data will be
        /// copied, but the caller can change the buffer size and decryption algorithm.
        /// </summary>
        /// <param name="buffer">The existing instance of the newtwork buffer.</param>
        /// <param name="decryptor">The new decryptor.</param>
        public NetworkBuffer(NetworkBuffer buffer, NetworkDecryptor decryptor)
        {
            this.decryptor = decryptor;

            listener = buffer.listener;
            socket = buffer.socket;
            identifier = buffer.identifier;
            data = buffer.data;
            length = buffer.length;
            offset = buffer.offset;

            Decrypt(offset, length);
        }

        /// <summary>
        /// Begins receiving data from the remote endpoint. If the buffer already
        /// contains data it will wait anyway for additional remote data. The handler
        /// will be notified in asynchronous way.
        /// </summary>
        /// <param name="handler">An instance of the incoming message handler.</param>
        public void Receive(NetworkIncomingMessageHandler handler)
        {
            if (listener.IsAvailable(identifier))
            {
                int receiveOffset;
                int receiveSize;

                if (offset + length >= data.Length)
                {
                    receiveOffset = offset + length - data.Length;
                    receiveSize = offset - (offset + length) % data.Length;
                }
                else
                {
                    receiveOffset = offset + length;
                    receiveSize = data.Length - offset - length;
                }

                socket.Receive(new TcpSocketBuffer(data, receiveOffset, receiveSize), context => OnReceived(context, handler));
            }
        }

        /// <summary>
        /// Begins receiving data first from the local buffer. If nothing is available
        /// from the remote endpoint. In both cases the caller will be notified in
        /// asynchronous way.
        /// </summary>
        /// <param name="handler">An instance of the incoming message handler.</param>
        public void ReceiveOrCallback(NetworkIncomingMessageHandler handler)
        {
            if (listener.IsAvailable(identifier))
            {
                if (length > 0)
                {
                    listener.Schedule(new NetworkPoolReceive(handler, new NetworkBufferMessage(this)));
                }
                else
                {
                    Receive(handler);
                }
            }
        }

        private void OnReceived(TcpSocketReceive context, NetworkIncomingMessageHandler handler)
        {
            listener.Schedule(new NetworkPoolDecrypt(this, handler, context.Count));
        }

        public void Process(NetworkIncomingMessageHandler handler, int count)
        {
            if (listener.IsAvailable(identifier))
            {
                if (count > 0)
                {
                    if (offset + length >= data.Length)
                    {
                        Decrypt(offset + length - data.Length, count);
                    }
                    else
                    {
                        Decrypt(offset + length, count);
                    }

                    length += count;
                    handler.OnMessage(new NetworkBufferMessage(this));
                }
                else
                {
                    listener.Disconnect(identifier);
                    handler.OnDisconnected();
                }
            }
        }

        public void Remove(int bytes)
        {
            if (bytes > length)
            {
                throw new InvalidOperationException();
            }

            offset = (offset + bytes) % data.Length;
            length = length - bytes;
        }

        private void Decrypt(int start, int count)
        {
            int min = Math.Min(count, data.Length - start);

            decryptor.Decrypt(data, start, min);
            decryptor.Decrypt(data, 0, count - min);
        }

        public NetworkBufferView View()
        {
            return new NetworkBufferView(data, length, offset);
        }
    }
}