using System.Linq;
using ConsoleApp.Models;
using EfCoreRepository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp.Profiles
{
    public class DummyModelProfile : IEntityProfile<DummyModel, int> 
    {
        private readonly IEntityProfileAuxiliary<Nested, int> _auxiliary;

        public DummyModelProfile(IEntityProfileAuxiliary<Nested, int> auxiliary)
        {
            _auxiliary = auxiliary;
        }

        public DummyModel Update(DummyModel entity, DummyModel dto)
        {
            entity.Name = dto.Name;
            entity.Children = _auxiliary.ModifyList(entity.Children, dto.Children);

            return entity;
        }

        public IQueryable<DummyModel> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<DummyModel>
        {
            return queryable.Include(x => x.Children);
        }
    }
}