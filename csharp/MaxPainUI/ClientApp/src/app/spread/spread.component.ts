import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { ActivatedRoute, Router} from '@angular/router';
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { Ticker } from "../models/ticker";
import { Spread } from '../models/spread';
import { AdsenseComponent } from '../adsense/adsense.component';


@Component( {
  selector: 'app-spread',
  templateUrl: './spread.component.html',
  styleUrls: ['./spread.component.scss']
})
export class SpreadComponent implements OnInit {

  @ViewChild('tickersearch') tickersearch: ElementRef;

  public tickerObj: Ticker = new Ticker(this.utils);
  public spreads: Array<Spread>;
  public calls: Array<Spread>;
  public puts: Array<Spread>;
  public maturities: string[];
  public isEmpty: boolean = false;

  //added the data parameter
  constructor(
    private data: DataService,
    private actRoute: ActivatedRoute,
    private route: Router,
    private utils: UtilsService,
    private title: Title) {

    // override the route reuse strategy
    this.route.routeReuseStrategy.shouldReuseRoute = function () {
      return false;
    };
  }

  ngOnInit() {
    this.tickerObj.Ticker = this.actRoute.snapshot.params.id;
    this.title.setTitle(this.tickerObj.Ticker + " Spreads");

    this.actRoute.queryParams.subscribe(params => {
      this.tickerObj.Maturity = this.utils.ParseDateDelimiter(params["m"], "-");
    });
  }

  ngAfterViewInit() {
    /*
    let observable$: Observable<Array<Spread>> = 
      this.data.getSpreads(this.tickerObj.Ticker);
    observable$.subscribe(response => {
      this.spreads = response;

      if (this.spreads == null || this.spreads.length == 0) {
        this.isEmpty = true;
        this.spreads = [];
      }
      else {
        this.loadMaturity();
      }
    });
    */
  }

  onKeydown(event) {
    //console.log(event);
    if (event.key === "Enter") {
      this.changeTicker(event);
    }
  }

  loadMaturity() {
    this.logTickerObj("loadMaturity");
    this.maturities = this.distinctMaturity(this.spreads);
    let maturityStr: string = this.maturities[this.maturities.length - 1];
    if (this.tickerObj.Maturity) {
      maturityStr = this.utils.FormatDate(this.tickerObj.Maturity, "MM/dd/yyyy");
      if (!this.validateMaturity(this.maturities, maturityStr)) {
        maturityStr = this.maturities[0];
      }
    }
    this.changeMaturity(maturityStr);
  }

  changeMaturity(maturityStr: string) {
    this.tickerObj.Maturity = this.utils.ParseDate(maturityStr);
    this.logTickerObj("changeMaturity");

    this.calls = this.filterItems(this.spreads, maturityStr, "1");
    this.puts = this.filterItems(this.spreads, maturityStr, "2");
  }

  changeTicker(event: Event) {
    let ticker: string = this.tickersearch.nativeElement.value;

    this.route.navigate(['/', 'maxpain-history', ticker], { relativeTo: this.actRoute }).then(e => {
      if (e) {
        //console.log("Navigation is successful!");
      } else {
        //console.log("Navigation has failed!");
      }
    });
  }

  validateMaturity(maturities: string[], maturityStr: string): boolean {
    for (let i: number = 1; i <= maturities.length; i++) {
      if (maturityStr == maturities[i]) {
        return true;
      }
      return false;
    }
  }

  distinctMaturity(spreads: Spread[]) {
    let result: string[] = [];
    let map: any = new Map();
    for (let jsonObj of spreads) {
      let spread: Spread = <Spread>jsonObj;
      let maturity: Date = this.utils.ParseDate(spread.maturity);
      let maturityStr: string = this.utils.FormatDate(maturity, "MM/dd/yyyy");
      if (!map.has(maturityStr)) {
        map.set(maturityStr, true);    // set any value to Map
        result.push(maturityStr);
      }
    }
    return result;
  }

  filterItems(spreads: Spread[], maturityStr: string, optionType: string): Spread[] {
    let result: Spread[] = [];
    for (let jsonObj of spreads) {
      let spread: Spread = <Spread>jsonObj;
      let dte = this.utils.ParseDate(spread.maturity);
      let str = this.utils.FormatDate(dte, "MM/dd/yyyy");
      if (maturityStr == str && spread.optionType == optionType) {
        result.push(spread);
      }
    }

    return result;
  }

  logTickerObj(description: string) {
    let maturityStr: string = "undefined";
    if (this.tickerObj.Maturity) {
      maturityStr = this.utils.FormatDate(this.tickerObj.Maturity, "MM/dd/yyyy");
    }


    //console.log(description+":"
    //  +" Ticker= "+this.tickerObj.Ticker
    //  +" Maturity= "+maturityStr
    //  +" Strike= "+this.tickerObj.Strike);
  }

}



