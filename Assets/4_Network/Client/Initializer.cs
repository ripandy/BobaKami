using BlendShapeBroadcast.Shared;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Unity;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;
using UnityEngine;

namespace Network.Client
{
    [MagicOnionClientGeneration(typeof(IBlendShapeHub))]
    partial class MagicOnionGeneratedClientInitializer
    {
    }

    class Initializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void RegisterResolvers()
        {
            // NOTE: Currently, CompositeResolver doesn't work on Unity IL2CPP build. Use StaticCompositeResolver instead of it.
            StaticCompositeResolver.Instance.Register(
                MagicOnionGeneratedClientInitializer.Resolver,
                StandardResolver.Instance,
                UnityResolver.Instance
            );

            MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions
                .WithResolver(StaticCompositeResolver.Instance);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnRuntimeInitialize()
        {
            // Use Grpc.Net.Client instead of C-core gRPC library.
            GrpcChannelProviderHost.Initialize(
                new GrpcNetClientGrpcChannelProvider(() => new GrpcChannelOptions()
                {
                    HttpHandler = new Cysharp.Net.Http.YetAnotherHttpHandler()
                    {
                        Http2Only = true,
                    }
                }));
        }
    }
}