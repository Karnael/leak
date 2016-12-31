﻿using Leak.Events;
using Leak.Glue;
using Leak.Metafile;
using Leak.Tasks;

namespace Leak.Metaget
{
    public class MetagetContext
    {
        private readonly MetagetParameters parameters;
        private readonly MetagetDependencies dependencies;
        private readonly MetagetHooks hooks;
        private readonly MetagetConfiguration configuration;
        private readonly LeakQueue<MetagetContext> queue;

        private MetamineBitfield metamine;

        public MetagetContext(MetagetParameters parameters, MetagetDependencies dependencies, MetagetHooks hooks, MetagetConfiguration configuration)
        {
            this.parameters = parameters;
            this.dependencies = dependencies;
            this.hooks = hooks;
            this.configuration = configuration;

            queue = new LeakQueue<MetagetContext>(this);

            dependencies.Metafile.Hooks.OnMetafileVerified += OnMetafileVerified;
        }

        private void OnMetafileVerified(MetafileVerified data)
        {
            this.hooks.CallMetadataDiscovered(data.Hash, data.Metainfo);
        }

        public MetamineBitfield Metamine
        {
            get { return metamine; }
            set { metamine = value; }
        }

        public MetagetParameters Parameters
        {
            get { return parameters; }
        }

        public MetagetDependencies Dependencies
        {
            get {  return dependencies; }
        }

        public MetagetHooks Hooks
        {
            get { return hooks; }
        }

        public MetagetConfiguration Configuration
        {
            get { return configuration; }
        }

        public LeakQueue<MetagetContext> Queue
        {
            get { return queue; }
        }
    }
}