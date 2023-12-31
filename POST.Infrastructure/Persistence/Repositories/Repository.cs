﻿using GIGL.POST.Core.Domain;
using GIGL.POST.Core.Repositories;
using POST.Core;
using POST.Core.Domain.Utility;
using POST.Core.IRepositories;
using POST.CORE.Domain;
using POST.Infrastructure.IdentityInfrastrure;
using POST.Infrastructure.Persistence.Repositories;
using LinqKit;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace POST.Infrastructure.Persistence.Repository
{
    public class Repository<TEntity, TContext> : IRepository<TEntity>
        where TEntity : class
        where TContext : GIGLSContext
    {
        protected readonly TContext Context;

        public Repository(TContext context)
        {
            Context = context;
            MapperConfig.Initialize();
        }

        public TEntity Get(int id)
        {
            return Context.Set<TEntity>().Find(id);
        }

        public Task<TEntity> GetAsync(int id)
        {
            return Context.Set<TEntity>().FindAsync(id);
        }

        public Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }
        public Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, string includeProperties = "")
        {
            IQueryable<TEntity> query = Context.Set<TEntity>();
            query = _IncludeProperties(query, includeProperties);
            return query.FirstOrDefaultAsync(predicate);
        }

        public IQueryable<TEntity> GetAll()
        {
            return Context.Set<TEntity>().AsQueryable();
        }

        public IQueryable<TEntity> GetAllAsQueryable()
        {
            return Context.Set<TEntity>().AsQueryable();
        }

        public IEnumerable<TEntity> GetAll(string includeProperties)
        {
            IQueryable<TEntity> query = Context.Set<TEntity>();
            query = _IncludeProperties(query, includeProperties);
            return query.ToList();  
        }
        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().Where(predicate);
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, string includeProperties = "")
        {
            IQueryable<TEntity> query = Context.Set<TEntity>();
            query = _IncludeProperties(query, includeProperties);
            return query.Where(predicate);
        }

        public Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, string includeProperties = "")
        {
            IQueryable<TEntity> query = Context.Set<TEntity>();
            query = _IncludeProperties(query, includeProperties);
            return Task.FromResult<IEnumerable<TEntity>>(query.Where(predicate).ToList());
        }

        private IQueryable<TEntity> _IncludeProperties(IQueryable<TEntity> query, string properties)
        {
            if (!string.IsNullOrEmpty(properties))
            {
                foreach (var property in properties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property.Trim());
                }
            }
            return query;
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().SingleOrDefault(predicate);
        }

        public void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().AddRange(entities);
        }
        
        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
        }

        public Task<bool> ExistAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().AnyAsync(predicate);
        }
    
        internal IQueryable<TEntity> Select(
             Expression<Func<TEntity, bool>> filter = null,
             List<Expression<Func<TEntity, object>>> includes = null,
             Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
             int? page = null,
             int? pageSize = null)
        {
            IQueryable<TEntity> query = Context.Set<TEntity>();

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (orderBy == null)
            {
                query = query.OrderBy("DateCreated asc");
            }

            if (filter != null)
            {
                query = query.AsExpandable().Where(filter);
            }
            if (page != null && pageSize != null)
            {
                var t = (page.Value - 1) * pageSize.Value;
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            return query;
        }

        internal async Task<IEnumerable<TEntity>> SelectAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null)
        {
            return await Select(filter, includes, orderBy, page, pageSize).ToListAsync();
        }

        public IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query)
        {
            return new QueryFluent<TEntity,TContext>(this, query);
        }

        public IQueryFluent<TEntity> Query()
        {
            return new QueryFluent<TEntity,TContext>(this);
        }





    }

    public class AuthRepository<TEntity, TContext> : IDisposable
                where TEntity : class
        where TContext : GIGLSContext
    {

        public UserManager<User> _userManager;
        public RoleManager<AppRole> _roleManager; 
        public Repository<User, TContext> _repo;
        public Repository<AppRole, TContext> _repoRole;
        public Repository<GlobalProperty, TContext> _globalProperty;
        public Repository<Company, TContext> _companyProperty;

        public AuthRepository(TContext context)
        {
            _userManager = new UserManager<User>(new GiglsUserStore<User>(context));
            _roleManager = new RoleManager<AppRole>(new RoleStore<AppRole>(context)); 
            _repo = new Repository<User, TContext>(context);
            _globalProperty = new Repository<GlobalProperty, TContext>(context);
            _companyProperty = new Repository<Company, TContext>(context);
        }
        
        public void Dispose()
        {
            _userManager.Dispose();
            //_roleManager.Dispose();
        }
    }
}