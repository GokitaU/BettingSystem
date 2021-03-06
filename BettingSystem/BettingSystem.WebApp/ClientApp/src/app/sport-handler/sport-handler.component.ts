import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Enums } from '../../../watts/generated/enums';
import { AngularEndpointsService } from '../../../watts/generated/angular-endpoints.service';
import { Interfaces } from '../../../watts/generated/interfaces';
import { CurrentBetService } from '../current-bet/current-bet.service';

@Component({
  selector: 'sport-handler',
  templateUrl: './sport-handler.component.html',
})

export class SportHandlerComponent implements OnInit {

  constructor(private route: ActivatedRoute, private router: Router, private endpointsService: AngularEndpointsService, public currentBetService: CurrentBetService) {
   this.route.params.subscribe(params => {
      switch (params['sportName']) {
        case 'football':
          this.gameType = Enums.GameType.Football;
          break;
        case 'basketball':
          this.gameType = Enums.GameType.Basketball;
          break;
        case 'handball':
          this.gameType = Enums.GameType.Handball;
          break;
        default:
          this.router.navigate(['']);
      }

     this.endpointsService.Game.GetAllGames({ sportType: this.gameType }).call<Interfaces.IGameOfferView>().then((data) => {
        this.gamesData = data;
      })
    });
  }
  public gamesData: Interfaces.IGameOfferView =  null;
  public gameType: Enums.GameType = null;
  public gameTypes: typeof Enums.GameType = Enums.GameType;
  public betTypes: typeof Enums.BetType = Enums.BetType;

  public getCoefficientValue = (game: Interfaces.IGameView, betType: string) => {
    var item = game.coefficients.find((el) => el.betType == Enums.BetType[betType]);
    return item ? item.coefficientValue.toString() : "";
  }

  public addCoefficient = (game: Interfaces.IGameView, betType: string, isSpecialOffer: boolean) => {
    var coefficient = game.coefficients.find((el) => el.betType == Enums.BetType[betType]);

    this.currentBetService.placeBetOnCoefficient(game.id, this.gameTypes[this.gameType].toString(), game.firstTeamName, game.secondTeamName, isSpecialOffer, coefficient);
  }

  public IsBetPlacedForCoefficient = (game: Interfaces.IGameView, betType: string) => {
    var item = game.coefficients.find((el) => el.betType == Enums.BetType[betType]);
    return this.currentBetService.isBetAlreadyPlaced(item.id, game.id);
  }

  public getBetTypeName = (betType: string) => this.currentBetService.getBetTypeName(Enums.BetType[betType]);

  ngOnInit() {

  }

}
