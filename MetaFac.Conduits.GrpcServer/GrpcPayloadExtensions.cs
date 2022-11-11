
using Conduits.GrpcCommon;
using Google.Protobuf;
using System;

namespace Conduits.GrpcServer
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