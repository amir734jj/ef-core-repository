using System.Linq;
using ConsoleApp.Models;
using EfCoreRepository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp.Profiles
{
    public class DummyModelProfile : IEntityProfile<DummyModel, int> 
    {
        private readonly IEntityProfileAuxiliary _auxiliary;

        public DummyModelProfile(IEntityProfileAuxiliary auxiliary)
        {
            _auxiliary = auxiliary;
        }

        public void Update(DummyModel entity, DummyModel dto)
        {
            entity.Name = dto.Name;
            entity.Children = _auxiliary.ModifyList<Nested, int>(entity.Children, dto.Children);
        }

        public IQueryable<DummyModel> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<DummyModel>
        {
            return queryable.Include(x => x.Children);
        }
    }
}