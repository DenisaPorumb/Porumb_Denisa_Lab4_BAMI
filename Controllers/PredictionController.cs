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
        public async Task<IActionResult> History()
        {
            var history = await _context.PredictionHistories
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
            return View(history);
        }

    }
}
