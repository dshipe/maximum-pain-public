import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { StateService } from '../services/state.service';
import { Ticker } from "../models/ticker";
import { MaxpainHistory } from '../models/maxpain-history';

@Component( {
  selector: 'app-maxpain-history',
  templateUrl: './maxpain-history.component.html',
  styleUrls: ['./maxpain-history.component.scss']
})
export class MaxpainHistoryComponent implements OnInit {

  public tickerForm: FormGroup;
  public tickerObj: Ticker = new Ticker(this.utils);
  public tickerObjJson: string;
  public quotes: Array<MaxpainHistory>;
  public filtered: Array<MaxpainHistory>;
  public hasError: boolean;
  public errorMsg: string;

  //added the data parameter
  constructor(
    private actRoute: ActivatedRoute,
    private route: Router,
    private data: DataService,
    private utils: UtilsService,
    private state: StateService,
    private title: Title) {

    // override the route reuse strategy
    this.route.routeReuseStrategy.shouldReuseRoute = function () {
      return false;
    };
  }


  ngOnInit() {
    this.tickerObj = this.state.initialize(this.actRoute, this.utils);
    this.title.setTitle(this.tickerObj.Ticker + " Max Pain History");
    this.state.setTickerObj(this.tickerObj);

    this.createForm();
	  this.bindForm();
    this.tickerForm.get('formMaturity').valueChanges
      .subscribe(content=>{
        if (content!=null)
        {
          this.logTickerObj("formMaturity valueChanges content="+content);
          this.changeMaturity(content);
          this.state.setTickerObj(this.tickerObj);
        }
      });

    this.hasError = false;
    let observable$: Observable<Array<MaxpainHistory>> = 
      this.data.getMaxpainHistory(this.tickerObj.Ticker);
    observable$.subscribe(
      response => {
        this.quotes = response;
        if (!this.quotes || this.quotes.length==0) 
        {
          this.hasError=true;
          this.errorMsg = `No data returned for ticker ${this.tickerObj.Ticker}`;
        }
        if (!this.hasError)
        {
          this.initializeMaturity();  
          this.bindForm();
        }
      },
      error => {
        this.hasError=true;
        this.errorMsg = `Server Error getting data for ${this.tickerObj.Ticker}`;
        //this.data.postMessage(this.errorMsg, error.message);
      });
  }

  onKeydown(event) {
    if (event.key === "Enter") {
      this.onSubmit(event);
    }
  }   

  onSubmit(event) {
    let ticker: string = this.tickerForm.controls["formTicker"].value;
    this.changeTicker(ticker);
  }

  onSearch(event) {
    let url: string = "https://www.schwab.wallst.com/research/Client/Content/Documents/SchwabSymbolLookup.html?criteria=CGK&filter=STK,MFD,ETF,BND,PFD,IDX&newsite=y&callbackDomains=client,y%7Cclient,y&ResourceKey=DetailQuote&site=DWT&fieldId=ccSymbolInput&invoker=68747470733A2F2F7777772E7363687761622E77616C6C73742E636F6D2F72657365617263682F436C69656E742F53796D626F6C2F496E76616C696453796D626F6C3F5858583130335F4E634E645078476E55684C48493561486B354C30767856436251474E656A74766B5077555038673477356C754E2F4750316278642B394365785461372B2F4F4C4833563051672F794A7938485665316956466161466E5246355856464D786B6155596F39392B707730523151674969466F4F4F2F4F305977384D662F2F2F46364C4D436359764F343946476C3739365A6D79562B333434487A77545042624C552F756D3134646A6E6E585766577750726E7055536B41566C77304277552B57483175316446436D5764714C626A58624372657A3058413D3D2670333D592673796D626F6C3D43474B265F50433D495241";
    window.open(url, "_blank");
  }

  createForm(): void {
    this.logTickerObj("createForm");
    this.tickerForm = new FormGroup({
      "formTicker": new FormControl(),
      "formMaturity": new FormControl(-1, [Validators.min(0)]),
      "formStrike": new FormControl(-1, [Validators.min(0)]),
    });
  }

  bindForm(): void {
    this.logTickerObj("bindForm");
    this.tickerForm.controls["formTicker"].setValue(this.tickerObj.Ticker);
    this.tickerForm.controls["formMaturity"].setValue(this.tickerObj.MaturityString);
  }
  
  initializeMaturity(): boolean {
    if(this.quotes.length==0) return false;

    this.logTickerObj("initializeMaturity");

    // build array of expiration dates
    this.tickerObj.Maturities = this.distinctMaturity(this.quotes);
    let maturityStr: string = this.tickerObj.Maturities[this.tickerObj.Maturities.length-1];

    // if the ticker object already has an expiration, validate and use it
    if(this.tickerObj.Maturity)
    {
      let m: string = this.utils.FormatDate(this.tickerObj.Maturity, "MM/dd/yyyy");
      if(this.utils.validateMaturity(this.tickerObj.Maturities, m))
      {
        maturityStr = m;
      }
    }

    // set the value on the ticker object
    this.tickerObj.Maturity = this.utils.ParseDate(maturityStr);

    return true;
  }

  changeTicker(ticker: string): void
  {
    this.redirect("maxpain-history", ticker);
  }

  changeMaturity(maturityStr: string): boolean {
    if(!maturityStr) return false;

    this.logTickerObj("changeMaturity");

    this.filtered = this.filterItems(this.quotes, maturityStr);
    let data: string = JSON.stringify(this.filtered);

    this.tickerObj.Maturity = this.utils.ParseDate(maturityStr);
    this.tickerObj.JsonData = data;

    this.tickerObjJson = JSON.stringify(this.tickerObj);

    return true;
  } 




  distinctMaturity(quotes: Array<MaxpainHistory>): Array<string> {
    console.log(JSON.stringify(quotes));

    let result: string[] = [];
    let map: any = new Map();
    for (let jsonObj of quotes) {
      let quote: MaxpainHistory = <MaxpainHistory>jsonObj;
      let maturity: Date = this.utils.ParseDate(quote.m);
      let maturityStr: string = this.utils.FormatDate(maturity, "MM/dd/yyyy");
      if (!map.has(maturityStr)) {
        map.set(maturityStr, true);    // set any value to Map
        result.push(maturityStr);
      }
    }
    return result;
  }

  filterItems(items: MaxpainHistory[], maturityStr: string): MaxpainHistory[] {
    let result: MaxpainHistory[] = [];
    for (let jsonObj of items) {
      let item: MaxpainHistory = <MaxpainHistory>jsonObj;
      let dte = this.utils.ParseDate(item.m);
      let str = this.utils.FormatDate(dte, "MM/dd/yyyy");
      if (maturityStr == str) {
        result.push(item);
      }
    }

    return result;
  }


  redirect(path: string, params: string)
  {
    this.route.navigate(['/', path, params], { relativeTo: this.actRoute }).then(e => {
      if (e) {
        //console.log("Navigation is successful!");
      } else {
        //console.log("Navigation has failed!");
      }
    });    
  }    

  logTickerObj(description:string): void
  {
    let content: string = description+":"
      +" Ticker= "+this.tickerObj.Ticker
      +" Maturity= "+this.tickerObj.MaturityString
      +" Strike= "+this.tickerObj.Strike;
    if (this.tickerObj.JsonData) content += " JsonData= " + this.tickerObj.JsonData.substr(0,10);
    console.log(content);
  }    
}
