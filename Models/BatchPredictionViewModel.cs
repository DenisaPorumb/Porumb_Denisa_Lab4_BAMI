using PriceML = Porumb_Denisa_Lab4.PricePredictionModel;
namespace Porumb_Denisa_Lab4.Models
{
    public class BatchPredictionViewModel
    {
        public IFormFile? File { get; set; }     
        
        // Rezultatele (predicțiile) pentru fiecare rând din fișier
        public List<PriceML.ModelOutput>? Predictions { get; set; }
        public string? ErrorMessage { get; set; } 
    }
}
