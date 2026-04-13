using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Security.Claims;
using TTH.Areas.Super.Data;
using TTH.Areas.Super.Data.Rent;
using TTH.Areas.Super.Data.TrekkersStore;
using TTH.Areas.Super.Models;
using TTH.Areas.Super.Models.Rent;

using TTH.Areas.Super.Models.TrekkersStore;
using TTH.Areas.Super.Repository;
using TTH.Areas.Super.Repository.RentRepository;
using TTH.Models;
using TTH.Models.user;

namespace TTH.Areas.Super.Controllers
{
    [Area("Super")]
    [Route("super/[controller]")]
    [Authorize(Roles = "Super,TLM,OperationTeam,SEO")]
    public class StoreAdminController : Controller
    {
        private readonly StoreAdminRepository _storeAdminRepository;
        private readonly AppDataContext _context;
        private readonly IWebHostEnvironment _IwebHostEnvironment;

        public StoreAdminController(StoreAdminRepository storeAdminRepository, AppDataContext context,IWebHostEnvironment iwebHostEnvironment)
        {
            _storeAdminRepository = storeAdminRepository;
            _IwebHostEnvironment = iwebHostEnvironment;
            _context = context;

        }
        //Add Products In Store
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

        [HttpGet]
        [Route("StoreAddProducts")]
        public IActionResult StoreAddProducts()
        {
            var categories = _context.StoreCategory.Where(c=>c.IsVisible==true)
                   .Select(c => new StoreCategoryViewModel
                   {
                       Category_Id = c.Category_Id,
                       CategoryName = c.CategoryName
                   })
                   .ToList();

            ViewBag.Categories = categories;


            return View();
        }
        [HttpPost]
        [Route("StoreAddProducts")]
        public async Task<IActionResult> StoreAddProducts(StoreProductsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var categories = _context.StoreCategory
                   .Select(c => new StoreCategoryViewModel
                   {
                       Category_Id = c.Category_Id,
                       CategoryName = c.CategoryName
                   })
                   .ToList();

            ViewBag.Categories = categories;
            string UserEmail = null;
            // Save image file
            string uniqueFileName = null;
            if (model.CoverPhoto != null)
            {
                

                uniqueFileName = await SaveImage("Shop", "Products", model.CoverPhoto);
              
            }
            if (User.Identity.IsAuthenticated)
            {
                var emailClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                UserEmail = emailClaim?.Value;
            }

            // Save data to database
            var productEntity = new StoreProducts
            {
                Products_Id = model.Products_Id,
                ProductName = model.ProductName,
                Description = model.Description,
                CoverImgUrl = uniqueFileName,
                CreatedBy = UserEmail, // or fetch the current user
                CreatedOn = DateTime.Now,
                CategoryId = model.Category_Id,
                Price=model.Price,
                IsVisible=true
            };

            // Assuming _context is your DbContext and injected via DI
            _context.StoreProducts.Add(productEntity);
            await _context.SaveChangesAsync();
            ViewBag.SuccessMessage = "Product added successfully!";
            ModelState.Clear();
            return View(); // return blank form with success message
        }



        //AddSize

        [HttpGet]
        [Route("StoreSizeList")]
        public async Task<IActionResult> StoreSizeList()
        {
            var sizes = await _storeAdminRepository.GetSize();
            return View(sizes);
        }

        [HttpPost]
        [Route("AddSize")]
        public async Task<IActionResult> AddSize(StoreSizeModel sizeModel)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var emailClaim = User.Claims.FirstOrDefault(c =>
                        c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                    sizeModel.CreatedBy = emailClaim?.Value;
                }


                sizeModel.CreatedOn = DateTime.UtcNow;

                await _storeAdminRepository.AddSize(sizeModel);
                return RedirectToAction(nameof(StoreSizeList));
            }
            return View(nameof(StoreSizeList), await _storeAdminRepository.GetSize());
        }

        [HttpPost]
        [Route("DeleteSize")]
        public async Task<IActionResult> DeleteSize(int id)
        {
            await _storeAdminRepository.DeleteSize(id);
            return RedirectToAction(nameof(StoreSizeList));
        }


        //For Store Add Color

        [HttpGet]
        [Route("StoreColorList")]
        public async Task<IActionResult> StoreColorList()
        {
            var color = await _storeAdminRepository.GetColor();
            return View(color);
        }

        [HttpPost]
        [Route("AddColor")]
        public async Task<IActionResult> AddColor(StoreColorModel colorModel)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var emailClaim = User.Claims.FirstOrDefault(c =>
                        c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                    colorModel.CreatedBy = emailClaim?.Value;
                }


                colorModel.CreatedOn = DateTime.UtcNow;
                await _storeAdminRepository.AddColor(colorModel);
                return RedirectToAction(nameof(StoreColorList));
            }
            return View(nameof(StoreColorList), await _storeAdminRepository.GetColor());
        }

        [HttpPost]
        [Route("DeleteColor")]
        public async Task<IActionResult> DeleteColor(int id)
        {
            await _storeAdminRepository.DeleteColor(id);
            return RedirectToAction(nameof(StoreColorList));
        }

      
        //Add Categories
        [HttpGet]
        [Route("StoreAddCategories")]
        public async Task<IActionResult> StoreAddCategories()
        {
            var categoryList = await _storeAdminRepository.GetStoreCategory();
            return View(categoryList); // Pass the data to the view
        }



        [HttpPost]
        [Route("AddCategory")]
        public async Task<IActionResult> AddCategory(CategoryModel categoryModel)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var emailClaim = User.Claims.FirstOrDefault(c =>
                        c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                    categoryModel.CreatedBy = emailClaim?.Value;
                }


                categoryModel.CreatedOn = DateTime.UtcNow;

               

                await _storeAdminRepository.AddCategory(categoryModel);
                return RedirectToAction("StoreAddCategories");
            }

            var category = await _storeAdminRepository.GetStoreCategory();
            return View("StoreAddCategories", category);
        }


        [HttpGet]
        [Route("EditCategory/{id}")]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _storeAdminRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [Route("EditCategory/{id}")]
        public async Task<IActionResult> EditCategory(int id, CategoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Fetch the entity (StoreCategory) from DbContext
            var existingCategory = await _context.StoreCategory.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            string uniqueFileName = existingCategory.CoverImgUrl; // keep old image if not updated
            string userEmail = existingCategory.CreatedBy; // keep old CreatedBy if not updated

            if (model.CoverPhoto != null)
            {
                // Save new image
                uniqueFileName = await SaveImage("Shop", "Category", model.CoverPhoto);
            }

            if (User.Identity.IsAuthenticated)
            {
                var emailClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                userEmail = emailClaim?.Value ?? existingCategory.CreatedBy;
            }

            // Update entity fields
            existingCategory.CategoryName = model.CategoryName;
            existingCategory.CoverImgUrl = uniqueFileName;
            existingCategory.CreatedBy = userEmail;
            existingCategory.CreatedOn = DateTime.Now; // Or keep the old CreatedOn
            existingCategory.IsVisible = true;

            // Save changes
            _context.StoreCategory.Update(existingCategory);
            await _context.SaveChangesAsync();

            return RedirectToAction("StoreAddCategories"); // back to list
        }





        //[HttpPost]
        //[Route("EditCategory")]
        //public async Task<IActionResult> EditCategory(CategoryModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Fetch the existing category from the database
        //        var category = await _context.StoreCategory.FindAsync(model.Category_Id);
        //        if (category == null)
        //        {
        //            return NotFound();
        //        }

        //        // Check if a new image is uploaded
        //        if (model.CoverPhoto != null && model.CoverPhoto.Length > 0)
        //        {
        //            // Check and delete existing image file if present
        //            if (!string.IsNullOrEmpty(category.CoverImgUrl))
        //            {
        //                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", category.CoverImgUrl);
        //                if (System.IO.File.Exists(oldImagePath))
        //                {
        //                    System.IO.File.Delete(oldImagePath); // Delete the existing image
        //                }
        //            }

        //            // Save the new uploaded image
        //            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
        //            Directory.CreateDirectory(uploadsFolder); // Ensure uploads folder exists

        //            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CoverPhoto.FileName);
        //            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await model.CoverPhoto.CopyToAsync(stream);
        //            }

        //            // Update the new image name in the database
        //            category.CoverImgUrl = uniqueFileName;
        //        }

        //        // Update other fields
        //        category.CategoryName = model.CategoryName;
        //        category.CreatedBy = model.CreatedBy;
        //        category.CreatedOn = DateTime.Now;

        //        _context.Update(category);
        //        await _context.SaveChangesAsync();

        //        return RedirectToAction("CategoryList");
        //    }

        //    return View(model);
        //}








        [HttpPost]
        [Route("DeleteCategory/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _storeAdminRepository.DeleteCategory(id);
            if (success)
            {
                return RedirectToAction("StoreAddCategories");
            }
            // Handle the case when delete fails
            return BadRequest();
        }


        [HttpGet]
        [Route("GetAllProductDetails")]
        public async Task<IActionResult> GetAllProductDetails()
        {
            var products = await _storeAdminRepository.GetStoreProductsDetails();
            return View(products);
        }

        [HttpGet]
        [Route("EditProductDetails/{id}")]
        public async Task<IActionResult> EditProductDetails(int id)
        {
            var product = await _context.StoreProducts.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new StoreProductsModel
            {
                Products_Id = product.Products_Id,
                ProductName = product.ProductName,
                Description = product.Description,
                CoverImgUrl = product.CoverImgUrl,
                CreatedBy = product.CreatedBy,
                CreatedOn = product.CreatedOn,
                Category_Id = product.CategoryId,
                Price=product.Price
            };

            return View(model);
        }
        [HttpPost]
        [Route("EditProductDetails/{id}")]
        public async Task<IActionResult> EditProductDetails(int id, StoreProductsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var product = await _context.StoreProducts.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }


            string uniqueFileName = null;
            if (model.CoverPhoto != null)
            {


                uniqueFileName = await SaveImage("Shop", "Products", model.CoverPhoto);


            }
            else
            {
                uniqueFileName = _context.StoreProducts
      .Where(p => p.Products_Id == id)
      .Select(p => p.CoverImgUrl)
      .FirstOrDefault();
            }


            product.ProductName = model.ProductName;
            product.Description = model.Description;
            product.CreatedOn = DateTime.Now;
            product.Price = model.Price;
            product.CoverImgUrl = uniqueFileName;
            _context.StoreProducts.Update(product);
            await _context.SaveChangesAsync();

            var variants = await _context.ProductVariants
     .Where(p => p.ProductId == id)
     .ToListAsync();

            if (model.Price != null)
            {
                if (variants.Any())
                {
                    foreach (var variant in variants)
                    {
                        variant.Price = model.Price.Value; // Explicit conversion from decimal? to decimal
                    }

                    await _context.SaveChangesAsync();
                }
            }


            return RedirectToAction("GetAllProductDetails");

        }



        [HttpGet]
        [Route("AddVariants/{id}")]
        public ActionResult AddVariants(int id)
        {
            var sizes = _context.StoreSize.Where(c=>c.IsVisible==true)
                .Select(s => new SelectListItem
                {
                    Value = s.IdSize.ToString(),
                    Text = s.Name
                }).ToList();

            var colors = _context.StoreColor.Where(c => c.IsVisible == true)
                .Select(c => new SelectListItem
                {
                    Value = c.IdColor.ToString(),
                    Text = c.Name
                }).ToList();

            ViewBag.Sizes = sizes;
            ViewBag.Colors = colors;

            var variantDetails = _context.ProductVariants
    .Where(pv => pv.ProductId == id)
    .Select(pv => new ProductVariantViewModel
    {
        SizeId = pv.SizeId,
        SizeName = _context.StoreSize
            .Where(s => s.IdSize == pv.SizeId)
            .Select(s => s.Name)
            .FirstOrDefault(),
        ColorId = pv.ColorId,
        ColorName = _context.StoreColor
            .Where(c => c.IdColor == pv.ColorId)
            .Select(c => c.Name)
            .FirstOrDefault()
    })
    .ToList();

            var viewModel = new ProductVariantInputModel
            {
                ProductId = id,
                ExistingVariants = variantDetails
            };





            return View(viewModel);
        }
        [HttpPost]
        [Route("AddVariants")]
        public IActionResult AddVariants(int ProductId, List<int> SelectedSizeIds, List<int> SelectedColorIds)
        {
            string CreatedBy = null;
            if (User.Identity.IsAuthenticated)
            {
                var emailClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                CreatedBy = emailClaim?.Value;
            }

            for (int i = 0; i < SelectedSizeIds.Count; i++)
            {
                int sizeId = SelectedSizeIds[i];
                int colorId = SelectedColorIds[i];

                // Fetch SizeName and ColorName from DB
                string sizeName = _context.StoreSize
                    .Where(s => s.IdSize == sizeId)
                    .Select(s => s.Name)
                    .FirstOrDefault();

                string colorName = _context.StoreColor
                    .Where(c => c.IdColor == colorId)
                    .Select(c => c.Name)
                    .FirstOrDefault();

                var variant = new ProductVariants
                {
                    ProductId = ProductId,
                    SizeId = sizeId,
                    SizeName = sizeName,       // Save SizeName
                    ColorId = colorId,
                    ColorName = colorName,     // Save ColorName
                    CreatedOn = DateTime.Now,
                    CreatedBy = CreatedBy
                };

                _context.ProductVariants.Add(variant);
            }

            _context.SaveChanges();

            TempData["Success"] = "Variants added successfully!";
            return RedirectToAction("GetAllProductDetails");
        }



        [HttpGet]
        [Route("CheckSizeExists/{sizeId}/{productId}")]
        public JsonResult CheckSizeExists(int sizeId, int productId)
        {
            bool exists = _context.ProductVariants
                .Any(pv => pv.SizeId == sizeId && pv.ProductId == productId);

            return Json(new { exists });
        }
        [HttpGet]
        [Route("CheckColorExists/{colorId}/{productId}")]
        public JsonResult CheckColorExists(int colorId, int productId)
        {
            bool exists = _context.ProductVariants
                .Any(pv => pv.ColorId == colorId && pv.ProductId == productId);

            return Json(new { exists });
        }


        [HttpPost]
        [Route("DeleteVariant")]
        public IActionResult DeleteVariant(int productId, int sizeId, int colorId)
        {
            var variant = _context.ProductVariants
                .FirstOrDefault(pv => pv.ProductId == productId && pv.SizeId == sizeId && pv.ColorId == colorId);

            if (variant != null)
            {
                _context.ProductVariants.Remove(variant);
                _context.SaveChanges();
            }

            return RedirectToAction("AddVariants", new { id = productId });
        }

        [HttpGet]
        [Route("AddInventory")]
        public IActionResult AddInventory()
        {
            var products = _context.StoreProducts
                .Where(p => p.IsVisible == true)
                .Select(p => new StoreProductsModel
                {
                    Products_Id = p.Products_Id,
                    ProductName = p.ProductName,
                    Price = p.Price
                })
                .ToList();

            return View(products);
        }

        [HttpGet]
        [Route("GetInventoryItems")]
        public async Task<IActionResult> GetInventoryItems(int storeId)
        {
            var inventoryItems = await _context.ProductVariants
    .Where(i => i.ProductId == storeId)
    .Select(i => new
    {
        id=i.VariantId,
        ProductName = _context.StoreProducts
                             .Where(p => p.Products_Id == i.ProductId)
                             .Select(p => p.ProductName)
                             .FirstOrDefault(),

        SizeName = _context.StoreSize
                           .Where(s => s.IdSize == i.SizeId)
                           .Select(s => s.Name)
                           .FirstOrDefault(),

        colorName = _context.StoreColor.Where(c => c.IdColor == i.ColorId).Select(s => s.Name).FirstOrDefault(),

        i.Price,
        i.ProductGalleries,
        i.Quantity,
        i.IsVisible
    })
    .ToListAsync();


            return Json(inventoryItems);
        }
        [HttpPost]
        [Route("UpdateInventoryItems")]
        public async Task<IActionResult> UpdateInventoryItems([FromForm] List<ProductVariantsModel> inventoryItems)
        {
            if (inventoryItems == null || !inventoryItems.Any())
            {
                return Json(new { success = false, message = "No data received." });
            }

            try
            {
                string CreatedBy = null;
                if (User.Identity.IsAuthenticated)
                {
                    var emailClaim = User.Claims.FirstOrDefault(c =>
                        c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                    CreatedBy = emailClaim?.Value;
                }

                foreach (var item in inventoryItems)
                {
                    var existingItem = await _context.ProductVariants
                        .Include(p => p.ProductGalleries)
                        .FirstOrDefaultAsync(pv => pv.VariantId == item.Id);

                    if (existingItem != null)
                    {
                        // Update Quantity & Price
                        existingItem.Quantity = item.Quantity;
                        existingItem.Price = item.Price;

                        // Retain existing galleries
                        var existingGalleries = existingItem.ProductGalleries.ToList();

                        // Add new galleries if provided
                        if (item.ProductGalleries != null && item.ProductGalleries.Any())
                        {
                            int sortOrder = existingGalleries.Count + 1;

                            foreach (var file in item.ProductGalleries)
                            {
                                var imagePath = await SaveImage("Shop", "Galleries", file);

                                if (!string.IsNullOrEmpty(imagePath)) // Prevent null insert
                                {
                                    existingGalleries.Add(new StoreProductGallery
                                    {
                                        VariantId = existingItem.VariantId,
                                        ImagePath = imagePath,
                                        SortOrder = sortOrder++,
                                        CreatedBy = CreatedBy,
                                        CreatedOn = DateTime.Now
                                    });
                                }
                            }
                        }

                        existingItem.ProductGalleries = existingGalleries;

                        _context.ProductVariants.Update(existingItem);
                    }

                    int VariantId = existingItem.VariantId;
                    var detailsCartItems = await _context.StoreCartItem
                        .Where(c => c.VariantId == VariantId) // make sure item.Id is VariantId
                        .ToListAsync();

                    if (detailsCartItems.Any())
                    {
                        foreach (var cart in detailsCartItems)
                        {
                            // Update price correctly (Price * Quantity)
                            cart.TotalPrice = item.Price * cart.Quantity;

                            _context.StoreCartItem.Update(cart);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }








        [HttpGet]
        [Route("AddStoreSlider")]
        public async Task<IActionResult> AddStoreSlider()
        {

            var model = new StoreSliderModel();

            // Assuming _context.StoreSliders is your table
            var existingSliders = await _context.StoreSlider
      .OrderBy(s => s.SortOrder)
      .ToListAsync();

            ViewBag.ExistingSliders = existingSliders;
            var products = await _context.StoreProducts
              .Select(p => new SelectListItem
              {
                  Value = p.Products_Id.ToString(),
                  Text = p.ProductName
              })
              .ToListAsync();

            model.Products = products;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSliderImage(StoreSliderModel model, List<IFormFile> desktopImage, List<IFormFile> mobileImage)
        {
            if ((desktopImage == null || !desktopImage.Any()) || (mobileImage == null || !mobileImage.Any()))
            {
                ModelState.AddModelError("", "Please upload both desktop and mobile images.");
                return View(model);
            }
            string CreatedBy = null;
            if (User.Identity.IsAuthenticated)
            {
                var emailClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                CreatedBy = emailClaim?.Value;
            }
            int lastSortOrder = 0;
            if (_context.StoreSlider.Any())
            {
                lastSortOrder = _context.StoreSlider.Max(s => s.SortOrder);
            }
            // Ensure both lists are of equal length
            int count = Math.Max(desktopImage.Count, mobileImage.Count);

            for (int i = 0; i < count; i++)
            {
                string desktopImageName = i < desktopImage.Count ? await SaveImage("Shop", "Storeslider", desktopImage[i]) : null;
                string mobileImageName = i < mobileImage.Count ? await SaveImage("Shop", "Storeslider", mobileImage[i]) : null;




                var slider = new StoreSlider
                {
                    Title = model.Title,
                    Product_Id = model.ProductId,
                    DesktopImagePath = desktopImageName,
                    MobileImagePath = mobileImageName,
                    CreatedOn = DateTime.Now,
                    CreatedBy = CreatedBy,
                    SortOrder = lastSortOrder + 1 // continue from last sort order
                };

                _context.StoreSlider.Add(slider);
                lastSortOrder++; // increment for next image
            }



            await _context.SaveChangesAsync();
            TempData["Success"] = "Slider images uploaded successfully!";
            return RedirectToAction("AddStoreSlider");// or wherever you want
        }


        [HttpPost]
        [Route("UpdateVisibilityProduct")]
        public IActionResult UpdateVisibilityProduct(int id)
        {
            var products = _context.StoreProducts.FirstOrDefault(p => p.Products_Id == id);
            if (products == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            // Toggle visibility
            products.IsVisible = !products.IsVisible;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                isVisible = products.IsVisible,
                message = products.IsVisible ? "Product enabled successfully." : "Product disabled successfully."
            });
        }

        [HttpPost]
        [Route("UpdateVisibilitySize")]
        public IActionResult UpdateVisibilitySize(int id)
        {
            var size = _context.StoreSize.FirstOrDefault(p => p.IdSize == id);
            if (size == null)
            {
                return Json(new { success = false, message = "size not found." });
            }

            // Toggle visibility
            size.IsVisible = !size.IsVisible;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                isVisible = size.IsVisible,
                message = size.IsVisible ? "Size enabled successfully." : "Size disabled successfully."
            });
        }
       


             [HttpPost]
        [Route("UpdateVisibilityColor")]
        public IActionResult UpdateVisibilityColor(int id)
        {
            var color = _context.StoreColor.FirstOrDefault(p => p.IdColor == id);
            if (color == null)
            {
                return Json(new { success = false, message = "Color not found." });
            }

            // Toggle visibility
            color.IsVisible = !color.IsVisible;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                isVisible = color.IsVisible,
                message = color.IsVisible ? "Color enabled successfully." : "Color disabled successfully."
            });
        }


        [HttpPost]
        [Route("UpdateVisibilityCategory")]
        public JsonResult UpdateVisibilityCategory(int id)
        {
            var category = _context.StoreCategory.FirstOrDefault(p => p.Category_Id == id);
            if (category == null)
            {
                return Json(new { success = false, message = "Category not found." });
            }

            // Toggle visibility
            category.IsVisible = !category.IsVisible;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                isVisible = category.IsVisible,
                message = category.IsVisible ? "Category enabled successfully." : "Category disabled successfully."
            });
        }




        [HttpPost]
        [Route("UpdateVisibilityProductVariants")]
        public IActionResult UpdateVisibilityProductVariants(int id)
        {
            var PVariants = _context.ProductVariants.FirstOrDefault(p => p.VariantId == id);
            if (PVariants == null)
            {
                return NotFound();
            }

            // Toggle the visibility
            PVariants.IsVisible = !PVariants.IsVisible;
            _context.SaveChanges();

            TempData["Success"] = PVariants.IsVisible ? "Product enabled successfully." : "Product disabled successfully.";
            return RedirectToAction("StoreAddCategories"); // Replace with your actual view name
        }



        [HttpPost]
        [Route("UpdateVisibilityVariants")]
        public JsonResult UpdateVisibilityVariants(int id, bool isVisible)
        {
            var item = _context.ProductVariants.FirstOrDefault(x => x.VariantId == id);
            if (item == null)
            {
                return Json(new { success = false });
            }

            item.IsVisible = isVisible;
            _context.SaveChanges();

            return Json(new { success = true, newVisibility = isVisible });
        }

        [HttpPost]
        [Route("DeleteSliderImage")]
        public async Task<JsonResult> DeleteSliderImage(int id, string imageType)
        {
            var image = await _context.StoreSlider.FindAsync(id);
            if (image == null)
                return Json(new { success = false, message = "Image not found" });

            // Optionally: Delete the physical file if you have its path
            // string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", image.ImageName);
            // if (System.IO.File.Exists(imagePath))
            // {
            //     System.IO.File.Delete(imagePath);
            // }

            // Remove the record from the database
            _context.StoreSlider.Remove(image);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        [HttpPost]
        [Route("DeleteGalleryImage")]
        public JsonResult DeleteGalleryImage(int id)
        {
            try
            {
                var image = _context.StoreProductGallery.FirstOrDefault(g => g.Id == id);
                if (image != null)
                {
                    _context.StoreProductGallery.Remove(image);
                    _context.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Image not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Route("Orders")]
        public async Task<IActionResult> Orders()
        {
            var model = await (
                from sp in _context.StoreParticipants
                join p in _context.StoreProducts on sp.ProductId equals p.Products_Id into productGroup
                from product in productGroup.DefaultIfEmpty()

                join o in _context.RazorPayOrderDetails on sp.BookingId equals o.BookingId into OrderIdGroup
                from OrderId in OrderIdGroup.DefaultIfEmpty()

                join v in _context.ProductVariants on sp.VariantId equals v.VariantId into variantGroup
                from variant in variantGroup.DefaultIfEmpty()

                join u in _context.Users on sp.UserId equals u.Id into userGroup
                from user in userGroup.DefaultIfEmpty()

                where OrderId.PaymentStatus == "Paid" && sp.Status == "Processing"
                orderby sp.OrderDate descending
                select new OrderDetailsViewModel
                {
                    Id = OrderId.Id,
                    BookingId = sp.BookingId,
                    UserId = sp.UserId,
                    FirstName = sp.FirstName,
                    LastName = sp.LastName,
                    PhoneNo = user.PhoneNumber,
                    Email = sp.Email,
                    AddressId = sp.AddressId,
                    OrderDate = sp.OrderDate,
                    Quantity = sp.Quantity,
                    ProductName = product.ProductName,
                    Color = variant.ColorName ?? "N/A",
                    Size = variant.SizeName ?? "N/A",
                    PaymentStatus = sp.PaymentStatus,
                    Amount = sp.TotalPrice,
                    Status = sp.Status
                }
            ).ToListAsync();

            return View(model);
        }
        [HttpGet]
        [Route("_SearchResults")]
        public IActionResult _SearchResults()
        {
            return View();
        }

        [HttpGet]
        [Route("SearchOrders")]
        public async Task<IActionResult> SearchOrders(DateTime? dateFrom, DateTime? dateTo)
        {
            var query = from sp in _context.StoreParticipants
                        join p in _context.StoreProducts
                            on sp.ProductId equals p.Products_Id into productGroup
                        from product in productGroup.DefaultIfEmpty()

                        join o in _context.RazorPayOrderDetails
                              on sp.BookingId equals o.BookingId into OrderIdGroup
                        from OrderId in OrderIdGroup.DefaultIfEmpty()

                        join v in _context.ProductVariants
                            on sp.VariantId equals v.VariantId into variantGroup
                        from variant in variantGroup.DefaultIfEmpty()

                        join u in _context.Users
                            on sp.UserId equals u.Id into userGroup
                        from user in userGroup.DefaultIfEmpty()

                        where sp.PaymentStatus == "Paid"
                              && (!dateFrom.HasValue || sp.OrderDate >= dateFrom.Value)
                              && (!dateTo.HasValue || sp.OrderDate <= dateTo.Value)

                        orderby sp.OrderDate descending
                        select new OrderDetailsViewModel
                        {
                            Id = OrderId.Id,
                            UserId = sp.UserId,
                            BookingId = sp.BookingId,
                            FirstName = sp.FirstName,
                            LastName = sp.LastName,
                            PhoneNo = user.PhoneNumber,   // assuming User has PhoneNumber
                            Email = sp.Email,
                            AddressId = sp.AddressId,
                            OrderDate = sp.OrderDate,
                            Quantity = sp.Quantity,
                            ProductName = product != null ? product.ProductName : "N/A",
                            Color = variant != null ? variant.ColorName : "N/A",
                            Size = variant != null ? variant.SizeName : "N/A",
                            PaymentStatus = sp.PaymentStatus,
                            Amount = sp.TotalPrice,
                            Status=sp.Status
                        };

            var model = await query.ToListAsync();

            return PartialView("_SearchResults", model);
        }

        

        [HttpPost]
        [Route("SearchTrekker")]
        public async Task<IActionResult> SearchTrekker(string trekkerName)
        {
            if (string.IsNullOrEmpty(trekkerName))
            {
                return PartialView("_SearchResults", new List<StoreParticipants>());
            }

            var participants = _context.StoreParticipants
     .Where(ci => (ci.FirstName + " " + ci.LastName).Contains(trekkerName)
                  || ci.Email.Contains(trekkerName))
     .ToList();


            var model = new List<OrderDetailsViewModel>();

            foreach (var item in participants)
            {
                var productName = await _context.StoreProducts
                    .Where(p => p.Products_Id == item.ProductId)
                    .Select(p => p.ProductName)
                    .FirstOrDefaultAsync();

                var variant = await _context.ProductVariants
                    .Where(v => v.VariantId == item.VariantId)
                    .Select(v => new { v.ColorName, v.SizeName })
                    .FirstOrDefaultAsync();

                var phoneNo = await _context.Users
                    .Where(u => u.Id == item.UserId)
                    .Select(u => u.PhoneNumber)
                    .FirstOrDefaultAsync();

                model.Add(new OrderDetailsViewModel
                {
                    Id = item.Id,
                    UserId = item.UserId,
                    BookingId = item.BookingId,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    PhoneNo = phoneNo,
                    Email = item.Email,
                    AddressId = item.AddressId,
                    OrderDate = item.OrderDate,
                    Quantity = item.Quantity,
                    ProductName = productName,
                    Color = variant?.ColorName ?? "N/A",
                    Size = variant?.SizeName ?? "N/A",
                    PaymentStatus = item.PaymentStatus,
                    Amount = item.TotalPrice
                });
            }

            return PartialView("_SearchResults", model);
        }
        [HttpGet]
        [Route("UserProductDetails/{bookingId}")]
        public async Task<IActionResult> UserProductDetails(string bookingId)
        {
            var orderDetails = await _context.StoreParticipants
                                             .Where(o => o.BookingId == bookingId)
                                             .ToListAsync();
            RazorPayModel orders = null;
            StoreUserAddress address = null;
            if (orderDetails.Any())
            {
                var firstAddressId = orderDetails.First().AddressId;
                address = await _context.StoreUserAddress
                                        .FirstOrDefaultAsync(a => a.Id == firstAddressId);

                orders = await _context.RazorPayOrderDetails.FirstOrDefaultAsync(a => a.BookingId == bookingId);
            }

            var orderItems = await (from sp in _context.StoreParticipants
                                    where sp.BookingId == bookingId
                                    join v in _context.ProductVariants
                                        on sp.VariantId equals v.VariantId into variantGroup
                                    from variant in variantGroup.DefaultIfEmpty()
                                    let gallery = _context.StoreProductGallery
                                                          .Where(g => g.VariantId == sp.VariantId)
                                                          .Select(g => g.ImagePath)
                                                          .FirstOrDefault()
                                    select new AdminOrderItemViewModel
                                    {
                                         ProductName= sp.ProductName,
                                        ColorName = variant.ColorName,
                                        SizeName = variant.SizeName,
                                        ImagePath = gallery,
                                        Quantity = sp.Quantity,
                                        PricePerDay = sp.TotalPrice,
                                        OrderDate = sp.OrderDate,
                                        Status = sp.Status
                                    }).ToListAsync();

            var viewModel = new AdminOrderDetailsViewModel
            {
                OrderItems = orderItems,
                Address = address,
                BookingId = bookingId,
                Orders = orders,
            };

            return View(viewModel);
        }



        [HttpGet]
        [Route("UserDetailsPartial")]
        public IActionResult UserDetailsPartial()
        {
            return PartialView();
        }

        [HttpGet]
        [Route("GetUserDetails/{bookingId}")]
        public async Task<IActionResult> GetUserDetails(string bookingId)
        {
            var orderDetails = await _context.StoreParticipants
                                             .Where(o => o.BookingId == bookingId)
                                             .ToListAsync();
            RazorPayModel orders = null;
            StoreUserAddress address = null;
            if (orderDetails.Any())
            {
                var firstAddressId = orderDetails.First().AddressId;
                address = await _context.StoreUserAddress
                                        .FirstOrDefaultAsync(a => a.Id == firstAddressId);
                orders = await _context.RazorPayOrderDetails.FirstOrDefaultAsync(a => a.BookingId == bookingId);
            }

            var orderItems = await (from sp in _context.StoreParticipants
                                    where sp.BookingId == bookingId
                                    join v in _context.ProductVariants
                                        on sp.VariantId equals v.VariantId into variantGroup
                                    from variant in variantGroup.DefaultIfEmpty()
                                    let gallery = _context.StoreProductGallery
                                                          .Where(g => g.VariantId == sp.VariantId)
                                                          .Select(g => g.ImagePath)
                                                          .FirstOrDefault()
                                    select new AdminOrderItemViewModel
                                    {
                                        ProductName = sp.ProductName,
                                        ColorName = variant.ColorName,
                                        SizeName = variant.SizeName,
                                        ImagePath = gallery,
                                        Quantity = sp.Quantity,
                                        PricePerDay = sp.TotalPrice,
                                        OrderDate = sp.OrderDate,
                                        Status = sp.Status
                                    }).ToListAsync();

            var viewModel = new AdminOrderDetailsViewModel
            {
                OrderItems = orderItems,
                Address = address,
                BookingId = bookingId,
                Orders = orders,
            };

            return PartialView("UserDetailsPartial", viewModel);
        }
        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(string bookingId, string status)
        {
            if (string.IsNullOrEmpty(bookingId) || string.IsNullOrEmpty(status))
                return BadRequest("BookingId and Status are required");

            var participants = await _context.StoreParticipants
                                             .Where(o => o.BookingId == bookingId)
                                             .ToListAsync();

            if (!participants.Any())
                return NotFound("No participants found for this booking");

            foreach (var p in participants)
            {
                p.Status = status;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Status updated successfully" });
        }
        [HttpPost]
        [Route("UpdateSliderOrder")]
        public IActionResult UpdateSliderOrder([FromBody] List<int> ids)
        {
            try
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    int sliderId = ids[i];
                    var slider = _context.StoreSlider.FirstOrDefault(s => s.Id == sliderId);
                    if (slider != null)
                    {
                        slider.SortOrder = i; // update order
                    }
                }

                _context.SaveChanges();
                return Ok(new { success = true, message = "Sort order updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
