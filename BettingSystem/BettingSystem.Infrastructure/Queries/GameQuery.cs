﻿using BettingSystem.Common.Infrastructure.DatabaseContext;
using BettingSystem.Infrastructure.Entities;
using System.Linq;
using BettingSystem.Core.Views;
using BettingSystem.Core.InfrastructureContracts.Queries;
using BettingSystem.Common.Core.Enums;

namespace BettingSystem.Infrastructure.Queries
{
    public class GameQuery : BaseQuery<GameView, Game>, IGameQuery
    {
        private readonly BettingSystemDatabaseContext context;

        public GameQuery(BettingSystemDatabaseContext context) : base(context)
        {
            this.context = context;
        }

        GameQuery(GameQuery previous, IQueryable<Game> inner) : base(inner)
        {
            this.context = previous.context;
        }

        public IGameQuery WhereUnresolved() {
            return new GameQuery(this, this.inner.Where(e => !e.DateTimePlayed.HasValue));
        }

        public IGameQuery WhereGameType(GameType type)
        {
            return new GameQuery(this, this.inner.Where(e => e.GameType == type));
        }

        public override IQueryable<GameView> Project()
        {
            return from game in this.inner
                   select new GameView
                   {
                       Id = game.Id,
                       GameType = game.GameType,
                       FirstTeamName = game.FirstTeamName,
                       SecondTeamName = game.SecondTeamName,
                       FirstTeamScore = game.FirstTeamScore,
                       SecondTeamScore = game.SecondTeamScore,
                       DateTimeStarting = game.DateTimeStarting,
                       DateTimePlayed = game.DateTimePlayed,
                       Coefficients = (from coefficient in context.Set<Coefficient>()
                                      where coefficient.GameId == game.Id
                                      select new CoefficientView
                                      {
                                          Id = coefficient.Id,
                                          GameId = game.Id,
                                          BetType = coefficient.BetType,
                                          CoefficientValue = coefficient.CoefficientValue
                                      }).ToList()
                   };
        }



        public GameOfferView AsGameOfferView()
        {
            var games = this.Project().ToArray().GroupBy(e => e.GameType).Select(group => new
            {
                Type = group.Key,
                Values = group.OrderByDescending(e => e.Coefficients.Select(v => v.CoefficientValue).Aggregate(1.0, (x, y) => x * y))
            });

            return new GameOfferView
            {
                BestOffers = games.SelectMany(e => e.Values.Take(1)).OrderBy(e => e.DateTimeStarting).ToArray(),
                OtherOffers = games.SelectMany(e => e.Values.Skip(1)).OrderBy(e => e.DateTimeStarting).ToArray()
            };
        }
    }
}