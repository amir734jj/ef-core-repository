using Core.Tests.Models;
using EfCoreRepository;

namespace Core.Tests.Profiles
{
    public class NestedProfile : EntityProfile<Nested>
    {
        public NestedProfile()
        {
            MapAll(x => x.ParentRef);
        }
    }
}