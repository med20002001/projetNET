using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

// Définir la classe RouteCounter avant de l'utiliser
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Instanciation de RouteCounter
var routeCounter = new RouteCounter();
var userService = new UserService();
var logger = new LoggerService();

// Middleware pour compter les accès aux routes
app.Use(async (context, next) =>
{
    var apiKey = context.Request.Headers["Authorization"];

    if (string.IsNullOrEmpty(apiKey) || apiKey != "Bearer secret123")
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Accès refusé : Authentification requise.");
        return;
    }

    await next();
});


// Définition des routes API
app.MapGet("/users", () =>
{
    return Results.Ok(userService.GetAllUsers());
});

app.MapPost("/users", (User user) =>
{
    if (!UserValidator.IsValid(user))
        return Results.BadRequest(new { Message = "Données utilisateur invalides !" });

    if (!userService.AddUser(user))
        return Results.BadRequest(new { Message = "Utilisateur déjà existant !" });

    logger.Log($"Utilisateur ajouté : {user.Username}");
    return Results.Created($"/users/{user.Username}", user);
});

app.MapPut("/users/{username}", (string username, User user) =>
{
    if (!UserValidator.IsValid(user))
        return Results.BadRequest(new { Message = "Données utilisateur invalides !" });

    if (!userService.UpdateUser(username, user))
        return Results.NotFound(new { Message = "Utilisateur introuvable !" });

    logger.Log($"Utilisateur mis à jour : {username}");
    return Results.Ok(new { Message = $"Utilisateur {username} mis à jour", Data = user });
});

app.MapDelete("/users/{username}", (string username) =>
{
    if (!userService.DeleteUser(username))
        return Results.NotFound(new { Message = "Utilisateur introuvable !" });

    logger.Log($"Utilisateur supprimé : {username}");
    return Results.Ok(new { Message = $"Utilisateur {username} supprimé" });
});


// Démarrer l'application
app.Run();

// Classe pour compter les accès aux routes
public class RouteCounter
{
    private readonly Dictionary<string, int> _routeCounts = new();

    public void Increment(string route) => _routeCounts[route] = _routeCounts.GetValueOrDefault(route, 0) + 1;

    public int GetCount(string route) => _routeCounts.GetValueOrDefault(route, 0);
}
public class UserValidator
{
    public static bool IsValid(User user)
    {
        return !string.IsNullOrWhiteSpace(user.Username) &&
               !string.IsNullOrWhiteSpace(user.Usage) &&
               user.Username.Length >= 3 &&
               new[] { "Admin", "User", "Guest" }.Contains(user.Usage);
    }
}
public class UserService
{
    private readonly List<User> _users = new();
    
    public bool AddUser(User user)
    {
        if (_users.Any(u => u.Username == user.Username)) return false;
        _users.Add(user);
        return true;
    }

    public bool UpdateUser(string username, User newUser)
    {
        var existingUser = _users.FirstOrDefault(u => u.Username == username);
        if (existingUser == null) return false;

        existingUser.Usage = newUser.Usage;
        return true;
    }

    public bool DeleteUser(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);
        if (user == null) return false;

        _users.Remove(user);
        return true;
    }

    public User? GetUser(string username) => _users.FirstOrDefault(u => u.Username == username);

    public IEnumerable<User> GetAllUsers() => _users;
}
public class LoggerService
{
    public void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now}] {message}");
    }
}