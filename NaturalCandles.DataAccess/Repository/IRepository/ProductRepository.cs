using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NaturalCandles.DataAccess.Data;
using NaturalCandles.DataAccess.Repository.IRepository;
using NaturalCandles.Models;

namespace NaturalCandles.DataAccess.Repository
{
	public class ProductRepository : Repository<Product>, IProductRepository
    {
		private ApplicationDbContext _db;
		public ProductRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}

		public void Update(Product obj)
		{
			var objFromDb = _db.Products.FirstOrDefault(u=>u.ProductId == obj.ProductId);
			if(objFromDb != null)
			{
				objFromDb.Name = obj.Name;
				objFromDb.Description = obj.Description;
				objFromDb.CategoryName = obj.CategoryName;
				objFromDb.BasePrice = obj.BasePrice;
				objFromDb.Price4 = obj.Price4;
				objFromDb.Price10 = obj.Price10;
				objFromDb.AvailableNow = obj.AvailableNow;
				objFromDb.HasColorOption = obj.HasColorOption;
				objFromDb.HasColorSchemeOption = obj.HasColorSchemeOption;
				objFromDb.HasTypeOption = obj.HasTypeOption;
				if (obj.ImageUrl != null)
				{
					objFromDb.ImageUrl = obj.ImageUrl;
				}


			}
		}
	}
}
