﻿using Leak.Core.Common;
using Leak.Core.Net;
using Leak.Core.Network;

namespace Leak.Core.Negotiator
{
    public class HandshakeNegotiatorPassive
    {
        private readonly HandshakeNegotiatorPassiveContext context;
        private readonly HandshakeConnection connection;
        private readonly PeerCredentials credentials;
        private readonly HandshakeKeyContainer keys;

        private FileHash found;

        public HandshakeNegotiatorPassive(NetworkConnection connection, HandshakeNegotiatorPassiveContext context)
        {
            this.context = context;
            this.connection = new HandshakeConnection(connection, context);

            this.credentials = PeerCryptography.Generate();
            this.keys = new HandshakeKeyContainer();
        }

        public void Execute()
        {
            connection.Receive(HandleKeyExchangeMessage, 96);
        }

        private void HandleKeyExchangeMessage(NetworkIncomingMessage message)
        {
            HandshakeKeyExchange exchange = new HandshakeKeyExchange(message);

            keys.Secret = PeerCryptography.Secret(credentials, exchange.Key);
            message.Acknowledge(96);

            connection.Send(new HandshakeKeyExchangeMessage(credentials));
            connection.Receive(SynchronizeCryptoHashMessage, VerifyCryptoHashMessage);
        }

        private bool VerifyCryptoHashMessage(NetworkIncomingMessage message)
        {
            byte[] synchronize = HandshakeCryptoHashMessage.GetHash(keys.Secret);
            int offset = Bytes.Find(message.ToBytes(), synchronize);

            return offset >= 0 && message.Length >= offset + 40;
        }

        private void SynchronizeCryptoHashMessage(NetworkIncomingMessage message)
        {
            byte[] synchronize = HandshakeCryptoHashMessage.GetHash(keys.Secret);
            int offset = Bytes.Find(message.ToBytes(), synchronize);

            byte[] bytes = message.ToBytes(offset + 20, 20);
            HandshakeMatch match = new HandshakeMatch(keys.Secret, bytes);

            found = context.Hashes.Find(match);

            if (found == null)
            {
                context.OnRejected(new HandshakeRejection(match));
                connection.Close();

                return;
            }

            keys.Local = new HandshakeKey(HandshakeKeyOwnership.Receiver, keys.Secret, found);
            keys.Remote = new HandshakeKey(HandshakeKeyOwnership.Initiator, keys.Secret, found);

            message.Acknowledge(offset + 40);
            connection.Receive(MeasureCryptoPayloadMessage, HandshakeCryptoPayload.MinimumSize);
        }

        private void MeasureCryptoPayloadMessage(NetworkIncomingMessage message)
        {
            HandshakeKey decryptor = keys.Remote.Clone();
            NetworkIncomingMessage decrypted = decryptor.Decrypt(message);

            int size = HandshakeCryptoPayload.GetSize(decrypted);
            connection.Receive(HandleCryptoPayloadMessage, size);
        }

        private void HandleCryptoPayloadMessage(NetworkIncomingMessage message)
        {
            HandshakeKey decryptor = keys.Remote.Clone();
            NetworkIncomingMessage decrypted = decryptor.Decrypt(message);
            int size = HandshakeCryptoPayload.GetSize(decrypted);

            message.Acknowledge(size);
            keys.Remote.Acknowledge(size);

            connection.Receive(MeasureCryptoMessage, HandshakeCryptoMessage.MinimumSize);
        }

        private void MeasureCryptoMessage(NetworkIncomingMessage message)
        {
            HandshakeKey decryptor = keys.Remote.Clone();
            NetworkIncomingMessage decrypted = decryptor.Decrypt(message);

            int size = HandshakeCryptoMessage.GetSize(decrypted);
            connection.Receive(HandleCryptoMessage, size);
        }

        private void HandleCryptoMessage(NetworkIncomingMessage message)
        {
            HandshakeKey decryptor = keys.Remote.Clone();
            NetworkIncomingMessage decrypted = decryptor.Decrypt(message);
            int size = HandshakeCryptoMessage.GetSize(decrypted);

            message.Acknowledge(size);
            keys.Remote.Acknowledge(size);

            connection.Send(new HandshakeCryptoPayloadMessage(), keys.Local);
            connection.Receive(MeasureHandshakeMessage, HandshakeMessage.MinSize);
        }

        private void MeasureHandshakeMessage(NetworkIncomingMessage message)
        {
            HandshakeKey decryptor = keys.Remote.Clone();
            NetworkIncomingMessage decrypted = decryptor.Decrypt(message);

            int size = HandshakeMessage.GetSize(decrypted);
            connection.Receive(HandleHandshakeMessage, size);
        }

        private void HandleHandshakeMessage(NetworkIncomingMessage message)
        {
            HandshakeKey decryptor = keys.Remote.Clone();
            NetworkIncomingMessage decrypted = decryptor.Decrypt(message);

            int size = HandshakeMessage.GetSize(decrypted);
            Handshake handshake = new Handshake(context.Peer, context.Peer, found);

            message.Acknowledge(size);
            keys.Remote.Acknowledge(size);

            connection.Send(new HandshakeMessage(context.Peer, found, context.Options), keys.Local);
            context.OnHandshake(connection.StartEncryption(keys), handshake);
        }
    }
}