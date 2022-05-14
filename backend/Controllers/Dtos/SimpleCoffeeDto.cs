namespace Zool.PetaKopi.Backend.Controllers.Dtos;

public record SimpleCoffeeDto(
    string Logo,
    string Name,
    string District,
    string State,
    string Slug
);
