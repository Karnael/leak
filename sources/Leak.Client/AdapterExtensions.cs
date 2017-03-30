using Leak.Client.Adapters;
using Leak.Data.Get;
using Leak.Data.Map;
using Leak.Data.Share;
using Leak.Data.Store;
using Leak.Glue;
using Leak.Memory;
using Leak.Meta.Get;
using Leak.Meta.Store;
using Leak.Networking;

namespace Leak.Client
{
    public static class AdapterExtensions
    {
        public static MetagetGlue AsMetaGet(this GlueService service)
        {
            return new MetaGetToGlueAdapter(service);
        }

        public static MetagetMetafile AsMetaGet(this MetafileService service)
        {
            return new MetaGetToMetaStoreAdapter(service);
        }

        public static DataGetToGlue AsDataGet(this GlueService service)
        {
            return new DataGetToGlueAdapter(service);
        }

        public static DataGetToDataStore AsDataGet(this RepositoryService service)
        {
            return new DataGetToDataStoreAdapter(service);
        }

        public static DataGetToDataMap AsDataGet(this OmnibusService service)
        {
            return new DataGetToDataMapAdapter(service);
        }

        public static DataShareToDataMap AsDataShare(this OmnibusService service)
        {
            return new DataShareToDataMapAdapter(service);
        }

        public static DataShareToDataStore AsDataShare(this RepositoryService service)
        {
            return new DataShareToDataStoreAdapter(service);
        }

        public static DataShareToGlue AsDataShare(this GlueService service)
        {
            return new DataShareToGlueAdapter(service);
        }

        public static NetworkPoolMemory AsNetwork(this MemoryService service)
        {
            return new MemoryToNetworkAdapter(service);
        }

        public static RepositoryMemory AsDataStore(this MemoryService service)
        {
            return new MemoryToDataStoreAdapter(service);
        }
    }
}