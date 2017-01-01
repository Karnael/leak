﻿using Leak.Common;
using Leak.Tasks;

namespace Leak.Omnibus
{
    public class OmnibusBuilder
    {
        private readonly OmnibusParameters parameters;
        private readonly OmnibusDependencies dependencies;
        private readonly OmnibusConfiguration configuration;

        public OmnibusBuilder()
        {
            parameters = new OmnibusParameters();
            dependencies = new OmnibusDependencies();
            configuration = new OmnibusConfiguration();
        }

        public OmnibusBuilder WithHash(FileHash hash)
        {
            parameters.Hash = hash;
            return this;
        }

        public OmnibusBuilder WithPipeline(LeakPipeline pipeline)
        {
            dependencies.Pipeline = pipeline;
            return this;
        }

        public OmnibusService Build()
        {
            return Build(new OmnibusHooks());
        }

        public OmnibusService Build(OmnibusHooks hooks)
        {
            return new OmnibusService(parameters, dependencies, configuration, hooks);
        }
    }
}
