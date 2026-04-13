using DocumentFormat.OpenXml.Office2021.Drawing.Livefeed;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using NPOI.SS.Formula.Functions;
using Razorpay.Api;
using sib_api_v3_sdk.Client;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TTH.Areas.Super.Controllers;
using TTH.Areas.Super.Data;
using TTH.Areas.Super.Data.Rent;
using TTH.Areas.Super.Data.TrekkersStore;
using TTH.Areas.Super.Models;
using TTH.Areas.Super.Models.TrekkersStore;
using TTH.Areas.Super.Repository;
using TTH.Models;
using TTH.Models.booking;
using TTH.Models.Rent;
using TTH.Models.user;
using TTH.Service;
using TTH.uirepository;

namespace TTH.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly AppDataContext _context;
        private readonly StoreAdminRepository _storeAdminRepository;
        private readonly IGenerateRentBookingId _generateRentBookingId;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRazorpayService razorpayService;
        private readonly IBravoMail _bravoMail;
        public StoreController(StoreAdminRepository storeAdminRepository, AppDataContext context, UserManager<ApplicationUser> userManager, IGenerateRentBookingId generateRentBookingId, IRazorpayService razorpayService, IBravoMail bravoMail)
        {
            _storeAdminRepository = storeAdminRepository;
            _userManager = userManager;
            _generateRentBookingId = generateRentBookingId;
            _context = context;
            this.razorpayService = razorpayService;
            _bravoMail = bravoMail;
        }

        [HttpGet("GetSlider")]
        public async Task<IActionResult> GetSlider()
        {
            var sliders = await _context.StoreSlider
      .OrderBy(s => s.SortOrder)
      .ToListAsync();
            return Ok(new
            {
                status = true,
             
                data = sliders
            });
        }

        [HttpGet("AllCategories")]
        public async Task<IActionResult> AllCategories()
        {
            var categories = await _context.StoreCategory.Where(e => e.IsVisible).ToListAsync();
            return Ok(new
            {
                status = true,
                message = "Variant data fetched successfully.",
                data = categories
            });
        }


        [HttpGet("AllProducts")]
        public async Task<IActionResult> AllProducts()
        {

            var allProducts = await _context.StoreProducts.Where(e => e.IsVisible)

                .Select(g => new
                {
                    ProductId = g.Products_Id,
                    Name = g.ProductName,
                    Price = g.Price,
                    CoverImage = g.CoverImgUrl
                })
                .ToListAsync();
            return Ok(new
            {
                status = true,
                message = "Variant data fetched successfully.",
                data = allProducts
            });
        }

        [HttpGet("ProductByCategory/{categoryId}")]
        public async Task<IActionResult> ProductByCategory(int categoryId)
        {
            var result = await _context.StoreProducts

                .Where(p => p.CategoryId == categoryId && p.IsVisible)
                .Select(p => new
                {
                    ProductId = p.Products_Id,
                    Name = p.ProductName,
                    Price = p.Price,
                    CoverImage = p.CoverImgUrl

                })
                .ToListAsync();

            if (!result.Any())
            {
                return Ok(new
                {
                    status = false,
                    message = "No products found for this category."
                });
            }

            return Ok(new
            {
                status = true,
              
                data = result
            });
        }

        [HttpGet("GetVarientBySizeAndColor/{ProductId}/{SizeId}/{ColorId}")]
        public async Task<IActionResult> GetVarientBySizeAndColor(int ProductId, int SizeId, int ColorId)
        {

            var variant = await _context.ProductVariants
                .Include(p => p.StoreProduct)
                .Where(p => p.ProductId == ProductId && p.SizeId == SizeId && p.ColorId == ColorId && p.Quantity > 0 && p.IsVisible)
                .Select(p => new
                {
                    price = p.Price,
                    quantity = p.Quantity,
                    VariantId = p.VariantId,
                    ProductId = p.ProductId,
                    productName = p.StoreProduct.ProductName,
                    description = p.StoreProduct.Description,
                    Galleries = p.ProductGalleries

                })
                .FirstOrDefaultAsync();
            if (variant == null)
            {
                return Ok(new
                {
                    status = false,
                    message = "Product variant not found."
                });

            }

            return Ok(new
            {
                status = true,
                data = variant
            });
        }

        [HttpGet("GetSizes/{Id}")]
        public async Task<IActionResult> GetSizes(int Id)
        {
            var sizes = await _context.ProductVariants
                .Include(p => p.StoreSize)
                .Where(p => p.ProductId == Id)
                .GroupBy(p => new { p.SizeId, SizeName = p.StoreSize.Name, p.IsVisible })
                .Select(g => new
                {
                    productId = Id,
                    sizeId = g.Key.SizeId,
                    sizeName = g.Key.SizeName
                })
                .ToListAsync();
            if (!sizes.Any())
            {
                return Ok(new
                {
                    status = false,
                    message = "No sizes found for this product."
                });
            }
            return Ok(new
            {
                status = true,
                data = sizes
            });
        }

        [HttpGet("GetColors/{Id}")]
        public async Task<IActionResult> GetColors(int Id)
        {

            var colors = await _context.ProductVariants
    .Include(p => p.StoreColor)
    .Where(p => p.ProductId == Id && p.IsVisible)
    .GroupBy(p => new { p.ColorId, ColorName = p.StoreColor.Name })
    .Select(g => new
    {
        productId = Id,
        colorId = g.Key.ColorId,
        colorName = g.Key.ColorName
    })
    .ToListAsync();

            if (!colors.Any())
            {
                return Ok(new
                {
                    status = false,
                    message = "No colors found for this product."
                });
            }
            return Ok(new
            {
                status = true,
                data = colors
            });
        }

        [HttpPost("AddToCart/{productId}/{quantity}/{variantId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AddToCart(int productId, int quantity, int variantId)
        {
            var userId = User.FindFirstValue("userid");

            if (userId == null)
            {

                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Unauthorized();
            }

            var userEmail = user.Email;
            // Use userEmail as needed

            string bookingId = await _generateRentBookingId.GetStoreBookingId(userId);
            if (string.IsNullOrEmpty(bookingId))
            {
                return Ok(new
                {
                    status = false,
                  
                       message = "Booking Id is coming null.",
                  
            });
            }

            decimal totalPrice = await _context.ProductVariants
      .Where(p => p.VariantId == variantId)
      .Select(p => p.Price)
      .FirstOrDefaultAsync();


            var productName = await _context.StoreProducts
      .Where(p => p.Products_Id == productId)
      .Select(p => p.ProductName)
      .FirstOrDefaultAsync();

            var existingCartItem = await _context.StoreCartItem
 .Where(c => c.UserId == userId
             && c.ProductId == productId
             && c.VariantId == variantId)

 .FirstOrDefaultAsync();


            if (existingCartItem != null)
            {

                existingCartItem.Quantity += quantity;
                existingCartItem.TotalPrice = existingCartItem.Quantity * totalPrice;

                _context.StoreCartItem.Update(existingCartItem);
            }
            else
            {

                var cartItem = new StoreCartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    Quantity = quantity,
                    TotalPrice = quantity * totalPrice,

                    UserId = userId,

                    Email = userEmail,

                    OrderDate = DateTime.Now,
                    VariantId = variantId,
                    BookingId = bookingId,


                };

                _context.StoreCartItem.Add(cartItem);
            }

            await _context.SaveChangesAsync();



            var cartItems = await _context.StoreCartItem
        .Where(c => c.BookingId == bookingId)
        .ToListAsync();

            var totalCost = cartItems.Sum(item => item.TotalPrice);
            var totalcount = cartItems.Count();
            var result = cartItems.Select(item => new
            {
                productId = item.ProductId,
                Productname = item.ProductName,
                varientId = item.VariantId,
                quantity = item.Quantity,
                totalPrice = item.TotalPrice,
                bookingId = bookingId,
                UserId = item.UserId

            }).ToList();

            decimal gstRate = 5m;
            decimal gstPrice = totalCost * gstRate / 100;
            int gstRounded = (int)Math.Round(gstPrice, 0);
            decimal totalPricewithGST = totalCost + gstRounded;

            return Ok(new StoreCartApiResponseModel
            {
                status = true,
                Message = "CartUpdated",
                Data = result,
                TotalCost = totalPricewithGST,
                ProductCount = totalcount,
            });

        }

        [HttpGet("AllInventory/{productId}")]
        public async Task<IActionResult> AllInventory(int productId)
        {
            var variants = await _context.ProductVariants
                .Include(pv => pv.StoreProduct)
             
                .Where(pv => pv.StoreProduct.Products_Id == productId)
                .Select(pv => new
                {
                    price =pv.Price,

                    quantity = pv.Quantity,
                    VariantId = pv.VariantId,
                    ProductId = pv.StoreProduct.Products_Id,
                    productName = pv.StoreProduct.ProductName,
                    description = pv.StoreProduct.Description,
                    galleries = pv.StoreProduct.CoverImgUrl
                })
                .ToListAsync();

            if (variants == null || !variants.Any())
            {
                return Ok(new
                {
                    status = false,
                    message = "No inventory found for the given product ID."
                });
            }

            return Ok(new
            {
                status = true,
                message = "Inventory data fetched successfully.",
                data = variants
            });
        }


        //[Route("CreateBooking/{bookingId}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public async Task<IActionResult> CreateBooking(string bookingId)
        //{
        //    // Retrieve all cart items for the given bookingId
        //    var storeCartItems = await _context.StoreCartItem
        //        .Where(c => c.BookingId == bookingId)
        //        .Select(c => new
        //        {
        //            c.Email,
        //            c.ProductId,
        //            c.ProductName,
        //            c.VariantId,
        //            c.Quantity,
        //            c.TotalPrice,
        //            c.UserId,
        //            c.BookingId
        //        })
        //        .ToListAsync();

        //    // Fallback: Check StoreParticipants if no cart items found
        //    if (storeCartItems == null || !storeCartItems.Any())
        //    {
        //        var storeItems = await _context.StoreParticipants
        //            .Where(c => c.BookingId == bookingId && c.PaymentStatus == "Unpaid")
        //            .Select(c => new
        //            {
        //                c.ProductId,
        //                c.ProductName,
        //                c.Quantity,
        //                c.VariantId,
        //                c.TotalPrice,
        //                c.UserId,
        //                c.Email,
        //                c.OrderDate
        //            })
        //            .ToListAsync();

        //        return Ok(new { success = false, message = "No cart items found for the given BookingId." });
        //    }

        //    // Remove existing participant entries with the same booking ID
        //    var existingParticipants = await _context.StoreParticipants
        //        .Where(r => r.BookingId == bookingId)
        //        .ToListAsync();

        //    if (existingParticipants.Any())
        //    {
        //        _context.StoreParticipants.RemoveRange(existingParticipants);
        //    }

        //    decimal totalAmount = 0;
        //    string userId = "";

        //    foreach (var item in storeCartItems)
        //    {
        //        var participant = new StoreParticipants
        //        {
        //            UserId = item.UserId,
        //            Email = item.Email,
        //            TotalPrice = item.TotalPrice,
        //            ProductName = item.ProductName,
        //            ProductId = item.ProductId,
        //            BookingId = bookingId,
        //            PaymentStatus = "Unpaid",
        //            Quantity = item.Quantity,
        //            VariantId = item.VariantId,
        //            OrderDate = DateTime.Now
        //        };

        //        totalAmount += item.TotalPrice;
        //        userId = item.UserId;

        //        _context.StoreParticipants.Add(participant);
        //    }

        //    // Generate transaction ID and create Razorpay order
        //    IdGenerator idGenerator = new IdGenerator(_context);
        //    string transactionId = idGenerator.GenerateTransactionId();
        //    string orderId = await razorpayService.CreateOrder(totalAmount, "INR", transactionId);

        //    var razorpayOrder = new RazorPayModel
        //    {
        //        RazorpayOrderId = orderId,
        //        TransactionId = transactionId,
        //        Amount = totalAmount,
        //        BookingId = bookingId,
        //        PaymentStatus = "Unpaid",
        //        CreatedDate = DateTime.Now,
        //        PaymentSource = "Shop",

        //    };

        //    _context.RazorPayOrderDetails.Add(razorpayOrder);

        //    // Update order date for all cart items
        //    var cartItemsToUpdate = await _context.StoreCartItem
        //        .Where(b => b.BookingId == bookingId)
        //        .ToListAsync();

        //    foreach (var item in cartItemsToUpdate)
        //    {
        //        item.OrderDate = DateTime.Now;
        //    }

        //    await _context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        success = true,
        //        OrderId = orderId,
        //        TransactionId = transactionId,
        //        Amount = totalAmount,
        //        BookingId = bookingId,
        //        UserId = userId
        //    });
        //}




        [HttpPost]
        [Route("CreateBooking/{bookingId}/{addressid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateBooking(string bookingId, int addressid)
        {
            var storeCartItems = await _context.StoreCartItem
                .Where(c => c.BookingId == bookingId)
                .Select(c => new
                {
                    c.Email,
                    c.ProductId,
                    c.ProductName,
                    c.VariantId,
                    c.Quantity,
                    c.TotalPrice,
                    c.UserId,
                    c.BookingId
                })
                .ToListAsync();


            if (storeCartItems == null || !storeCartItems.Any())
            {
                return Ok(new { status = false, message = "No cart items found for the given BookingId." });
            }

            // Remove existing participant entries with the same booking ID
            var existingParticipants = await _context.StoreParticipants
                .Where(r => r.BookingId == bookingId)
                .ToListAsync();

            if (existingParticipants.Any())
            {
                _context.StoreParticipants.RemoveRange(existingParticipants);
            }

            decimal totalAmount = 0;
            decimal totalPricewithGST = 0;
            int gstRounded = 0;
            string userId = storeCartItems.First().UserId;

            // Fetch user name once instead of querying inside the loop
            var userInfo = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.FirstName, u.LastName })
                .FirstOrDefaultAsync();

            foreach (var item in storeCartItems)
            {
                var participant = new StoreParticipants
                {
                    UserId = item.UserId,
                    Email = item.Email,
                    TotalPrice = item.TotalPrice,
                    ProductName = item.ProductName,
                    ProductId = item.ProductId,
                    BookingId = bookingId,
                    PaymentStatus = "Unpaid",
                    Quantity = item.Quantity,
                    VariantId = item.VariantId,
                    OrderDate = DateTime.Now,
                    AddressId = addressid,
                    FirstName = userInfo?.FirstName,
                    LastName = userInfo?.LastName,
                    Status = "Processing"
                };

                totalAmount += item.TotalPrice;

                _context.StoreParticipants.Add(participant);
            }


         

            // Generate transaction ID and create Razorpay order
            IdGenerator idGenerator = new IdGenerator(_context);
            string transactionId = idGenerator.GenerateTransactionId();
            string orderId = await razorpayService.CreateOrder(totalPricewithGST, "INR", transactionId);

            var razorpayOrder = new RazorPayModel
            {
                RazorpayOrderId = orderId,
                TransactionId = transactionId,
                Amount = totalAmount,
                BookingId = bookingId,
                PaymentStatus = "Unpaid",
                CreatedDate = DateTime.Now,
                PaymentSource = "Shop",

            };

            _context.RazorPayOrderDetails.Add(razorpayOrder);

            // Update order date for all cart items
            var cartItemsToUpdate = await _context.StoreCartItem
                .Where(b => b.BookingId == bookingId)
                .ToListAsync();

            foreach (var item in cartItemsToUpdate)
            {
                item.OrderDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = true,
                OrderId = orderId,
                totalPrice = totalAmount,
               
                BookingId = bookingId,
                UserId = userId,
                AddressId = addressid
            });
        }


        private string GenerateRazorpaySignature(string orderId, string paymentId)
        {
            // Use your Razorpay secret key to generate the signature
           // string secret = "R1BDrYjGpWnD46sH1P6Li2y1"; //testkey
            string secret = "Hctq0JOfkx8ziVyXzoZKzaxI"; //LiveKey
           
            string payload = orderId + "|" + paymentId;

            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        [HttpPost]
        [Route("HandlePaymentSuccess")]
        public async Task<IActionResult> HandlePaymentSuccess([FromBody] HandlePaymentDtoStore dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Verify Signature
                string generatedSignature = GenerateRazorpaySignature(dto.razorpay_order_id, dto.razorpay_payment_id);
                if (!string.Equals(generatedSignature, dto.razorpay_signature, StringComparison.Ordinal))
                {
                    return Ok(new { status = false, message = "Invalid payment signature." });
                }

                // 2. Update Razorpay Order
                var razorOrder = await _context.RazorPayOrderDetails
                    .FirstOrDefaultAsync(r => r.RazorpayOrderId == dto.razorpay_order_id);

                if (razorOrder != null)
                {
                    razorOrder.RazorpayPaymentId = dto.razorpay_payment_id;
                    razorOrder.PaymentStatus = "Paid";
                }

                // 3. Update StoreParticipants Table & Inventory
                var participants = await _context.StoreParticipants
                    .Where(r => r.BookingId == dto.bookingId)
                    .ToListAsync();

                if (!participants.Any())
                {
                    await transaction.RollbackAsync();
                    return Ok(new { status = false, message = "Booking records not found." });
                }
                
                // If they are already paid (prevent double processing)
                if (participants.All(p => p.PaymentStatus == "Paid"))
                {
                    await transaction.RollbackAsync();
                    return Ok(new { status = true, message = "Payment already processed.", bookingId = dto.bookingId });
                }

                foreach (var p in participants)
                {
                    p.PaymentStatus = "Paid";
                    p.OrderDate = DateTime.Now;

                    // Update Inventory Quantity
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.VariantId == p.VariantId && v.ProductId == p.ProductId);
                    
                    if (variant != null)
                    {
                        variant.Quantity -= p.Quantity;
                        if (variant.Quantity < 0) variant.Quantity = 0; // Prevent negative inventory
                        _context.ProductVariants.Update(variant);
                    }
                }

                // 4. Clean up Cart
                var cartItems = await _context.StoreCartItem
                    .Where(c => c.BookingId == dto.bookingId)
                    .ToListAsync();

                if (cartItems.Any())
                {
                    _context.StoreCartItem.RemoveRange(cartItems);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 5. Send order confirmation email
                try
                {
                    var firstParticipant = participants.FirstOrDefault();
                    if (firstParticipant != null && !string.IsNullOrEmpty(firstParticipant.Email))
                    {
                        string senderName = "Trek the Himalayas";
                        string senderEmail = "info@trekthehimalayas.com";
                        string orderedItems = string.Join(", ", participants.Select(p => $"{p.ProductName ?? "Item"} (x{p.Quantity})"));
                        decimal totalAmount = razorOrder?.Amount ?? participants.Sum(p => p.TotalPrice);
                        string trekkerName = !string.IsNullOrWhiteSpace(firstParticipant.FirstName) 
                            ? $"{firstParticipant.FirstName} {firstParticipant.LastName}".Trim() 
                            : "Trekker";

                        JObject Params = new JObject
                        {
                            { "trekker_name", trekkerName },
                            { "trekker_email", firstParticipant.Email },
                            { "order_date", DateTime.Now.ToString("dd-MMM-yyyy") },
                            { "ordered_items", orderedItems },
                            { "orderid", dto.razorpay_order_id ?? "" },
                            { "total_amount", totalAmount.ToString("0.00") },
                            { "status", "Paid" }
                        };

                        // Client Email
                        _bravoMail.SendEmail(senderName, senderEmail, firstParticipant.Email, trekkerName, 363, Params);

                        // Admin Email
                        string adminEmail = "Store@trekthehimalayas.com";
                        _bravoMail.SendEmail(senderName, senderEmail, adminEmail, "TTH Team", 340, Params);
                    }
                }
                catch (Exception ex)
                {
                    // Log email error if necessary, but don't fail the payment transaction
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }

                return Ok(new { status = true, message = "Payment successful.", bookingId = dto.bookingId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { status = false, message = "Internal error processing payment." });
            }
        }


        //store @trekthehimalayas.com



        [HttpGet]
        [Route("CartItems")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CartItems()
        {
            var userId = User.FindFirstValue("userid");
         
            var cartItems = await _context.StoreCartItem
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems == null || !cartItems.Any())
            {
                return Ok(new
                {
                    status = false,
                    message = "CartItem is null.",
                   
                });
            }

            var productIds = cartItems.Select(c => c.ProductId).Distinct().ToList();
            var variantsIds = cartItems.Select(c => c.VariantId).Distinct().ToList();

            var products = await _context.StoreProducts
                .Where(p => productIds.Contains(p.Products_Id))
                .ToDictionaryAsync(p => p.Products_Id, p => p.CoverImgUrl);

            var varinatsGallery = await _context.StoreProductGallery
     .Where(p => variantsIds.Contains(p.VariantId))
     .GroupBy(p => p.VariantId)
     .Select(g => new
     {
         VariantId = g.Key,
         ImagePath = g.OrderBy(p => p.Id).Select(p => p.ImagePath).FirstOrDefault() // or any ordering logic you prefer
     })
     .ToDictionaryAsync(x => x.VariantId, x => x.ImagePath);

            var variantDetails = await _context.ProductVariants
.Where(p => variantsIds.Contains(p.VariantId))
.ToDictionaryAsync(p => p.VariantId, p => new
{
p.SizeName,
p.ColorName
});
            var itemViewModels = cartItems.Select(cart => new StoreCartItemViewModel
            {
                VariantId = cart.VariantId,
                ProductId = cart.ProductId,
                ProductName = cart.ProductName,
                Quantity = cart.Quantity,
                Price = cart.TotalPrice,
                bookingid=cart.BookingId,
 


                SizeName = variantDetails.ContainsKey(cart.VariantId)
? variantDetails[cart.VariantId].SizeName
: null,
                ColorName = variantDetails.ContainsKey(cart.VariantId)
? variantDetails[cart.VariantId].ColorName
: null,
                //ImageUrl = products.ContainsKey(cart.ProductId) ? products[cart.ProductId] : null,
                ImageUrl = varinatsGallery.ContainsKey(cart.VariantId) ? varinatsGallery[cart.VariantId] : null
            }).ToList();


            var grandTotal = itemViewModels.Sum(item => item.Price);

            decimal gstRate = 5m;
            decimal gstPrice = grandTotal * gstRate / 100;
            int gstRounded = (int)Math.Round(gstPrice, 0);
            decimal totalPricewithGST = grandTotal + gstRounded;

            var totalCount = cartItems.Count;
            var response = new StoreCartItemsResponseViewModel
            {
                Items = itemViewModels,
                GstTotalPrice = totalPricewithGST,
                GstPrice= gstRounded,
                totalPrice= grandTotal,
                ProductCount = totalCount
            };

            return Ok(new
            {
                status = true,
               
                data = response
            });
        }


        [HttpGet("UserDeatils")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UserDeatils()
        {

            var userId = User.FindFirstValue("userid");

            if (userId == null)
            {

                return Ok(new
                {
                    status = false,
                    message = "UserId not found.",
                  
                });
            }


            var user =await _context.Users
                        .Where(u => u.Id == userId)
                        .FirstOrDefaultAsync();

            if (user == null)
            {
                return Ok(new
                {
                    status = false,
                    message = "User not found.",
                 
                });
            }




            return Ok(new StoreUsersDetailsApiResponseModel
            {

                status = true,
                Data = user,

            });
        }

        [HttpPost("RemoveProductFromCart/{productId}/{variantId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RemoveProductFromCart(int productId, int variantId)
        {
            var userId = User.FindFirstValue("userid");

            if (userId == null)
            {
                return Ok(new
                {
                    status = false,
                    message = "UserId not found.",

                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Ok(new
                {
                    status = false,
                    message = "User not found.",

                });

            }

            var cartItem = await _context.StoreCartItem
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == productId && c.VariantId == variantId);

            if (cartItem == null)
            {
                return Ok(new
                {
                    status = false,
                    message = "Item not found in Cart.",

                });
            }

            var bookingId = cartItem.BookingId;

            _context.StoreCartItem.Remove(cartItem);
            await _context.SaveChangesAsync();

            var cartItems = await _context.StoreCartItem
                .Where(c => c.BookingId == bookingId)
                .ToListAsync();

            var totalCost = cartItems.Sum(item => item.TotalPrice);
            var totalCount = cartItems.Count;
            var result = cartItems.Select(item => new
            {
                productId = item.ProductId,
                Productname = item.ProductName,
                varientId = item.VariantId,
                quantity = item.Quantity,
                totalPrice = item.TotalPrice,
                bookingId = item.BookingId,
                UserId = item.UserId
            }).ToList();

            decimal gstRate = 5m;
            decimal gstPrice = totalCost * gstRate / 100;
            int gstRounded = (int)Math.Round(gstPrice, 0);
            decimal totalPricewithGST = totalCost + gstRounded;

            return Ok(new StoreCartApiResponseModel
            {
                status = true,
                Message = "ProductRemoved",
                Data = result,
                TotalCost = totalPricewithGST,
                ProductCount = totalCount

            });
        }

        [HttpPost("RemoveQuantityFromCart/{productId}/{quantity}/{variantId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RemoveQuantityFromCart(int productId, int quantity, int variantId)
        {
            var userId = User.FindFirstValue("userid");

            if (userId == null)
            {
                return Unauthorized(new { Status = "Failed", Message = "User not authenticated." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Ok(new
                {
                    status = false,
                    message = "User not found.",

                });
            }

            var cartItem = await _context.StoreCartItem
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == productId && c.VariantId == variantId);

            if (cartItem == null)
            {
                return Ok(new
                {
                    status = false,
                    message = "Item not found.",

                });
            }

            // Save bookingId before potential removal
            var bookingId = cartItem.BookingId;

            if (quantity >= cartItem.Quantity)
            {
                // Remove item from cart if quantity is zero or less than requested removal
                _context.StoreCartItem.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity -= quantity;
                var totalPrice = await _context.ProductVariants
                    .Where(p => p.VariantId == variantId)
                    .Select(p => p.Price)
                    .FirstOrDefaultAsync();

                cartItem.TotalPrice = cartItem.Quantity * totalPrice;
                _context.StoreCartItem.Update(cartItem);
            }

            await _context.SaveChangesAsync();

            var cartItems =await _context.StoreCartItem
                .Where(c => c.BookingId == bookingId)
                .ToListAsync();

            var totalCost = cartItems.Sum(item => item.TotalPrice);
            var totalCount = cartItems.Count;

            var result = cartItems.Select(item => new
            {
                productId = item.ProductId,
                Productname = item.ProductName,
                varientId = item.VariantId,
                quantity = item.Quantity,
                totalPrice = item.TotalPrice,
                bookingId = item.BookingId,
                UserId = item.UserId,
                
            }).ToList();

          

            decimal gstRate = 5m;
            decimal gstPrice = totalCost * gstRate / 100;
            int gstRounded = (int)Math.Round(gstPrice, 0);
            decimal totalPricewithGST = totalCost + gstRounded;

            return Ok(new StoreCartApiResponseModel
            {
                status = true,
                Message = "CartUpdated",
                Data = result,
                TotalCost = totalPricewithGST,
                ProductCount = totalCount

            });
        }



        [HttpGet("GetColorsBySize/{productId}/{sizeId}")]
        public async Task<IActionResult> GetColorsBySize(int ProductId, int SizeId)
        {

            var variant = await _context.ProductVariants
                .Include(p => p.StoreProduct)
                .Where(p => p.ProductId == ProductId && p.SizeId == SizeId && p.Quantity > 0 && p.IsVisible)
                .Select(p => new
                {
                  
                  colorId= p.ColorId,
                  colorName= p.ColorName,
                  quantity=p.Quantity

                })
                .ToListAsync();
            if (!variant.Any())
            {
                return Ok(new
                {
                    status = false,
                    message = "Variant not found.",

                });
            }

            return Ok(new
            {
                status = true,
                message = "Colors fetched successfully.",
                data=variant

            });
        }


        [HttpPost("UserAddress")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UserAddress([FromBody] UserAddressRequest request)
        {

            var userId = User.FindFirstValue("userid");
          

            if (userId == null)
            {

                return Ok(new
                {
                    status = false,
                    message = "UserId not found.",

                });
            }
            var email = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync();
            try
            {
                var address = new StoreUserAddress
                {
                    UserId = userId,
                    Email=email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNo = request.PhoneNo,
                    Country = request.Country,
                    State = request.State,
                    City = request.City,
                    StreetAddress = request.StreetAddress,
                    PinCode = request.PinCode,
                    CompanyName = request.CompanyName,
                    Notes = request.Notes
                };

                _context.StoreUserAddress.Add(address);
                await _context.SaveChangesAsync();

                return Ok(new StoreUserAddressViewModel
                {
                    status=true,
                    Address = address
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, Message = ex.Message });
            }
        }



        [HttpGet("GetUserAddress")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserAddress()
        {

            var userId = User.FindFirstValue("userid");


            if (userId == null)
            {

                return Ok(new
                {
                    status = false,
                    message = "UserId not found.",
                });
            }
            var email = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync();
            try
            {
                var address = await _context.StoreUserAddress
              .Where(u => u.UserId == userId && u.Email == email)
              .ToListAsync();

                return Ok(new StoreUserAddressViewModel
                {
                    status=true,
                    Address = address
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, Message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("UserOrders")]
        public async Task<IActionResult> UserOrders()
        {
            var userId = User.FindFirstValue("userid");

            if (userId == null)
            {
                return NotFound("UserNotFound");
            }

            var user = await _context.Users
                        .Where(u => u.Id == userId)
                        .FirstOrDefaultAsync();

            var participants = await _context.StoreParticipants
                .Where(s => s.UserId == userId && s.PaymentStatus == "Paid")
                .ToListAsync();

            var model = new List<UsersOrdersViewModel>();

            var productIds = participants.Select(p => p.ProductId).Distinct().ToList();
            var variantIds = participants.Select(p => p.VariantId).Distinct().ToList();

            var products = await _context.StoreProducts
                .Where(p => productIds.Contains(p.Products_Id))
                .ToDictionaryAsync(p => p.Products_Id, p => p.ProductName);

            var images = await _context.StoreProductGallery
                .Where(v => variantIds.Contains(v.VariantId))
                .GroupBy(v => v.VariantId)
                .ToDictionaryAsync(g => g.Key, g => g.OrderBy(x => x.Id).Select(x => x.ImagePath).FirstOrDefault());

            var variants = await _context.ProductVariants
                .Where(v => variantIds.Contains(v.VariantId))
                .ToDictionaryAsync(v => v.VariantId, v => new { v.ColorName, v.SizeName });

            foreach (var item in participants)
            {
                model.Add(new UsersOrdersViewModel
                {
                    UserId = item.UserId,
                    BookingId = item.BookingId,
                    Quantity = item.Quantity,
                    ProductName = products.ContainsKey(item.ProductId) ? products[item.ProductId] : "Unknown Product",
                    Color = variants.ContainsKey(item.VariantId) ? variants[item.VariantId].ColorName : "N/A",
                    Size = variants.ContainsKey(item.VariantId) ? variants[item.VariantId].SizeName : "N/A",
                    PaymentStatus = item.PaymentStatus,
                    Amount = item.TotalPrice,
                    ImagePath = images.ContainsKey(item.VariantId) ? images[item.VariantId] : null,
                    OrderDate = item.OrderDate.ToString("dd-MM-yy")
                });
            }

            return Ok(new
            {
                status = true,
                data=model
            });
        }


        [HttpGet("GetAutocompleteStore")]
        public async Task<IActionResult> GetAutocompleteStore([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest(new { message = "Search term is required." });
            }

            var suggestions = await _context.StoreProducts
                .Include(t => t.StoreCategory) // assuming StoreCategory is the navigation property name
                .Where(t =>
                    (EF.Functions.Like(t.ProductName, $"%{term}%") ||
                     EF.Functions.Like(t.StoreCategory.CategoryName, $"%{term}%")) &&
                    t.IsVisible)
                .Select(t => new { ProductName = t.ProductName, ProductId = t.Products_Id })
                .Distinct()
                .ToListAsync();
            if (!suggestions.Any())
            {
                return Ok(new
                {
                    status = true,
                    message = "No products found.",
                    data = suggestions
                });
            }
            
            return Ok(new
            {
                status = true,
              
                data=suggestions
            });
        }
        [HttpPost]
        [Route("DeleteAddress/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteAddress(int id)

        {

            var userId = User.FindFirstValue("userid");
            var address = await _context.StoreUserAddress.FindAsync(id);

            if (address == null || address.UserId != userId)
            {
                return Ok(new
                {
                    status = false,
                    message = "Address not found or unauthorized.",
                });
            }
            else
            {
                var participants = await _context.StoreParticipants
                    .Where(u => u.AddressId == id)
                    .ToListAsync();

                if (participants.Any())
                {
                    foreach (var participant in participants)
                    {
                        bool isPaid = participant.PaymentStatus == "Paid";

                        DateTime deliveryDate = participant.OrderDate.AddDays(10).Date;
                        bool isDateMatched = deliveryDate <= DateTime.Today;

                        if (!(isPaid && isDateMatched))
                        {
                            return Ok(new
                            {
                                status = false,
                                message = "You can't delete this address",
                            });
                        }
                    }
                }

                _context.StoreUserAddress.Remove(address);
            }
            await _context.SaveChangesAsync();

            //return Ok($"Address with ID {address.Id} deleted successfully.");

            return Ok(new
            {
                status = true,
             
                message = $"Address with ID {address.Id} deleted successfully.",
            });
        }

        [HttpPost]
        [Route("UpdateUserAddress")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateUserAddress([FromBody] StoreUpdateUserAddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue("userid");
                var existingData = await _context.StoreUserAddress.FindAsync(model.Id);

                if (existingData != null && existingData.UserId == userId)
                {


                    existingData.FirstName = model.FirstName;
                    existingData.LastName = model.LastName;
                    existingData.PhoneNo = model.PhoneNo;
                    existingData.Country = model.Country;
                    existingData.State = model.State;
                    existingData.City = model.City;
                    existingData.StreetAddress = model.StreetAddress;
                    existingData.PinCode = model.PinCode;
                    existingData.CompanyName = model.CompanyName;
                    existingData.Notes = model.Notes;

                    _context.Update(existingData);
                    await _context.SaveChangesAsync();
                    return Ok(new { status = true, message = "Address updated successfully." });

                }
                return Ok(new
                {
                    status = false,
                    message = "Data not found.",
                });
            }

            return Ok(new
            {
                status = false,
                message = "Invalid data.",
            });
        }
    }
}
