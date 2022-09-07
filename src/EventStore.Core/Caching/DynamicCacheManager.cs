using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EventStore.Common.Utils;
using EventStore.Core.Bus;
using EventStore.Core.Messages;
using EventStore.Core.Messaging;
using EventStore.Core.Services.TimerService;
using Serilog;

namespace EventStore.Core.Caching {
	public class DynamicCacheManager:
		IHandle<MonitoringMessage.DynamicCacheManagerTick>,
		IHandle<MonitoringMessage.InternalStatsRequest> {

		private readonly IPublisher _bus;
		private readonly Func<long> _getFreeMem;
		private readonly long _totalMem;
		private readonly int _keepFreeMemPercent;
		private readonly long _keepFreeMemBytes;
		private readonly long _keepFreeMem;
		private readonly TimeSpan _minResizeInterval;
		private readonly ICacheResizer _rootCacheResizer;
		private readonly Message _scheduleTick;
		private readonly object _lock = new();

		private DateTime _lastResize = DateTime.UtcNow;

		public DynamicCacheManager(
			IPublisher bus,
			Func<long> getFreeMem,
			long totalMem,
			int keepFreeMemPercent,
			long keepFreeMemBytes,
			TimeSpan monitoringInterval,
			TimeSpan minResizeInterval,
			ICacheResizer rootCacheResizer) {

			if (keepFreeMemPercent is < 0 or > 100)
				throw new ArgumentException($"{nameof(keepFreeMemPercent)} must be between 0 to 100 inclusive.");

			if (keepFreeMemBytes < 0)
				throw new ArgumentException($"{nameof(keepFreeMemBytes)} must be non-negative.");

			_bus = bus;
			_getFreeMem = getFreeMem;
			_totalMem = totalMem;
			_keepFreeMemPercent = keepFreeMemPercent;
			_keepFreeMemBytes = keepFreeMemBytes;
			_keepFreeMem = Math.Max(_keepFreeMemBytes, _totalMem.ScaleByPercent(_keepFreeMemPercent));
			_minResizeInterval = minResizeInterval;
			_rootCacheResizer = rootCacheResizer;
			_scheduleTick = TimerMessage.Schedule.Create(
				monitoringInterval,
				new PublishEnvelope(_bus),
				new MonitoringMessage.DynamicCacheManagerTick());
		}

		public void Start() {
			ResizeCaches(_getFreeMem(), 0);
			Tick();
		}

		public void Handle(MonitoringMessage.DynamicCacheManagerTick message) {
			ThreadPool.QueueUserWorkItem(_ => {
				try {
					lock (_lock) { // only to add read/write barriers
						ResizeCachesIfNeeded();
					}
				} finally {
					Tick();
				}
			});
		}

		public void Handle(MonitoringMessage.InternalStatsRequest message) {
			Thread.MemoryBarrier(); // just to ensure we're seeing latest values

			var stats = new Dictionary<string, object>();
			var cachesStats = _rootCacheResizer.GetStats(string.Empty);

			foreach(var cacheStat in cachesStats) {
				var statNamePrefix = $"es-cache-{cacheStat.Key}-";
				stats[statNamePrefix + "name"] = cacheStat.Name;
				stats[statNamePrefix + "weight"] = cacheStat.Weight;
				stats[statNamePrefix + "mem-used"] = cacheStat.MemUsed;
				stats[statNamePrefix + "mem-allotted"] = cacheStat.MemAllotted;
			}

			message.Envelope.ReplyWith(new MonitoringMessage.InternalStatsRequestResponse(stats));
		}

		private void ResizeCachesIfNeeded() {
			var freeMem = _getFreeMem();

			if (freeMem >= _keepFreeMem && DateTime.UtcNow - _lastResize < _minResizeInterval)
				return;

			if (freeMem < _keepFreeMem) {
				Log.Debug("Available system memory is lower than "
				          + "{thresholdPercent}% or {thresholdBytes:N0} bytes: {freeMem:N0} bytes. Resizing caches.",
					_keepFreeMemPercent, _keepFreeMemBytes, freeMem);
			}

			try {
				var cachedMem = _rootCacheResizer.GetMemUsage();
				ResizeCaches(freeMem, cachedMem);
				GC.Collect(Math.Min(2, GC.MaxGeneration), GCCollectionMode.Forced);
			} catch(Exception ex) {
				Log.Error(ex, "Error while resizing caches");
			} finally {
				_lastResize = DateTime.UtcNow;
			}
		}

		private void ResizeCaches(long freeMem, long cachedMem) {
			var availableMem = CalcAvailableMemory(freeMem, cachedMem);
			_rootCacheResizer.CalcAllotment(availableMem, _rootCacheResizer.Weight);
		}

		// Memory available for caching
		private long CalcAvailableMemory(long freeMem, long cachedMem) {
			var availableMem = Math.Max(0L, freeMem + cachedMem - _keepFreeMem);

			Log.Debug("Calculating memory available for caching based on:\n" +
			          "Free memory: {freeMem:N0} bytes\n" +
			          "Total memory: {totalMem:N0} bytes\n" +
			          "Cached memory: ~{cachedMem:N0} bytes\n" +
			          "Keep free memory: {keepFreeMem:N0} bytes (higher of {keepFreeMemPercent}% total mem & {keepFreeMemBytes:N0} bytes)\n\n" +
			          "Memory available for caching: ~{availableMem:N0} bytes\n",
				freeMem, _totalMem, cachedMem,
				_keepFreeMem, _keepFreeMemPercent, _keepFreeMemBytes,
				availableMem);

			return availableMem;
		}

		private void Tick() => _bus.Publish(_scheduleTick);
	}
}
