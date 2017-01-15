﻿using Leak.Common;
using Leak.Glue;
using Leak.Metafile;
using Leak.Tasks;

namespace Leak.Metashare
{
    public class MetashareBuilder
    {
        private readonly MetashareParameters parameters;
        private readonly MetashareDependencies dependencies;
        private readonly MetashareConfiguration configuration;

        public MetashareBuilder()
        {
            parameters = new MetashareParameters();
            dependencies = new MetashareDependencies();
            configuration = new MetashareConfiguration();
        }

        public MetashareBuilder WithHash(FileHash hash)
        {
            parameters.Hash = hash;
            return this;
        }

        public MetashareBuilder WithPipeline(LeakPipeline pipeline)
        {
            dependencies.Pipeline = pipeline;
            return this;
        }

        public MetashareBuilder WithGlue(GlueService glue)
        {
            dependencies.Glue = glue;
            return this;
        }

        public MetashareBuilder WithMetafile(MetafileService metafile)
        {
            dependencies.Metafile = metafile;
            return this;
        }

        public MetashareService Build()
        {
            return Build(new MetashareHooks());
        }

        public MetashareService Build(MetashareHooks hooks)
        {
            return new MetashareService(parameters, dependencies, configuration, hooks);
        }
    }
}