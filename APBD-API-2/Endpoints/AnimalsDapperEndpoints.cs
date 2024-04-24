using System.Data.SqlClient;
using APBD_API_2.DTOs;
using Dapper;
using FluentValidation;

namespace APBD_API_2.Endpoints;

public static class AnimalsDapperEndpoints
{
    public static void RegisterAnimalsDapperEndpoints(this WebApplication app)
    {
        var animals = app.MapGroup("api/animals-dapper");

        animals.MapGet("/", GetAnimals);
        animals.MapPost("/", CreateAnimal);
        animals.MapDelete("{idAnimal:int}", RemoveAnimal);
        animals.MapPut("{idAnimal:int}", ReplaceAnimal);
    }

    private static IResult ReplaceAnimal(IConfiguration configuration, IValidator<ReplaceAnimalRequest> validator, int id, ReplaceAnimalRequest request)
    {
        
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }
        
        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var affectedRows = sqlConnection.Execute(
                "UPDATE Animal SET Name = @Name, Description = @Description, Category = @Category, Area = @Area WHERE IdAnimal = @IdAnimal",
                new
                {
                    Name = request.Name,
                    Description = request.Description,
                    Category = request.Category,
                    Area = request.Area,
                    IdAnimal = id
                }
            );
            
            if (affectedRows == 0) return Results.NotFound();
        }

        return Results.NoContent();
    }

    private static IResult RemoveAnimal(IConfiguration configuration, int id)
    {
        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var affectedRows = sqlConnection.Execute(
                "DELETE FROM Animal WHERE ID = @Id",
                new { Id = id }
            );
            return affectedRows == 0 ? Results.NotFound() : Results.NoContent();
        }
    }

    private static IResult CreateAnimal(IConfiguration configuration, IValidator<CreateAnimalRequest> validator, CreateAnimalRequest request)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var id = sqlConnection.ExecuteScalar<int>(
                "INSERT INTO Animal (Name, Description, Category, Area) values (@Name, @Description, @Category, @Area); SELECT CAST(SCOPE_IDENTITY() as int)",
                new
                {
                    Name = request.Name,
                    Description = request.Description,
                    Category = request.Category,
                    Area = request.Area
                }
            );

            return Results.Created($"/api/animals-dapper/{id}", new CreateAnimalResponse(id, request));
        }
    }

    private static IResult GetAnimals(IConfiguration configuration)
    {
        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var animals = sqlConnection.Query<GetAnimalsResponse>("SELECT * FROM animal");
            return Results.Ok(animals);
        }
    }

}