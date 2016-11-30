﻿using FluentAssertions;
using Leak.Core.Common;
using Leak.Core.Events;
using Leak.Core.Glue;
using Leak.Core.Metadata;
using Leak.Core.Retriever;
using Leak.Core.Tests.Core;
using NUnit.Framework;

namespace Leak.Core.Tests.Components
{
    public class RetrieverTests
    {
        private RetrieverHooks hooks;
        private RetrieverConfiguration configuration;

        private Fixture fixture;
        private Environment environemnt;

        [OneTimeSetUp]
        public void OneSetUp()
        {
            fixture = new Fixture();
        }

        [OneTimeTearDown]
        public void OneTearDown()
        {
            fixture.Dispose();
            fixture = null;
        }

        [SetUp]
        public void SetUp()
        {
            environemnt = new Environment(fixture.Debian.Metadata);

            configuration = new RetrieverConfiguration();
            hooks = new RetrieverHooks();
        }

        [TearDown]
        public void TearDown()
        {
            environemnt.Dispose();
            environemnt = null;
        }

        private RetrieverService NewRetrieverService()
        {
            Metainfo metainfo = fixture.Debian.Metadata.Metainfo;
            Bitfield bitfield = new Bitfield(metainfo.Pieces.Length);
            GlueService glue = environemnt.Peers.Bob.Entry.Glue;

            return new RetrieverService(metainfo, environemnt.Destination, bitfield, glue, environemnt.Files, environemnt.Pipeline, hooks, configuration);
        }

        [Test]
        public void ShouldTriggerDataChanged()
        {
            Trigger handler = Trigger.Bind(ref hooks.OnDataChanged, data =>
            {
                data.Hash.Should().Be(fixture.Debian.Metadata.Hash);
                data.Completed.Should().Be(1);
            });

            using (RetrieverService retriever = NewRetrieverService())
            {
                retriever.Start();
                retriever.HandleBlockReceived(fixture.Debian.Events.BlockReceived[0]);

                handler.Wait().Should().BeTrue();
            }
        }

        [Test]
        public void ShouldTriggerDataHandled()
        {
            Trigger handler = Trigger.Bind(ref hooks.OnBlockHandled, data =>
            {
                data.Hash.Should().Be(fixture.Debian.Metadata.Hash);
                data.Peer.Should().NotBeNull();
                data.Piece.Should().Be(1);
                data.Block.Should().Be(0);
                data.Size.Should().Be(16384);
            });

            using (RetrieverService retriever = NewRetrieverService())
            {
                retriever.Start();
                retriever.HandleBlockReceived(fixture.Debian.Events.BlockReceived[1]);

                handler.Wait().Should().BeTrue();
            }
        }

        [Test]
        public void ShouldTriggerOnDataCompleted()
        {
            Trigger handler = Trigger.Bind(ref hooks.OnDataCompleted, data =>
            {
                data.Hash.Should().Be(fixture.Debian.Metadata.Hash);
            });

            using (RetrieverService retriever = NewRetrieverService())
            {
                retriever.Start();

                foreach (BlockReceived data in fixture.Debian.Events.BlockReceived)
                {
                    retriever.HandleBlockReceived(data);
                }

                handler.Wait().Should().BeTrue();
            }
        }

        [Test]
        public void ShouldTriggerDataRequested()
        {
            Trigger handler = Trigger.Bind(ref hooks.OnBlockRequested, data =>
            {
                data.Hash.Should().Be(fixture.Debian.Metadata.Hash);
                data.Peer.Should().Be(environemnt.Peers.Sue.Hash);
                data.Piece.Should().Be(1);
                data.Block.Should().Be(0);
            });

            PeerChanged changed = new PeerChanged
            {
                Peer = environemnt.Peers.Sue.Hash,
                Bitfield = Bitfield.Sequence(false, true, false),
                IsLocalInterestedInRemote = true
            };

            using (RetrieverService retriever = NewRetrieverService())
            {
                retriever.Start();
                retriever.HandlePeerChanged(changed);

                handler.Wait().Should().BeTrue();
            }
        }
    }
}