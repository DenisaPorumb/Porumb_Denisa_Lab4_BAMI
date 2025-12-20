using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using PriceML = Porumb_Denisa_Lab4.PricePredictionModel;
using TimeML = Porumb_Denisa_Lab4.TimePrediction;

namespace Porumb_Denisa_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        [HttpGet]
        public IActionResult Price() => View();

        [HttpPost]
        public IActionResult Price(PriceML.ModelInput input)
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

    }
}
