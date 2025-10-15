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

@Component( {
  selector: 'app-download-csv',
  templateUrl: './download-csv.component.html',
  styleUrls: ['./download-csv.component.scss']
})
export class DownloadCsvComponent implements OnInit {
  public tickerForm: FormGroup;
  public tickerObj: Ticker;
  public hasError: boolean;
  public isDownloadProcessing: boolean = false;
 
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
    this.title.setTitle(this.tickerObj.Ticker + " Download");

    this.createForm();
	  this.bindForm();
    this.tickerForm.get('formMaturity').valueChanges
      .subscribe(content=>{
        this.logTickerObj("formMaturity valueChanges content="+content);
        this.changeMaturity(content);
        this.state.setTickerObj(this.tickerObj);
      })


    /*
    this.hasError=false;
    let observable2$: Observable<Array<OptionHistory>> = 
      this.data.getOptionHistory(this.tickerObj.Ticker);
    observable2$.subscribe(response => {
      this.quotes = response;
      if (!this.quotes || this.quotes.length==0) this.hasError=true;
      if (!this.hasError)
      {
      }
    });
    */
  }

  onKeydown(event) {
    this.logTickerObj("onKeydown");
    if (event.key === "Enter") {
      this.onSubmit(event);
    }
  }   
  
  onSubmit(event) {
    this.logTickerObj("onSubmit");
    let ticker: string = this.tickerForm.controls["formTicker"].value;
    this.changeTicker(ticker);
  }

  onClick(event) {
    this.logTickerObj("onClick");

    this.isDownloadProcessing = true;
    let observable$: Observable<any> = 
      this.data.tdacsv(this.tickerObj.Ticker);
    observable$.subscribe(response => {
      this.downloadFile(response);
      this.isDownloadProcessing = false;
    });
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
  }

  changeTicker(ticker: string)
  {
    this.redirect("download-csv", ticker);
  }

  changeMaturity(maturityStr: string): void {
    this.logTickerObj("changeMaturity");
  } 

  downloadFile(data: any) {
    this.logTickerObj("downloadFile");
    const replacer = (key, value) => value === null ? '' : value; // specify how you want to handle null values here
    const header = Object.keys(data[0]);
    let csv = data.map(row => header.map(fieldName => JSON.stringify(row[fieldName], replacer)).join(','));
    csv.unshift(header.join(','));
    let csvArray = csv.join('\r\n');

    var blob = new Blob([csvArray], {type: 'text/csv' }),
    url = window.URL.createObjectURL(blob);
    
    //window.open(url);

    var a = document.createElement('a');
    a.href = url;
    a.download = this.tickerObj.Ticker + ".csv";
    a.click();
    window.URL.revokeObjectURL(url);
    a.remove();    
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