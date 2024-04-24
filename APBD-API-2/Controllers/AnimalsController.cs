using APBD_API_2.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace APBD_API_2.Controllers;

[ApiController]
[Route("api/animals")]
public class AnimalsController : ControllerBase
{
    
    private readonly IConfiguration _configuration;

    public AnimalsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetAllAnimals([FromQuery] string orderBy = "name")
    {
        var allowedSortColumns = new List<string> { "name", "description", "category", "area" };
        if (!allowedSortColumns.Contains(orderBy.ToLower()))
        {
            return BadRequest("Invalid orderBy parameter. Must be one of: name, description, category, area.");
        }

        var response = new List<GetAnimalsResponse>();
        using (var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand($"SELECT * FROM Animal ORDER BY {orderBy} ASC", sqlConnection);
            sqlCommand.Connection.Open();

            var reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                response.Add(new GetAnimalsResponse(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4)
                ));
            }
        }

        return Ok(response);
    }

    [HttpPost]
    public IActionResult CreateAnimal(CreateAnimalRequest request)
    {
        using (var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand(
                "INSERT INTO Animal (Name, Description, Category, Area) values (@1, @2, @3, @4); SELECT CAST(SCOPE_IDENTITY() as int)",
                sqlConnection
            );
            sqlCommand.Parameters.AddWithValue("@1", request.Name);
            sqlCommand.Parameters.AddWithValue("@2", request.Description);
            sqlCommand.Parameters.AddWithValue("@3", request.Category);
            sqlCommand.Parameters.AddWithValue("@4", request.Area);
            sqlCommand.Connection.Open();
            
            var id = sqlCommand.ExecuteScalar();
            
            return Created($"api/animals/{id}", new CreateAnimalResponse((int)id, request));
        }
    }

    [HttpPut("{idAnimal}")]
    public IActionResult ReplaceAnimal(int idAnimal, ReplaceAnimalRequest request)
    {
        using (var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand(
                "UPDATE Animal SET Name = @1, Description = @2, Category = @3, Area = @4 WHERE IdAnimal = @5",
                sqlConnection
            );
            sqlCommand.Parameters.AddWithValue("@1", request.Name);
            sqlCommand.Parameters.AddWithValue("@2", request.Description);
            sqlCommand.Parameters.AddWithValue("@3", request.Category);
            sqlCommand.Parameters.AddWithValue("@4", request.Area);
            sqlCommand.Parameters.AddWithValue("@5", idAnimal);
            sqlCommand.Connection.Open();

            var affectedRows = sqlCommand.ExecuteNonQuery();
            return affectedRows == 0 ? NotFound() : NoContent();
        }
    }
    
    [HttpDelete("{idAnimal}")]
    public IActionResult RemoveAnimal(int idAnimal)
    {
        using (var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            var command = new SqlCommand("DELETE FROM Animal WHERE IdAnimal = @1", sqlConnection);
            command.Parameters.AddWithValue("@1", idAnimal);
            command.Connection.Open();

            var affectedRows = command.ExecuteNonQuery();

            return affectedRows == 0 ? NotFound() : NoContent();
        }
    }

}