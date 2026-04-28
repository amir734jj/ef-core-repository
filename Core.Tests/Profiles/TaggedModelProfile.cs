using Core.Tests.Models;
using EfCoreRepository;

namespace Core.Tests.Profiles;

public class TaggedModelProfile : EntityProfile<TaggedModel>
{
    public TaggedModelProfile()
    {
        MapAll();
    }
}
