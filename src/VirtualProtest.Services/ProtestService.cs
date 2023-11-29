using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VirtualProtest.Core.Interfaces;
using VirtualProtest.Core.Models;

namespace VirtualProtest.Services
{
    public class ProtestService : IProtestService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ProtestService> _logger;
        private const string ProtestCacheKey = "ProtestList";

        public ProtestService(IMemoryCache memoryCache, ILogger<ProtestService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;

            // Ensure the cache is initialized
            if (!_memoryCache.TryGetValue(ProtestCacheKey, out List<Protest>? cachedProtests))
            {
                _memoryCache.Set(ProtestCacheKey, new List<Protest>());
            }
        }

        public IEnumerable<Protest> GetAllProtests()
        {
            bool exists = _memoryCache.TryGetValue(ProtestCacheKey, out List<Protest>? protests);
            if (!exists || protests == null)
            {
                protests = new List<Protest>();
                _memoryCache.Set(ProtestCacheKey, protests);
            }

            return protests;
        }

        public Protest CreateProtest(Protest protest)
        {
            if (protest == null)
            {
                throw new ArgumentNullException(nameof(protest));
            }

            // Ensuring protests is not null before adding
            bool exists = _memoryCache.TryGetValue(ProtestCacheKey, out List<Protest>? protests);
            if (!exists || protests == null)
            {
                protests = new List<Protest>();
                _memoryCache.Set(ProtestCacheKey, protests);
            }

            var existingProtest = protests.FirstOrDefault(p => p.Id == protest.Id);
            if (existingProtest != null)
            {
                _logger.LogWarning("Protest with {ProtestId} already exists. TEMP: replacing it with new one - replacement for update for now.", protest.Id);
                protests.Remove(existingProtest);
            }

            protests.Add(protest); // Now safe to add

            _logger.LogInformation("Added new protest {ProtestId}", protest.Id);

            return protest;
        }

        public Protest? GetProtestById(Guid id)
        {
            // Ensuring protests is not null
            bool exists = _memoryCache.TryGetValue(ProtestCacheKey, out List<Protest>? protests);
            if (!exists || protests == null)
            {
                return null; // Return null immediately if the list doesn't exist
            }

            var protest = protests.FirstOrDefault(p => p.Id == id);

            _logger.LogInformation("Returning protest with requested {ProtestId}, returned null? {IsProtestNull} ", id, protest == null);

            return protest;
        }

        public void JoinProtest(Guid protestId, string participantId)
        {
            var protest = this.GetProtestById(protestId);
            if (protest != null && !protest.Participants.Any(p => p.Id == participantId))
            {
                protest.Participants.Add(new Participant { Id = participantId });
                _logger.LogInformation("Participant {ParticipantId} joined protest {ProtestId}", participantId, protestId);
            }
        }

        public void LeaveProtest(Guid protestId, string participantId)
        {
            var protest = this.GetProtestById(protestId);
            if (protest != null)
            {
                var participantToRemove = protest.Participants.FirstOrDefault(p => p.Id == participantId);
                if (participantToRemove != null)
                {
                    protest.Participants.Remove(participantToRemove);
                    _logger.LogInformation("Participant {ParticipantId} left protest {ProtestId}", participantId, protestId);
                }
            }
        }

        public int GetParticipantCount(Guid protestId)
        {
            var protest = this.GetProtestById(protestId);
            if (protest != null)
            {
                return protest.Participants.Count;
            }
            return 0;
        }
    }
}