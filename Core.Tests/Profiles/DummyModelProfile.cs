using System.Linq;
using Core.Tests.Models;
using EfCoreRepository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Tests.Profiles
{
    public class DummyModelProfile : IEntityProfile<DummyModel> 
    {
        private readonly IEntityProfileAuxiliary _auxiliary;

        public DummyModelProfile(IEntityProfileAuxiliary auxiliary)
        {
            _auxiliary = auxiliary;
        }

        public void Update(DummyModel entity, DummyModel dto)
        {
            entity.Name = dto.Name;
            entity.Children = _auxiliary.ModifyList<Nested, int>(entity.Children, dto.Children).ToList();
        }

        public IQueryable<DummyModel> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<DummyModel>
        {
            return queryable.Include(x => x.Children);
        }
    }
}