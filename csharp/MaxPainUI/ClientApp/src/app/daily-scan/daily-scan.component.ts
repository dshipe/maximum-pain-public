import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges, HostListener, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";
import { SeoService } from '../services/seo.service';

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { StateService } from '../services/state.service';
import { DailyScan } from '../models/daily-scan';
import { Daily } from '../models/daily';
import { NONE_TYPE } from '@angular/compiler';

@Component( {
  selector: 'app-daily-scan',
  templateUrl: './daily-scan.component.html',
  styleUrls: ['./daily-scan.component.scss']
})
export class DailyScanComponent implements OnInit, AfterViewInit {

  @ViewChild('chartContainer') chartContainer: ElementRef;
  public chartVisible: boolean = false;

  public currentIndex: number = 0;
  public histories: Array<DailyScan>;
  public filteredHistories: Array<DailyScan>;
  public maturities: Array<string>;

  public tickerForm: FormGroup = new FormGroup({})
  public filterForm: FormGroup = new FormGroup({})
  public hasError: boolean = false;
  public errorMsg: string;

  public showProgress: boolean = false;
  public showWatch: boolean = false;

  public source: string = "";
  public watch: boolean = false;
  public alert: boolean = false;
  public minSMAVolume: string = "100000";
  public rsi: string = "";
  public adr: string = "";
  public adrPerc: string = "";
  public atrDrop: boolean = false;
  public flatChannel: boolean = false;
  public higherLows: boolean = false;
  public movingAvg: boolean = false;
  public pricePattern: boolean = false;
  public volumeReqs: boolean = false;
  public marketCap: boolean = false;
  public avoidGap: boolean = false;
  public rsiMomentum: boolean = false;
  public proPerc: string = "";
  
  public buyPricePerc: number = 0.03;
  public buyVolumePerc: number = -0.95;
  public stopLossPerc: number = -0.07;

  public dailyJson: string = "";
    
  //added the data parameter
  constructor(
    private actRoute: ActivatedRoute, 
    private route: Router, 
    private readonly formBuilder: FormBuilder,
    private data: DataService,
    private utils: UtilsService,
    private state: StateService,
    private title: Title,
    private seo: SeoService,
    @Inject(PLATFORM_ID) private platformId: Object) { 
          
        this.createForm(); // initialize form before template binding (SSR safe)
    // override the route reuse strategy
    this.route.routeReuseStrategy.shouldReuseRoute = function() {
      return false;
    };

    // NOTE: Do NOT initialize showWatch from ActivatedRoute here. The
    // constructor runs during SSR/prerender where no query string exists,
    // so any value computed here gets baked into the static HTML served
    // by CloudFront. showWatch is initialized to false above and updated
    // on the browser in ngOnInit from window.location.search.
  }

  ngOnInit(): void {
    this.seo.updateMetaTags({
      title: "Daily Max Pain Scan | Maximum-Pain.com",
      description: "Daily scan of max pain prices and open interest across all optionable stocks. Updated each trading day.",
      url: "https://maximum-pain.com/daily-scan"
    })
    this.createForm();

    // Skip API calls during SSR — prerender only needs SEO metadata
    if (!isPlatformBrowser(this.platformId)) { return; }

    // Re-evaluate the ?watch query string on the client. The constructor runs
    // during prerender (no query string is available), so the value captured
    // there is baked into the static HTML served by CloudFront.
    //
    // We read window.location.search directly rather than relying solely on
    // ActivatedRoute.queryParamMap because:
    //   1. CloudFront serves the cached prerendered HTML regardless of query
    //      string, so the hydrated app starts with showWatch=false.
    //   2. During hydration, ActivatedRoute may not re-emit queryParamMap if
    //      the Router considers the route unchanged.
    // window.location.search is always the source of truth in the browser.
    const applyWatchFromUrl = () => {
      try {
        const params = new URLSearchParams(window.location.search);
        this.showWatch = params.has('watch');
        console.log("showWatch (from window.location): " + this.showWatch);
      } catch (e) {
        console.log("Failed to parse window.location.search: " + e);
      }
    };
    applyWatchFromUrl();

    // Also subscribe to future navigations that only change the query string.
    this.actRoute.queryParamMap.subscribe(params => {
      if (params.has('watch')) {
        this.showWatch = true;
      }
      console.log("showWatch (queryParamMap): " + this.showWatch);
    });

    this.tickerForm.get('formMaturity').valueChanges
      .subscribe(content=>{
        this.changeMaturity(content);
      })

    this.filterForm.get('formWatch').valueChanges
      .subscribe(content => {
        this.changeFilter('formWatch', content);
      })
    this.filterForm.get('formAlert').valueChanges
      .subscribe(content => {
        this.changeFilter('formAlert', content);
      })
    this.filterForm.get('formSource').valueChanges
      .subscribe(content => {
        this.changeFilter('formSource', content);
      })
    this.filterForm.get('formMinSMAVolume').valueChanges
      .subscribe(content=>{
        this.changeFilter('formMinSMAVolume', content);
      })
    this.filterForm.get('formRSI').valueChanges
      .subscribe(content => {
        this.changeFilter('formRSI', content);
      })
    this.filterForm.get('formADR').valueChanges
      .subscribe(content => {
        this.changeFilter('formADR', content);
      })
    this.filterForm.get('formADRPerc').valueChanges
      .subscribe(content => {
        this.changeFilter('formADRPerc', content);
      })
    this.filterForm.get('formAtrDrop').valueChanges
      .subscribe(content => {
        this.changeFilter('formAtrDrop', content);
      })
    this.filterForm.get('formFlatChannel').valueChanges
      .subscribe(content => {
        this.changeFilter('formFlatChannel', content);
      })
    this.filterForm.get('formHigherLows').valueChanges
      .subscribe(content => {
        this.changeFilter('formHigherLows', content);
      })
    this.filterForm.get('formMovingAvg').valueChanges
      .subscribe(content => {
        this.changeFilter('formMovingAvg', content);
      })
    this.filterForm.get('formPricePattern').valueChanges
      .subscribe(content => {
        this.changeFilter('formPricePattern', content);
      })
    this.filterForm.get('formVolumeReqs').valueChanges
      .subscribe(content => {
        this.changeFilter('formVolumeReqs', content);
      })
    this.filterForm.get('formMarketCap').valueChanges
      .subscribe(content => {
        this.changeFilter('formMarketCap', content);
      })
    this.filterForm.get('formAvoidGap').valueChanges
      .subscribe(content => {
        this.changeFilter('formAvoidGap', content);
      })
    this.filterForm.get('formRsiMomentum').valueChanges
      .subscribe(content => {
        this.changeFilter('formRsiMomentum', content);
      })
    this.filterForm.get('formProPerc').valueChanges
      .subscribe(content => {
        this.changeFilter('formProPerc', content);
      })
    this.filterForm.get('formBuyPricePerc').valueChanges
      .subscribe(content => {
        this.changeBuyPoint('formBuyPricePerc', content);
      })
    this.filterForm.get('formBuyVolumePerc').valueChanges
      .subscribe(content => {
        this.changeBuyPoint('formBuyVolumePerc', content);
      })
    this.filterForm.get('formStopLossPerc').valueChanges
      .subscribe(content => {
        this.changeBuyPoint('formStopLossPerc', content);
      })

    let observable$: Observable<Array<DailyScan>> =
      this.data.getDailyScanDates();
    observable$.subscribe(
      response => {
        let dates: Array<string> = [];
        for (let item of response) 
        {
          //console.log(item.midnight + "\n" + typeof(item.midnight));
          let maturity: Date = new Date(item.date.toString());
          //console.log(maturity + "\n" + typeof(maturity));
          let maturityStr: string = this.utils.FormatDate(maturity, "MM/dd/yyyy");
          //console.log(maturityStr + "\n" + typeof(maturityStr));
          dates.push(maturityStr);
        }
        this.maturities = dates.filter(function(value, index){ return dates.indexOf(value) == index });
        this.bindForm(this.maturities[0])
      },
      error => {
        console.log(error);
        this.hasError=true;
        this.errorMsg = error.message;
        //this.data.postMessage(this.errorMsg, error.message);
      });
    }

  ngAfterViewInit(): void {
    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          this.chartVisible = true;
          observer.disconnect();
        }
      });
    });

    if (this.chartContainer) {
      observer.observe(this.chartContainer.nativeElement);
    }
  }

  createForm(): void {
    this.tickerForm = new FormGroup({
      "formMaturity": new FormControl(-1, [Validators.min(0)])
    });

    this.filterForm = new FormGroup({
      "formShowProgress": new FormControl(),
      "formAddTicker": new FormControl(),

      "formWatch": new FormControl(),
      "formAlert": new FormControl(),
      "formSource": new FormControl(),
      "formMinSMAVolume": new FormControl(),
      "formRSI": new FormControl(),
      "formADR": new FormControl(),
      "formADRPerc": new FormControl(),
      "formAtrDrop": new FormControl(),
      "formFlatChannel": new FormControl(),
      "formHigherLows": new FormControl(),
      "formMovingAvg": new FormControl(),
      "formPricePattern": new FormControl(),
      "formVolumeReqs": new FormControl(),
      "formMarketCap": new FormControl(),
      "formAvoidGap": new FormControl(),
      "formRsiMomentum": new FormControl(),
      "formProPerc": new FormControl(),
      "formBuyPricePerc": new FormControl(),
      "formBuyVolumePerc": new FormControl(),
      "formStopLossPerc": new FormControl()
    });

  }

  bindForm(maturityStr): void {
    this.tickerForm.controls["formMaturity"].setValue(maturityStr);

    this.filterForm.controls["formShowProgress"].setValue(this.showProgress);

    this.filterForm.controls["formWatch"].setValue(this.watch);
    this.filterForm.controls["formAlert"].setValue(this.alert);
    this.filterForm.controls["formSource"].setValue(this.source);
    this.filterForm.controls["formMinSMAVolume"].setValue(this.minSMAVolume);
    this.filterForm.controls["formRSI"].setValue(this.rsi);
    this.filterForm.controls["formADR"].setValue(this.adr);
    this.filterForm.controls["formADRPerc"].setValue(this.adrPerc);
    this.filterForm.controls["formAtrDrop"].setValue(this.atrDrop);
    this.filterForm.controls["formFlatChannel"].setValue(this.flatChannel);
    this.filterForm.controls["formHigherLows"].setValue(this.higherLows);
    this.filterForm.controls["formMovingAvg"].setValue(this.movingAvg);
    this.filterForm.controls["formPricePattern"].setValue(this.pricePattern);
    this.filterForm.controls["formVolumeReqs"].setValue(this.volumeReqs);
    this.filterForm.controls["formMarketCap"].setValue(this.marketCap);
    this.filterForm.controls["formAvoidGap"].setValue(this.avoidGap);
    this.filterForm.controls["formRsiMomentum"].setValue(this.rsiMomentum);
    this.filterForm.controls["formProPerc"].setValue(this.proPerc);
    this.filterForm.controls["formBuyPricePerc"].setValue(this.buyPricePerc);
    this.filterForm.controls["formBuyVolumePerc"].setValue(this.buyVolumePerc);
    this.filterForm.controls["formStopLossPerc"].setValue(this.stopLossPerc);
  }
  
  changeMaturity(maturityStr: string): boolean {
    if(!maturityStr) return false;
    this.fetchHistory(maturityStr);
    return true;
  } 

  changeProgress(b: boolean): boolean {
    console.log("changeProgress: " + b)
    this.showProgress = b;
    return true;
  }

  updateWatch(id: number): boolean {
    let item: DailyScan = this.histories.find(x => x.id == id);
    let flag: boolean = item.watchFlag;
    if (flag == null) flag = false;
    flag = !flag;
    console.log("updateWatch: id=" + id + " : flag=" + flag);

    item.watchFlag = flag;
    this.applyFilter();

    let observable$: Observable<Array<DailyScan>> =
      this.data.dailyScanUpdateWatch(id, flag);
    observable$.subscribe(
      response => {
        //console.log(response);
      },
      error => {
        this.hasError = true;
        this.errorMsg = `dailyScanUpdateWatch Server Error`;
        //this.data.postMessage(this.errorMsg, error.message);
      });

    return true;
  }

  changeBuyPoint(field: string, value: string): boolean {
    console.log("changeBuyPoint: " + field + " : " + value)
    if (!value || value.length == 0) return false;

    if (field == "formBuyPricePerc") this.buyPricePerc = parseFloat(value);
    if (field == "formStopLossPerc") this.stopLossPerc = parseFloat(value);
    if (field == "formBuyVolumePerc") this.buyVolumePerc = parseFloat(value);

    return true;
  }

  changeFilter(field: string, value: any): boolean {
    console.log("changeFilter: " + field + " : " + value)

    if (field == "formWatch") this.watch = value === true;
    if (field == "formAlert") this.alert = value === true;
    if (field == "formSource") this.source = value;
    if (field == "formMinSMAVolume") this.minSMAVolume = value;
    if (field == "formRSI") this.rsi = value;
    if (field == "formADR") this.adr = value;
    if (field == "formADRPerc") this.adrPerc = value;
    if (field == "formAtrDrop") this.atrDrop = value === true;
    if (field == "formFlatChannel") this.flatChannel = value === true;
    if (field == "formHigherLows") this.higherLows = value === true;
    if (field == "formMovingAvg") this.movingAvg = value === true;
    if (field == "formPricePattern") this.pricePattern = value === true;
    if (field == "formVolumeReqs") this.volumeReqs = value === true;
    if (field == "formMarketCap") this.marketCap = value === true;
    if (field == "formAvoidGap") this.avoidGap = value === true;
    if (field == "formRsiMomentum") this.rsiMomentum = value === true;
    if (field == "formProPerc") this.proPerc = value;

    this.applyFilter();
    this.currentIndex = 0;
    return true;
  }

  applyFilter(): boolean {
    if (!this.histories) {
      return false;
    }

    this.filteredHistories = this.histories;
    if (this.watch) {
      this.filteredHistories = this.filteredHistories.filter(h => h.watchFlag == true);
    }
    if (this.alert) {
      this.filteredHistories = this.filteredHistories.filter(h => h.hasAlerted == true);
    }
    if (this.source && this.source.length > 0) {
      this.filteredHistories = this.filteredHistories.filter(h => h.source >= this.source);
    }
    if (this.minSMAVolume && this.minSMAVolume.length > 0) {
      this.filteredHistories = this.filteredHistories.filter(h => h.volume20 >= parseInt(this.minSMAVolume));
    }
    if (this.rsi && this.rsi.length > 0) {
      this.filteredHistories = this.filteredHistories.filter(h => h.rsi >= parseFloat(this.rsi));
    }
    if (this.adr && this.adr.length > 0) {
      this.filteredHistories = this.filteredHistories.filter(h => h.adr >= parseFloat(this.adr));
    }
    if (this.adrPerc && this.adrPerc.length > 0) {
      this.filteredHistories = this.filteredHistories.filter(h => h.adr / h.price * 100 >= parseFloat(this.adrPerc));
    }
    if (this.atrDrop) {
      this.filteredHistories = this.filteredHistories.filter(h => h.flagAtrDrop == true);
    }
    if (this.flatChannel) {
      this.filteredHistories = this.filteredHistories.filter(h => h.flagFlatChannel == true);
    }
    if (this.higherLows) {
      this.filteredHistories = this.filteredHistories.filter(h => h.flagHigherLows == true);
    }
    if (this.movingAvg) {
      this.filteredHistories = this.filteredHistories.filter(h => h.flagMovingAverages == true);
    }
    if (this.pricePattern) {
      this.filteredHistories = this.filteredHistories.filter(h => h.flagPricePattern == true);
    }
    if (this.volumeReqs) {
      this.filteredHistories = this.filteredHistories.filter(h => h.flagVolumeRequirements == true);
    }
    if (this.marketCap) {
      this.filteredHistories = this.filteredHistories.filter(h => h.flagMarketCap == true);
    }
    if (this.avoidGap) {
      this.filteredHistories = this.filteredHistories.filter(h => h.flagAvoidGapDown == true);
    }
    if (this.rsiMomentum) {
      this.filteredHistories = this.filteredHistories.filter(h => h.flagRsiMomentum == true);
    }
    if (this.proPerc && this.proPerc.length > 0) {
      this.filteredHistories = this.filteredHistories.filter(h => h.progressPercent >= parseFloat(this.proPerc));
    }

    console.log("this.histories.length: " + this.histories.length + " this.filteredHistories.length: " + this.filteredHistories.length);
    return true;
  }

  fetchHistory(maturityStr: string) {
    let observable$: Observable<Array<DailyScan>> =
      this.data.getDailyScan(maturityStr);
    observable$.subscribe(
      response => {
        this.histories = response.map(item => new DailyScan(item));
        this.changeFilter('fromMinSMAVolume', this.minSMAVolume);
        if (!this.histories || this.histories.length == 0)
        {
          this.hasError=true;
          this.errorMsg = `No data returned`;
        }
        if (!this.hasError) {
        }
      },
      error => {
        this.hasError=true;
        this.errorMsg = `Server Error getting data`;
        //this.data.postMessage(this.errorMsg, error.message);
      });    
  }

  addTicker()
  {
    let ticker = this.filterForm.get('formAddTicker').value;

    let observable$: Observable<Array<DailyScan>> =
      this.data.addDailyScan(ticker);
    observable$.subscribe(
      response => {
        this.histories = response.map(item => new DailyScan(item));
        this.changeFilter('fromMinSMAVolume', this.minSMAVolume);
        if (!this.histories || this.histories.length == 0) {
          this.hasError = true;
          this.errorMsg = `No data returned`;
        }
        if (!this.hasError) {
        }
      },
      error => {
        this.hasError = true;
        this.errorMsg = `Server Error adding ticker`;
        console.log(`Server Error adding ticker`);
      });
  }

  goToPrevious(): void {
    if (this.currentIndex > 0) {
      this.currentIndex--;
    }
  }

  goToNext(): void {
    if (this.currentIndex < this.filteredHistories.length - 1) {
      this.currentIndex++;
    }
  }
}
