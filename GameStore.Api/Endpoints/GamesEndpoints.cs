using System;
using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGame";

    public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("games").WithParameterValidation();
        //GET /games
        group.MapGet("/", (GameStoreContext dbcontext) => dbcontext.Games
                    .Include(game => game.Genre)
                    .Select(game => game.ToGameSummaryDto())
                    .AsNoTracking()
       );
        //GET /games/1
        group.MapGet("/{id}", (int id,GameStoreContext dbContext) =>
        {
            Game? game = dbContext.Games.Find(id);
            return game is null ? Results.NotFound() : Results.Ok(game.ToGameDetailsDto());
        }).WithName(GetGameEndpointName);
        //POST /games
        group.MapPost("/", (CreateGameDto newGame,GameStoreContext dbContext) =>
        {
            Game game = newGame.ToEntity();
          
            dbContext.Games.Add(game);
            dbContext.SaveChanges();
         
       
            return Results.CreatedAtRoute(GetGameEndpointName, new { id = game.Id }, game.ToGameDetailsDto());
        });
        //PUT /games/1
        group.MapPut("/{id}", (int id, UpdateGameDto updatedGame,GameStoreContext dbcontext) =>
        {

            var existingGame = dbcontext.Games.Find(id);

            if (existingGame is null)
            {
                return Results.NotFound();
            }
            dbcontext.Entry(existingGame)
                        .CurrentValues
                        .SetValues(updatedGame.ToEntity2(id));
            dbcontext.SaveChanges();
            return Results.NoContent();
        });

        //DELETE /games/1
        group.MapDelete("/{id}", (int id,GameStoreContext dbcontext) =>
        {
            var game = dbcontext.Games.Find(id);
            if (game is null)
            {
                return Results.NoContent();
            }
            dbcontext.Games.Remove(game);
            dbcontext.SaveChanges();
            return Results.NoContent();
        });
        return group;
    }
}
