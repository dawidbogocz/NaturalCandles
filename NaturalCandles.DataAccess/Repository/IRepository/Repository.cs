using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NaturalCandles.DataAccess.Data;
using NaturalCandles.DataAccess.Repository.IRepository;

namespace NaturalCandles.DataAccess.Repository
{
	public class Repository<T> : IRepository<T> where T : class
	{
			public readonly ApplicationDbContext _db;
			internal DbSet<T> dbSet;
			public Repository(ApplicationDbContext db)
			{
				_db = db;
				this.dbSet = _db.Set<T>();
				_db.Products.Include(u => u.Category).Include(u => u.CategoryName);
			}
			public void Add(T entity)
		{
			dbSet.Add(entity);
		}

		public void Remove(T entity)
		{
			dbSet.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entity)
		{
			dbSet.RemoveRange(entity);
		}

		public T Get(Expression<Func<T, bool>> fliter, string? includeProperties = null)
		{
			IQueryable<T> query = dbSet;
			query = query.Where(fliter);
			if (!string.IsNullOrEmpty(includeProperties))
			{
				foreach (var includeProp in includeProperties
					.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}
			return query.FirstOrDefault();
		}

		public IEnumerable<T> GetAll(string? includeProperties = null)
		{
			IQueryable<T> query = dbSet;
			if(!string.IsNullOrEmpty(includeProperties))
			{
				foreach (var includeProp in includeProperties
					.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}
            return query.ToList();
        }
	}
}
