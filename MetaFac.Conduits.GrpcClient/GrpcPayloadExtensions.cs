using Google.Protobuf;
using MetaFac.Conduits.GrpcCommon;
using System;

namespace MetaFac.Conduits.GrpcClient
{
    internal static class GrpcPayloadExtensions
    {
        public static GrpcPayload ToGrpcPayload(this ReadOnlyMemory<byte> input)
        {
            return new GrpcPayload() { Data = UnsafeByteOperations.UnsafeWrap(input) };
        }
        public static ReadOnlyMemory<byte> ToPayload(this GrpcPayload input)
        {
            return input.Data.Memory;
        }
    }
}