using System;
using System.Collections.Generic;
using VirtualProtest.Core.Models;

namespace VirtualProtest.Core.Interfaces
{
    public interface IProtestService
    {
        IEnumerable<Protest> GetAllProtests();
        Protest CreateProtest(Protest protest);
        Protest? GetProtestById(Guid id);
        void JoinProtest(Guid protestId, string participantId);
        void LeaveProtest(Guid protestId, string participantId);
        int GetParticipantCount(Guid protestId);
    }
}