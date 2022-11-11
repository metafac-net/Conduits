﻿using Conduits.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Conduits.Testing
{
    public sealed class FakeConduitServer : IConduitServer, IDisposable
    {
        private readonly IConduitServer _server;
        private volatile bool _disposed = false;

        public string ServerName => ThisAssembly.AssemblyName;
        public string ServerVersion => ThisAssembly.AssemblyVersion;

        /// <summary>
        /// Creates a test/mock server wrapping another conduit server.
        /// </summary>
        /// <param name="server">The wrapped server.</param>
        public FakeConduitServer(IConduitServer server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }

        public IAsyncEnumerable<ReadOnlyMemory<byte>> ServerStream(ReadOnlyMemory<byte> request, CallContext context)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
            return _server.ServerStream(request, context);
        }

        public ValueTask<ReadOnlyMemory<byte>> SimpleUnaryCall(ReadOnlyMemory<byte> request, CallContext context)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
            return _server.SimpleUnaryCall(request, context);
        }

        public ValueTask<ReadOnlyMemory<byte>> ClientStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, CallContext context)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
            return _server.ClientStream(requests, context);
        }

        public IAsyncEnumerable<ReadOnlyMemory<byte>> DuplexStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, CallContext context)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
            return _server.DuplexStream(requests, context);
        }
    }
}
