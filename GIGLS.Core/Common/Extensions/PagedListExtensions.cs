﻿using POST.Core.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Core.Common.Extensions
{
    public static class PagedListExtensions
    {
     
        public static async Task<PagedList<TEntity>> ToPagedListAsync<TEntity>(this IQueryable<TEntity> entities, int page, int size) where TEntity : class
        {
            var count = await entities.CountAsync();
            var pagedEntities = entities.Skip((size * (page - 1))).Take(size);

            if (entities.GetType() != typeof(IOrderedQueryable<>))
            {
                entities = entities.OrderByDescending(e => true);
            }

            return new PagedList<TEntity>(page, size)
            {
                Items = pagedEntities.AsNoTracking(),
                Count = count,
                Page = page,
                Size = size == 0 ? count : size
            };
        }
    }
}
