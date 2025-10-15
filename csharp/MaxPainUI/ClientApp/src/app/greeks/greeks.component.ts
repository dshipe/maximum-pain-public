import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges, HostListener } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { StateService } from '../services/state.service';
import { Ticker } from "../models/ticker";
import { SdlChn, Sdl } from "../models/straddle";


@Component( {
  selector: 'app-greeks',
  templateUrl: './greeks.component.html',
  styleUrls: ['./greeks.component.scss'],
  providers: [DataService, UtilsService],
})
export class GreeksComponent implements OnInit {

  public tickerForm: FormGroup;
  public tickerObj: Ticker;
  public tickerObjJson: string;
  public chain: SdlChn;
  public filtered: SdlChn;
  public hasError: boolean = false;
  public errorMsg: string;

  public hasScrolledPast1: boolean=false;
  public hasScrolledPast2: boolean=false;
  public hasScrolledPast3: boolean=false;
    
  //added the data parameter
  constructor(
    private actRoute: ActivatedRoute, 
    private route: Router, 
    private readonly formBuilder: FormBuilder,
    private data: DataService,
    private utils: UtilsService,
    private state: StateService,
    private title: Title) { 
          
    // override the route reuse strategy
    this.route.routeReuseStrategy.shouldReuseRoute = function() {
      return false;
    };
  }

  ngOnInit() {
    this.tickerObj = this.state.initialize(this.actRoute, this.utils);
    this.title.setTitle(this.tickerObj.Ticker + " Stock Option Greeks");
    this.state.setTickerObj(this.tickerObj);

    this.createForm();
    this.bindForm();
    this.tickerForm.get('formMaturity').valueChanges
      .subscribe(content=>{
        this.logTickerObj("formMaturity valueChanges content="+content);
        this.changeMaturity(content);
        this.state.setTickerObj(this.tickerObj);
      })

    this.hasError = false;
    this.chain = this.state.getStraddleObj(this.tickerObj.Ticker);
    if (this.chain!=null)
    {
      console.log("straddle from state");
      this.initializeMaturity();  
      this.bindForm();
    }
    else
    {
      let observable$: Observable<SdlChn> = 
        this.data.getStraddle(this.tickerObj.Ticker);
      observable$.subscribe(
        response => {
          this.chain = response;
          if (!this.chain || this.chain.straddles.length==0)
          {
            this.hasError=true;
            this.errorMsg = `No data returned for ticker ${this.tickerObj.Ticker}`;
          }
          if (!this.hasError)
          {
            this.state.setStraddleObj(this.chain);
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
  }


  @HostListener('window:scroll', ['$event']) getScrollHeight(event) {
    //console.log(window.pageYOffset, event);

    if (window.pageYOffset>=100) this.hasScrolledPast1=true;
    if (window.pageYOffset>=500) this.hasScrolledPast2=true;
    if (window.pageYOffset>=1000) this.hasScrolledPast3=true;

    //console.log("window.pageYOffset="+window.pageYOffset
    //  +"  hasScrolledPast1="+this.hasScrolledPast1
    //  +"  hasScrolledPast2="+this.hasScrolledPast2
    //  +"  hasScrolledPast3="+this.hasScrolledPast3);
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
  
  initializeMaturity() {
    this.logTickerObj("initializeMaturity");

    // build array of expiration dates
    this.tickerObj.Maturities = this.utils.distinctMaturityStraddle(this.chain.straddles);
    let maturityStr: string = this.tickerObj.Maturities[0];

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
  }

  changeTicker(ticker: string)
  {
    this.redirect("greeks", ticker);
  }

  changeMaturity(maturityStr: string): boolean {
    if(!maturityStr) return false;

    this.logTickerObj("changeMaturity");

    this.filtered = new SdlChn(this.chain);
    this.filtered.straddles = this.utils.filterStraddle(this.filtered.straddles, maturityStr);

    this.tickerObj.Maturity = this.utils.ParseDate(maturityStr);
    this.tickerObj.JsonData = JSON.stringify(this.filtered);
    this.tickerObjJson = JSON.stringify(this.tickerObj);


    return true;
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
    /*
    let content: string = description+":"
      +" Ticker= "+this.tickerObj.Ticker
      +" Maturity= "+this.tickerObj.MaturityString
      +" Strike= "+this.tickerObj.Strike;
    if (this.tickerObj.JsonData) content += " JsonData= " + this.tickerObj.JsonData.substr(0,10);
    console.log(content);
    */    
  }    
}
