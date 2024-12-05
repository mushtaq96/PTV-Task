using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreetService.Data;
using StreetService.Models;
using System.Threading.Tasks;
using NetTopologySuite;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace StreetService.Controllers
{
    // This controller handles HTTP requests for managing street data.
    // It includes endpoints for creating, modifying, and deleting streets.
    [ApiController]
    [Route("api/[controller]")]
    public class StreetsController : ControllerBase
    {
        private readonly StreetContext _context;
        private readonly IFeatureManager _featureManager;

        public StreetsController(StreetContext context, IFeatureManager featureManager)
        {
            _context = context;
            _featureManager = featureManager;
        }
        
        // POST api/streets
        // Creates a new street.
        // Validates the input, sanitizes the name, and ensures the geometry is in a valid WKT format.
        [HttpPost]
        public async Task<ActionResult<Street>> CreateStreet([FromBody] Street street)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Sanitize input
                street.Name = SanitizeString(street.Name);

                // Normalize and validate geometry
                var normalizedGeometry = Regex.Replace(street.Geometry, @"\s+", " ").Trim();

                var wktReader = new WKTReader();
                try
                {
                    var geometry = wktReader.Read(normalizedGeometry);
                    street.Geometry = geometry.ToString();
                }
                catch (Exception ex)
                {
                    return BadRequest($"Provided geometry is not in a valid WKT format. Please ensure that the geometry is correctly formatted. Example of a valid WKT format: 'POINT (30 10)'. Error: {ex.Message}");
                }

                // Check for duplicates
                if (await _context.Streets.AnyAsync(s => s.Name == street.Name))
                {
                    return BadRequest("Street with the same name already exists");
                }

                _context.Streets.Add(street);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetStreet), new { id = street.Id }, street);
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, $"An error occurred while creating the street: {ex.Message}");
            }
        }
        
        // DELETE api/streets/{id}
        // Deletes a street by its ID.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStreet(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "Invalid street id." });
            }
            var street = await _context.Streets.FindAsync(id);
            if (street == null)
            {
                return NotFound(new { Message = $"Street with id {id} not found." });
            }
            _context.Streets.Remove(street);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST api/streets/modify
        // Modifies an existing street by adding a point to its geometry.
        // Handles race conditions and uses a feature flag to decide whether to perform the operation on the database level or within the backend code.
        [HttpPost("modify")]
        public async Task<ActionResult<Street>> ModifyStreet([FromBody] ModifyStreetRequest request)
        {
            bool useDatabaseOperation = await _featureManager.IsEnabledAsync("UseDatabaseOperation");
            
            // Validate input
            if (!request.Geometry.StartsWith("POINT"))
            {
                return BadRequest("Invalid geometry format. Use 'POINT(x y)' format.");
            }

            var street = await _context.Streets.FirstOrDefaultAsync(s=>s.Name==request.Name);
            if (street == null)
            {
                return NotFound();
            }
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                var wktReader = new WKTReader();
                var existingGeometry = wktReader.Read(street.Geometry);
                var newPoint = wktReader.Read(request.Geometry);
                
                if (useDatabaseOperation)
                {
                    // Database operation logic here
                    // Use PostGIS to modify the geometry
                    var result = await _context.Streets
                        .FromSqlRaw("SELECT ST_AsText(ST_AddPoint(ST_GeomFromText({0}), ST_GeomFromText({1}))) AS Geometry", existingGeometry.AsText(), newPoint.AsText())
                        .Select(s => s.Geometry)
                        .FirstOrDefaultAsync();
                    street.Geometry = result;
                    street.LastModified = DateTime.UtcNow;
                }
                else
                {
                    // In-memory operation logic here
                    var combinedGeometry = existingGeometry.Union(newPoint);
                    street.Geometry = combinedGeometry.AsText();
                    street.LastModified = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict("The street was modified by another user.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while modifying the street: {ex.Message}");
            }
            return Ok(street);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Street>> GetStreet(int id)
        {
            var street = await _context.Streets.FindAsync(id);
            if (street == null)
            {
                return NotFound();
            }
            return street;
        }

        // Sanitizes the input string by trimming, normalizing whitespace, and removing non-alphanumeric characters.
        private string SanitizeString(string input)
        {
            // Remove leading/trailing whitespace
            input = input.Trim();
            
            // Replace multiple spaces with a single space
            input = Regex.Replace(input, @"\s+", " ");
            
            // Remove any characters that aren't alphanumeric or spaces
            input = new string(input.ToCharArray().Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
            
            return input;
        }

    }
}