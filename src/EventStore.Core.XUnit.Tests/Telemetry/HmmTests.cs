using System;
using EventStore.Core.Tests.TransactionLog;
using EventStore.Core.TransactionLog.Chunks;
using EventStore.Core.TransactionLog.Chunks.TFChunk;
using EventStore.Core.Transforms.Identity;
using Xunit;

namespace EventStore.Core.XUnit.Tests.Telemetry;

public sealed class Hmm {
	[Fact]

	public void crash() {
		var config = TFChunkHelper.CreateSizedDbConfig("ignored", 0, chunkSize: 4096);
		var _db = new TFChunkDb(config);

		_db.Open(false);
		var chunk = _db.Manager.GetChunkFor(0);
		var result = chunk.TryReadAt(0, false);


		var xxx = new object[400_000];
		GC.KeepAlive(xxx);
	}

	[Fact]
	public void no_crash() {
		var chunk = TFChunk.CreateWithHeader(
			filename: "filename",
			header: new ChunkHeader(
				version: 3,
				chunkSize: 4000,
				chunkStartNumber: 1,
				chunkEndNumber: 1,
				isScavenged: false,
				chunkId: Guid.NewGuid(),
				transformType: Transforms.TransformType.Identity),
			fileSize: 4000,
			inMem: true,
			unbuffered: false,
			writethrough: false,
			reduceFileCachePressure: false,
			tracker: new TFChunkTracker.NoOp(),
			transformFactory: new IdentityChunkTransformFactory(),
			transformHeader: ReadOnlyMemory<byte>.Empty);

		var result = chunk.TryReadAt(0, false);
		var xxx = new object[400_000];
		GC.KeepAlive(xxx);
	}
}
