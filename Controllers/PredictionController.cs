using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Porumb_Denisa_Lab4.Models;
using PriceML = Porumb_Denisa_Lab4.PricePredictionModel;
using TimeML = Porumb_Denisa_Lab4.TimePrediction;
using Porumb_Denisa_Lab4.Data;

namespace Porumb_Denisa_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        private readonly AppDbContext _context;

        public PredictionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Price() => View();

        [HttpPost]
        public async Task<IActionResult> Price(PriceML.ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();
            // Create predection engine related to the loaded train model
            ITransformer mlModel =
           mlContext.Model.Load("PricePredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<PriceML.ModelInput,
           PriceML.ModelOutput>(mlModel);

            // Try model on sample data to predict fair price
            PriceML.ModelOutput result = predEngine.Predict(input);
            ViewBag.Price = result.Score;
            

            var history = new PredictionHistory
            {
                PassengerCount = input.Passenger_count,
                TripTimeInSecs = input.Trip_time_in_secs,
                TripDistance = input.Trip_distance,
                PaymentType = input.Payment_type,
                PredictedPrice = result.Score,
                CreatedAt = DateTime.Now
            };

            _context.PredictionHistories.Add(history);
            await _context.SaveChangesAsync();

            return View(input);
        }

        [HttpGet]
        public IActionResult Time()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Time(TimeML.ModelInput input)
        {
            
            // Load the model
            MLContext mlContext = new MLContext();
            // Create predection engine related to the loaded train model
            ITransformer mlModel =
           mlContext.Model.Load("TimePrediction.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<TimeML.ModelInput,
           TimeML.ModelOutput>(mlModel);

            // Try model on sample data to predict fair price
            TimeML.ModelOutput result = predEngine.Predict(input);
            ViewBag.Time = result.Score;
            return View(input);
        }

        [HttpGet]
        public async Task<IActionResult> History(string? paymentType,float? minPrice,float? maxPrice, string? sortOrder, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.PredictionHistories.AsQueryable();

            if (!string.IsNullOrEmpty(paymentType))
            {
                query = query.Where(p => p.PaymentType == paymentType);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice <= maxPrice.Value);
            }


            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= endDate.Value);
            }

            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.PredictedPrice),
                "price_desc" => query.OrderByDescending(p => p.PredictedPrice),
                "date_asc" => query.OrderBy(p => p.CreatedAt),
                "date_desc" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };


            ViewBag.CurrentPaymentType = paymentType;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");

            var result = await query.ToListAsync();
            return View(result);

        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.PredictionHistories.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date <= toDate.Value.Date);
            }


            // 1. Numărul total de predicții
            var totalPredictions = await query.CountAsync();

            // 2. Preț mediu per tip de plată + număr de predicții per tip
            var paymentTypeStats = await query
                .GroupBy(p => p.PaymentType)
                .Select(g => new PaymentTypeStat
                {
                    PaymentType = g.Key,
                    AveragePrice = g.Average(x => x.PredictedPrice),
                    Count = g.Count()
                })
                .ToListAsync();

            // 3. Distribuția prețurilor pe intervale (buckets)
            // Definim intervalele: 0-10, 10-20, 20-30, 30-50, >50
            var allPredictions = await query
                .Select(p => p.PredictedPrice)
                .ToListAsync();

            var buckets = new List<PriceBucketStat>
    {
        new PriceBucketStat { Label = "0 - 10", Count = 0 },
        new PriceBucketStat { Label = "10 - 20", Count = 0 },
        new PriceBucketStat { Label = "20 - 30", Count = 0 },
        new PriceBucketStat { Label = "30 - 50", Count = 0 },
        new PriceBucketStat { Label = "> 50", Count = 0 }
    };

            foreach (var price in allPredictions)
            {
                if (price < 10)
                    buckets[0].Count++;
                else if (price < 20)
                    buckets[1].Count++;
                else if (price < 30)
                    buckets[2].Count++;
                else if (price < 50)
                    buckets[3].Count++;
                else
                    buckets[4].Count++;
            }

            // 4. Construim ViewModel-ul
            var vm = new DashboardViewModel
            {
                TotalPredictions = totalPredictions,
                PaymentTypeStats = paymentTypeStats,
                PriceBuckets = buckets
            };

            return View(vm);
        }

    }
}
