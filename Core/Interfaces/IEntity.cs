using System;
using System.ComponentModel.DataAnnotations;

namespace EfCoreRepository.Interfaces
{
    public interface IEntity<TId> : IUntypedEntity where TId : struct
    {
        [Key]
        public TId Id { get; set; }

        TIdU IUntypedEntity.GetId<TIdU>()
        {
            return (TIdU) Convert.ChangeType(Id, typeof(TIdU));
        }
    }

    public interface IUntypedEntity
    {
        TId GetId<TId>() where TId : struct;
    }
}