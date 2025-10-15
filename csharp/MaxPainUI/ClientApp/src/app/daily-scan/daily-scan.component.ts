import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges, HostListener } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { StateService } from '../services/state.service';
import { DailyScan } from '../models/daily-scan';
import { NONE_TYPE } from '@angular/compiler';

@Component( {
  selector: 'app-daily-scan',
  templateUrl: './daily-scan.component.html',
  styleUrls: ['./daily-scan.component.scss']
})
export class DailyScanComponent implements OnInit {

  public histories: Array<DailyScan>;
  public filteredHistories: Array<DailyScan>;
  public maturities: Array<string>;

  public tickerForm: FormGroup;
  public filterForm: FormGroup;
  public hasError: boolean = false;
  public errorMsg: string;

  public showProgress: boolean = false;
  public source: string = "";
  public watch: string = "";
  public minSMAVolume: string = "100000";
  public rsRating: string = "";
  public adr: string = "";
  public bbw: string = "";

  public showWatch: boolean = false;

  public buyPricePerc: number = 0.03;
  public buyVolumePerc: number = -0.95;
  public stopLossPerc: number = -0.07;
    
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

    let watch = this.actRoute.snapshot.queryParamMap.get('watch');
    this.showWatch = watch != null;
    console.log("showWatch: " + this.showWatch);
  }

  ngOnInit(): void {
    this.title.setTitle("Daily Scan");

    this.createForm();
    this.tickerForm.get('formMaturity').valueChanges
      .subscribe(content=>{
        this.changeMaturity(content);
      })

    this.filterForm.get('formWatch').valueChanges
      .subscribe(content => {
        this.changeFilter('formWatch', content);
      })
    this.filterForm.get('formSource').valueChanges
      .subscribe(content => {
        this.changeFilter('formSource', content);
      })
    this.filterForm.get('formMinSMAVolume').valueChanges
      .subscribe(content=>{
        this.changeFilter('formMinSMAVolume', content);
      })
    this.filterForm.get('formRSRating').valueChanges
      .subscribe(content => {
        this.changeFilter('formRSRating', content);
      })
    this.filterForm.get('formADR').valueChanges
      .subscribe(content => {
        this.changeFilter('formADR', content);
      })
    this.filterForm.get('formBBW').valueChanges
      .subscribe(content => {
        this.changeFilter('formBBW', content);
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

  createForm(): void {
    this.tickerForm = new FormGroup({
      "formMaturity": new FormControl(-1, [Validators.min(0)])
    });

    this.filterForm = new FormGroup({
      "formShowProgress": new FormControl(),
      "formAddTicker": new FormControl(),

      "formWatch": new FormControl(),
      "formSource": new FormControl(),
      "formMinSMAVolume": new FormControl(),
      "formRSRating": new FormControl(),
      "formADR": new FormControl(),
      "formBBW": new FormControl(),

      "formBuyPricePerc": new FormControl(),
      "formBuyVolumePerc": new FormControl(),
      "formStopLossPerc": new FormControl()
    });

  }

  bindForm(maturityStr): void {
    this.tickerForm.controls["formMaturity"].setValue(maturityStr);

    this.filterForm.controls["formShowProgress"].setValue(this.showProgress);

    this.filterForm.controls["formWatch"].setValue(this.watch);
    this.filterForm.controls["formSource"].setValue(this.source);
    this.filterForm.controls["formMinSMAVolume"].setValue(this.minSMAVolume);
    this.filterForm.controls["formRSRating"].setValue(this.rsRating);
    this.filterForm.controls["formADR"].setValue(this.adr);
    this.filterForm.controls["formBBW"].setValue(this.bbw);
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

  changeFilter(field: string, value: string): boolean {
    console.log("changeFilter: " + field + " : " + value)

    if (field == "formWatch") this.watch = value;
    if (field == "formSource") this.source = value;
    if (field == "formMinSMAVolume") this.minSMAVolume = value;
    if (field == "formRSRating") this.rsRating = value;
    if (field == "formADR") this.adr = value;
    if (field == "formBBW") this.bbw = value;

    this.applyFilter();
    return true;
  }

  applyFilter(): boolean {
    if (!this.histories) {
      return false;
    }

    this.filteredHistories = this.histories;
    if (this.watch && this.watch.length > 0) {
      let flag: boolean = (this.watch === 'true');
      if (flag) {
        this.filteredHistories = this.filteredHistories.filter(h => h.watchFlag == true);
      }
      else {
        this.filteredHistories = this.filteredHistories.filter(h => h.watchFlag == null || h.watchFlag == false);
      }
    }
    if (this.source && this.source.length > 0) {
      this.filteredHistories = this.filteredHistories.filter(h => h.source >= this.source);
    }
    if (this.minSMAVolume && this.minSMAVolume.length > 0) {
      this.filteredHistories = this.filteredHistories.filter(h => h.volume20 >= parseInt(this.minSMAVolume));
    }
    if (this.rsRating && this.rsRating.length > 0) {
      this.filteredHistories = this.filteredHistories.filter(h => h.rsRating >= parseFloat(this.rsRating));
    }
    if (this.adr && this.adr.length > 0) {
      this.filteredHistories = this.filteredHistories.filter(h => h.adr >= parseFloat(this.adr));
    }
    if (this.bbw && this.bbw.length > 0) {
      this.filteredHistories = this.filteredHistories.filter(h => h.bbw >= parseFloat(this.bbw));
    }

    console.log("this.histories.length: " + this.histories.length + " this.filteredHistories.length: " + this.filteredHistories.length);
    return true;
  }

  fetchHistory(maturityStr: string) {
    let observable$: Observable<Array<DailyScan>> =
      this.data.getDailyScan(maturityStr);
    observable$.subscribe(
      response => {
        this.histories = response;
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
        this.histories = response;
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
}


