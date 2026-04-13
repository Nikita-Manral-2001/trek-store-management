using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TTH.Areas.Super.Data;

using TTH.Areas.Super.Data.TrekkersStore;

using TTH.Areas.Super.Models.TrekkersStore;
using TTH.Areas.Super.Repository.RentRepository;
using TTH.Models.user;


namespace TTH.Areas.Super.Repository
{
    public class StoreAdminRepository
    {


      
        private readonly AppDataContext _context;
        private readonly IWebHostEnvironment _IwebHostEnvironment;

        public StoreAdminRepository(AppDataContext context, IWebHostEnvironment iwebHostEnvironment)
        {
           
            _IwebHostEnvironment = iwebHostEnvironment;
            _context = context;

        }

        private async Task<string> SaveImage(string folderName1, string folderName2, IFormFile file)
        {

            string folderPath = $"images/{(folderName1)}/{folderName2}/";

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string serverFilePath = Path.Combine(_IwebHostEnvironment.WebRootPath, folderPath, uniqueFileName);

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(serverFilePath));

            using (var stream = new FileStream(serverFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/" + folderPath + uniqueFileName;
        }








        public async Task<int> AddCategory(CategoryModel model)
        {
            string uniqueFileName = null;
            if (model.CoverPhoto != null)
            {
                

                uniqueFileName = await SaveImage("Shop", "Category", model.CoverPhoto);
                
              
            }

            var newCategory = new StoreCategory()
            {
                CategoryName = model.CategoryName,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = model.CreatedBy,
                CoverImgUrl = uniqueFileName
            };

            await _context.StoreCategory.AddAsync(newCategory);
            await _context.SaveChangesAsync();
            return newCategory.Category_Id;
        }


        public async Task<List<CategoryModel>> GetStoreCategory()
        {
            var categories = await _context.StoreCategory.ToListAsync();




            return categories.Select(category => new CategoryModel()
            {
                Category_Id = category.Category_Id,
                CategoryName = category.CategoryName,
                CreatedBy=category.CreatedBy,
                CreatedOn=category.CreatedOn,
                CoverImgUrl=category.CoverImgUrl,
                IsVisible=category.IsVisible


            }).ToList();
        }


        public async Task<CategoryModel> GetCategoryById(int id)
        {
            var category = await _context.StoreCategory.FindAsync(id);
            if (category == null)
            {
                return null;
            }



            return new CategoryModel
            {
                Category_Id = category.Category_Id,
                CategoryName = category.CategoryName,
                CoverImgUrl=category.CoverImgUrl
            };
        }
        public async Task UpdateCategory(CategoryModel model)
        {
            string uniqueFileName = null;
            if (model.CoverPhoto != null)
            {
                uniqueFileName = await SaveImage("Shop", "Category", model.CoverPhoto);
            }
            var existingCategory = await _context.StoreCategory.FindAsync(model.Category_Id);
            if (existingCategory != null)
            {
                existingCategory.CategoryName = model.CategoryName;
                existingCategory.CoverImgUrl = uniqueFileName;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteCategory(int id)
        {
            var category = await _context.StoreCategory.FindAsync(id);
            if (category != null)
            {
                _context.StoreCategory.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Size

        public async Task<List<StoreSizeModel>> GetSize()
        {
            return await _context.StoreSize.Select(x => new StoreSizeModel()
            {
                IdSize = x.IdSize,
                Name = x.Name,
                CreatedOn = DateTime.UtcNow, // Use DateTime.Now if you want local time
                CreatedBy = x.CreatedBy,
               IsVisible=x.IsVisible
            }).ToListAsync();
        }

        public async Task AddSize(StoreSizeModel storeSizeModel)
        {
            var size = new StoreSize
            {
                Name = storeSizeModel.Name,
                CreatedOn=storeSizeModel.CreatedOn,
                CreatedBy=storeSizeModel.CreatedBy,
                IsVisible=true
            };
            _context.StoreSize.Add(size);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSize(int id)
        {
            var size = await _context.StoreSize.FindAsync(id);
            if (size != null)
            {
                _context.StoreSize.Remove(size);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<StoreSizeModel> GetSizeById(int id)
        {
            return await _context.StoreSize.Where(x => x.IdSize == id).Select(x => new StoreSizeModel()
            {
                IdSize = x.IdSize,
                Name = x.Name,
            }).FirstOrDefaultAsync();
        }


        // For Color

        public async Task<List<StoreColorModel>> GetColor()
        {
            return await _context.StoreColor.Select(x => new StoreColorModel()
            {
                IdColor = x.IdColor,
                Name = x.Name,
                CreatedOn = DateTime.UtcNow, // Use DateTime.Now if you want local time
                CreatedBy = x.CreatedBy,
               IsVisible=x.IsVisible
            }).ToListAsync();
        }

        public async Task AddColor(StoreColorModel storeColorModel)
        {
            var color = new StoreColor
            {
                Name = storeColorModel.Name,
                CreatedOn = storeColorModel.CreatedOn,
                CreatedBy = storeColorModel.CreatedBy
            };
            _context.StoreColor.Add(color);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteColor(int id)
        {
            var color = await _context.StoreColor.FindAsync(id);
            if (color != null)
            {
                _context.StoreColor.Remove(color);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<StoreColorModel> GetColorById(int id)
        {
            return await _context.StoreColor.Where(x => x.IdColor == id).Select(x => new StoreColorModel()
            {
                IdColor = x.IdColor,
                Name = x.Name,
            }).FirstOrDefaultAsync();
        }


        public async Task<List<StoreProductsModel>> GetStoreProductsDetails()
        {
            var products = await _context.StoreProducts
              
                .ToListAsync();

            return products.Select(product => new StoreProductsModel
            {
                Products_Id = product.Products_Id,
                Price=product.Price,

                CoverImgUrl = product.CoverImgUrl,
                Description = product.Description,
                ProductName = product.ProductName,
                IsVisible=product.IsVisible
               
            }).ToList();
        }
    }
}
