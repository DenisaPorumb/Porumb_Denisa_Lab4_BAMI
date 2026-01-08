using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Porumb_Denisa_Lab4.Data;
using Porumb_Denisa_Lab4.Models;

namespace Porumb_Denisa_Lab4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PredictionApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PredictionHistory>>> GetPredictions()
        {
            return await _context.PredictionHistories.ToListAsync();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrediction(int id)
        {
            var prediction = await _context.PredictionHistories.FindAsync(id);

            if (prediction == null)
            {
                return NotFound();
            }

            _context.PredictionHistories.Remove(prediction);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
