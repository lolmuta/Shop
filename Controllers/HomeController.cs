using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Shop.Models;
using Shop.Utils;
using System.Data;
using System.Diagnostics;

namespace Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbHelper dbHelper;
        private readonly UserInfoHelper userInfoHelper;
        private readonly IHttpContextAccessor httpContextAccessor;

        public HomeController(ILogger<HomeController> logger
            , DbHelper dbHelper
            , UserInfoHelper userInfoHelper
            , IHttpContextAccessor _httpContextAccessor)
        {
            _logger = logger;
            this.dbHelper = dbHelper;
            this.userInfoHelper = userInfoHelper;
            httpContextAccessor = _httpContextAccessor;
        }

        public IActionResult Index()
        {
           
            return View();
        }
        [HttpGet]
        public IActionResult IndexQuery()
        { 
            var dt = dbHelper.Query(@"
                 SELECT [IDNo]
                      ,[name]
                      ,[count]
                      ,[src]
                      , price  
                  FROM [TestDB].[dbo].[Items]", null);

            var jsonData = dt.AsEnumerable().Select(x => new
            {
                IDNo = x.Field<int>("IDNo"),
                name = x.IsNull("name") ? "" : x.Field<string>("name"),
                count = x.IsNull("count") ? 0 : x.Field<int>("count"),
                src = x.IsNull("src") ? "" : x.Field<string>("src"),
                price = x.IsNull("price") ? 0m : x.Field<decimal>("price"),
            });

            return Json(jsonData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Item(string id)
        {
            var request = httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            //string id = HttpContext.Request.Query["id"];
            var ItemInfo = dbHelper.Query<ItemInfo>(@"
                 SELECT [IDNo]
                      ,[name]
                      ,[count]
                      , @baseUrl + '/' + [src] as src
                      , price
                  FROM [TestDB].[dbo].[Items]
                  where IDNo = @IDNo", 
                  p => 
                  { 
                      p.AddWithValue("@IDNo", id);
                      p.AddWithValue("@baseUrl", baseUrl);
                  })
                .First();
            
                return View(ItemInfo);
        }
        [HttpPost]
        public IActionResult UpdateBuyItem([FromBody] BuyInfo buyInfo)
        {
            try
            {
                var sql = @" 
                MERGE [dbo].[BuyItems] AS tgt
                USING (SELECT @BuyCount as BuyCount, @Items_IDNo as Items_IDNo, @UserId as UserId) AS src
                    ON (tgt.Items_IDNo = src.Items_IDNo and tgt.UserID = src.UserId)
                WHEN MATCHED
                    THEN
                        UPDATE
                        SET tgt.buyCount = src.buyCount
				
                WHEN NOT MATCHED BY Target 
                    THEN
                        INSERT (buyCount, Items_IDNo, UserId)
                        VALUES (src.buyCount, src.Items_IDNo, src.UserId);
                ";
                var UserId = userInfoHelper.GetUserId();
                Action<SqlParameterCollection> paramAct = sqlCmd =>
                {
                    sqlCmd.AddWithValue("@BuyCount", buyInfo.buyCount);
                    sqlCmd.AddWithValue("@Items_IDNo", buyInfo.IDNo);
                    sqlCmd.AddWithValue("@UserId", UserId);
                };
                var result = dbHelper.ExecuteNonQuery(sql, paramAct);
                if (result == 1)
                {
                    // Return success message
                    return Ok(new { success = true, message = "Item updated successfully" });
                }
                else
                {
                    // Return error message
                    return BadRequest(new { success = false, message = "Failed to update item" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "測試");
                return BadRequest(new { success = false, message = "exception " + ex.Message });
            }
        }
        [HttpPost]
        public IActionResult DeleteBuyItem([FromBody] BuyInfoDelete buyInfoDelete)
        {
            try
            {
                var sql = @" 
                    delete [BuyItems] where IDNo=@idNo;
                ";
                var UserId = userInfoHelper.GetUserId();
                Action<SqlParameterCollection> paramAct = sqlCmd =>
                {
                    sqlCmd.AddWithValue("@idNo", buyInfoDelete.IDNo);
                };
                var result = dbHelper.ExecuteNonQuery(sql, paramAct);
                if (result == 1)
                {
                    // Return success message
                    return Ok(new { success = true, message = "Item delete successfully" });
                }
                else
                {
                    // Return error message
                    return BadRequest(new { success = false, message = "Failed to delete item" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "測試");
                return BadRequest(new { success = false, message = "exception " + ex.Message });
            }
        }
        public IActionResult BuyItemList()
        {
            var request = httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var sql = @"
            SELECT [BuyItems].[IDNo]
                  ,[BuyItems].[BuyCount]
                  ,[BuyItems].[Items_IDNo]
	              ,[Items].Name
	              ,[Items].price
                  ,@baseUrl + '/' + [Items].src as src
            FROM [TestDB].[dbo].[BuyItems]
            left join [dbo].[Items] on [Items].IDNo = [BuyItems].[Items_IDNo]
            where 1 = 1
	        and [BuyItems].UserId = @UserId
            ";
            var UserId = userInfoHelper.GetUserId();
            var list = dbHelper.Query<BuyItem>(sql, sqlcmd =>
            {
                sqlcmd.AddWithValue("@UserId", UserId);
                sqlcmd.AddWithValue("@baseUrl", baseUrl);
            });

            return View(list);
        }
        public IActionResult Buy()
        {
            return View();
        }
    }
    public class BuyInfo
    {
        public int IDNo { get; set; }
        public int buyCount { get; set; }
    }
    public class BuyInfoDelete
    {
        public int IDNo { get; set; }
    }
}