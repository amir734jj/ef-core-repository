using System.Collections.Generic;
using System.Linq;
using Core.Tests.Models;
using EfCoreRepository;
using EfCoreRepository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Tests.Profiles
{
    public class DummyModelProfile : EntityProfile<DummyModel> 
    {
        public override void Update(DummyModel entity, DummyModel dto)
        {
            entity.Name = dto.Name;
            ModifyList(entity.Children, dto.Children, x => x.Id);
        }

        public override IQueryable<DummyModel> Include<TQueryable>(TQueryable queryable)
        {
            return queryable.Include(x => x.Children);
        }
    }
}