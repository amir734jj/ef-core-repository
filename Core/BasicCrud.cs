using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using EfCoreRepository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EfCoreRepository
{
    internal class BasicCrud<TSource> : IBasicCrudWrapper<TSource>, IBasicCrudSession<TSource>
        where TSource : class, IUntypedEntity
    {
        private readonly IEntityProfile<TSource> _profile;

        private readonly DbContext _dbContext;

        private readonly bool _session;

        private readonly DbSet<TSource> _dbSet;

        public BasicCrud(IEntityProfile<TSource> profile, DbContext dbContext, bool session)
        {
            _profile = profile;
            _dbContext = dbContext;
            _session = session;
            _dbSet = dbContext.Set<TSource>();
        }

        /// <summary>
        /// Returns all entities
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TSource>> GetAll()
        {
            return await _profile.Include(_dbSet).ToListAsync();
        }

        /// <summary>
        /// Returns an entity given the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TSource> Get<TId>(TId id) where TId : struct
        {
            return await _profile.Include(_dbSet).FirstOrDefaultAsync(LambdaFactory(id));
        }

        /// <summary>
        /// Returns filters list of entities
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<TSource> Get(Expression<Func<TSource, bool>> expression)
        {
            return await _profile.Include(_dbSet).FirstOrDefaultAsync(expression);
        }

        /// <summary>
        /// Update entity given filter expression and dto
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<TSource> Update(Expression<Func<TSource, bool>> expression, TSource dto)
        {
            var entity = await Get(expression);

            if (entity != null)
            {
                _profile.Update(entity, dto);

                if (!_session)
                {
                    await _dbContext.SaveChangesAsync();
                }

                return entity;
            }

            return null;
        }

        /// <summary>
        /// Deletes entity given filter expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<TSource> Delete(Expression<Func<TSource, bool>> expression)
        {
            var entity = await Get(expression);

            if (entity != null)
            {
                _dbSet.Remove(entity);

                if (!_session)
                {
                    await _dbContext.SaveChangesAsync();
                }

                return entity;
            }

            return null;
        }

        /// <summary>
        /// Get all entities given a filter expression
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TSource>> GetAll(Expression<Func<TSource, bool>> filter)
        {
            return await _profile.Include(_dbSet).Where(filter).ToListAsync();
        }

        /// <summary>
        /// Get all entities given Id array
        /// </summary>
        /// <param name="ids"></param>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<TSource>> GetAll<TId>(params TId[] ids) where TId : struct
        {
            return await _profile.Include(_dbSet).Where(LambdaFactory(ids)).ToListAsync();
        }

        /// <summary>
        /// Save many DTOs at the same time
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TSource>> Save(params TSource[] instances)
        {
            await _dbSet.AddRangeAsync(instances);

            if (!_session)
            {
                await _dbContext.SaveChangesAsync();
            }

            return instances;
        }

        /// <summary>
        /// Saves an instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Save(TSource instance)
        {
            await _dbSet.AddAsync(instance);

            if (!_session)
            {
                await _dbContext.SaveChangesAsync();
            }

            return instance;
        }

        /// <summary>
        /// Deletes entity given the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Delete<TId>(TId id) where TId : struct
        {
            return await Delete(LambdaFactory(id));
        }

        /// <summary>
        /// Updates entity given the id and new instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct
        {
            return await Update(LambdaFactory(id), dto);
        }

        /// <summary>
        /// Invoke SaveChanges if session mode is active
        /// </summary>
        public void Dispose()
        {
            if (_session)
            {
                _dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Invoke SaveChangesAsync if session mode is active
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync()
        {
            if (_session)
            {
                return new ValueTask(_dbContext.SaveChangesAsync());
            }

            return new ValueTask(Task.CompletedTask);
        }

        /// <summary>
        /// Activates session mode which means SaveChanges will not get called unless repo is disposed
        /// </summary>
        /// <returns></returns>
        public IBasicCrudSession<TSource> Session()
        {
            return new BasicCrud<TSource>(_profile, _dbContext, true);
        }

        /// <summary>
        /// Creates a lambda expression of x => x.Id == id
        /// </summary>
        /// <param name="ids"></param>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        private static Expression<Func<TSource, bool>> LambdaFactory<TId>(params TId[] ids) where TId : struct
        {
            var parameter = Expression.Parameter(typeof(TSource));

            Expression body;

            switch (ids.Length)
            {
                case 0:
                    body = Expression.Constant(true);
                    break;
                case 1:
                    body = Expression.Equal(Expression.PropertyOrField(parameter, "Id"),
                        Expression.Constant(ids.First()));
                    break;
                default:
                {
                    var method = typeof(Enumerable)
                        .GetRuntimeMethods()
                        .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

                    var containsMethod = method.MakeGenericMethod(typeof(TId));
                    var containsInvoke = Expression
                        .Call(containsMethod, Expression.Constant(ids), Expression.PropertyOrField(parameter, "Id"));

                    body = containsInvoke;
                    break;
                }
            }

            var expression = Expression.Lambda<Func<TSource, bool>>(body, parameter);

            return expression;
        }
    }
}