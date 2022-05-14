namespace Zool.PetaKopi.Backend.Controllers.Dtos;

public record CoffeeDto(
    string Logo,
    string Name,
    string District,
    string State,
    List<string> Links,
    List<string> Tags,
    string SubmittedBy,
    DateTime SubmittedOn,
    double Latitude,
    double Longitude
);
