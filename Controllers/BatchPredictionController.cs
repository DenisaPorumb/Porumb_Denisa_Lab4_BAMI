using Microsoft.AspNetCore.Http;
using Microsoft.ML;
using Microsoft.AspNetCore.Mvc;
using Porumb_Denisa_Lab4.Models;
using PriceML = Porumb_Denisa_Lab4.PricePredictionModel;

public class BatchPredictionController : Controller
{
    private readonly IWebHostEnvironment _env;

    public BatchPredictionController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet]
    public IActionResult Batch()
    {
        var vm = new BatchPredictionViewModel();
        return View(vm);
    }

    // proceseaza fisierul CSV si face predictii in lot
    [HttpPost]
    public IActionResult Batch(BatchPredictionViewModel model)
    {
        if (model.File == null || model.File.Length == 0)
        {
            model.ErrorMessage = "Vă rugăm să selectați un fișier CSV.";
            return View(model);
        }

        // Salvăm fișierul încărcat într-un fișier temporar pe disc
        var uploadsFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var tempFilePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + ".csv");

        using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            model.File.CopyTo(stream);
        }

        // Incarcăm modelul ML
        var mlContext = new MLContext();
        ITransformer mlModel = mlContext.Model.Load("PricePredictionModel.mlnet", out var inputSchema);

        // Încărcăm datele din CSV în IDataView
        var dataView = mlContext.Data.LoadFromTextFile<PriceML.ModelInput>(
            path: tempFilePath,
            hasHeader: true,
            separatorChar: ',');

        // Aplicăm modelul pe întregul DataView (batch prediction)
        var predictionsDataView = mlModel.Transform(dataView);

        // Convertim rezultatele în IEnumerable<ModelOutput>
        var predictions = mlContext.Data.CreateEnumerable<PriceML.ModelOutput>(
            predictionsDataView,
            reuseRowObject: false)
            .ToList();

        // Ștergem fișierul temporar
        System.IO.File.Delete(tempFilePath);

        // Punem lista de rezultate în ViewModel și returnăm view-ul
        model.Predictions = predictions;

        return View(model);
    }
}