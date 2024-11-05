using System;

namespace EfCoreRepository.Models
{
    [Flags]
    internal enum SessionType
    {
        Generic = 1,
        LightWeight = 2,
        Delayed = 4,
        NoTracking = 8,
    }
}