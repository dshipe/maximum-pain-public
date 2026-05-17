
import { isPlatformBrowser } from '@angular/common';
import { OnInit, Component, AfterViewInit, Inject, PLATFORM_ID } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Observable } from 'rxjs'
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { SeoService } from '../services/seo.service';
import { UtilsService } from '../services/utils.service';
import { StateService } from '../services/state.service';
import { Ticker } from "../models/ticker";

@Component({
  selector: 'app-candlestick',
  templateUrl: './candlestick.component.html',
  styleUrls: ['./candlestick.component.scss']
})
export class CandlestickComponent implements OnInit {
public tickerForm: FormGroup;
public tickerObj: Ticker;
public ticker: string;
public hasError: boolean;
public errorMsg: string;
public isDebugHidden: boolean = true;
public startDate: string;
public numDays: number;
  
  constructor(
    private actRoute: ActivatedRoute, 
    private route: Router, 
    private data: DataService,
    private utils: UtilsService,
    private state: StateService,
    private title: Title,
    private seo: SeoService,
    @Inject(PLATFORM_ID) private platformId: Object) { 
    this.route.routeReuseStrategy.shouldReuseRoute = function() {
      return false;
    };
  }

  ngOnInit() {
    this.tickerObj = this.state.initialize(this.actRoute, this.utils);

    if (this.tickerObj.Ticker == "AIRBUS") {
      this.redirect("not-found", "");
      return;
    }

    if (!this.actRoute.snapshot.params.id) {
      this.redirect('candlestick', 'AAPL');
      return;
    }

    this.ticker = this.tickerObj.Ticker;
    this.title.setTitle(this.ticker + " Candlestick Chart");
    this.seo.updateMetaTags({
      title: this.ticker + " Candlestick Chart - Stock Price Analysis",
      description: `View ${this.ticker} candlestick chart with real-time price action and technical analysis.`,
      keywords: `${this.ticker}, candlestick chart, stock chart, technical analysis, price action`,
      url: `https://maximum-pain.com/candlestick/${this.ticker}`
    });
    this.createForm();
    this.bindForm();
    this.hasError = false;
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
    if (isPlatformBrowser(this.platformId)) { window.open(url, "_blank"); }
  }

  onClickDebug(event) {
    this.isDebugHidden = !this.isDebugHidden;
  }

  createForm(): void {
    const today = new Date();
    const ninetyDaysAgo = new Date(today);
    ninetyDaysAgo.setDate(today.getDate() - 90);
    
    const startDateString = this.formatDateForInput(ninetyDaysAgo);
    this.startDate = startDateString;
    this.numDays = 90;

    this.tickerForm = new FormGroup({
      "formTicker": new FormControl(),
      "formStartDate": new FormControl(startDateString),
      "formNumDays": new FormControl(90)
    });
  }

  bindForm(): void {
    this.tickerForm.controls["formTicker"].setValue(this.tickerObj.Ticker);
  }

  onDateRangeChange(event): void {
    const newStartDate = this.tickerForm.controls["formStartDate"].value;
    const newNumDays = this.tickerForm.controls["formNumDays"].value;
    
    // Validate the date format
    if (newStartDate && !this.isValidDateFormat(newStartDate)) {
      console.warn("Invalid date format. Please use YYYY-MM-DD");
      return;
    }
    
    if (newNumDays < 1) {
      console.warn("Number of days must be at least 1");
      return;
    }
    
    this.startDate = newStartDate;
    this.numDays = newNumDays;
  }

  private isValidDateFormat(dateString: string): boolean {
    const regex = /^\d{4}-\d{2}-\d{2}$/;
    if (!regex.test(dateString)) {
      return false;
    }
    
    const date = new Date(dateString);
    return date instanceof Date && !isNaN(date.getTime());
  }

  private formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  changeTicker(ticker: string) {
    this.redirect("candlestick", ticker);
  }

  redirect(path: string, params: string) {
    this.route.navigate(['/', path, params], { relativeTo: this.actRoute }).then(e => {
      if (e) {
        // Navigation successful
      } else {
        // Navigation failed
      }
    });    
  }
}
