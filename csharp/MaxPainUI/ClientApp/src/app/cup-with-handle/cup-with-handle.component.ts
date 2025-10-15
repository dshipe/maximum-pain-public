import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges, HostListener } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { StateService } from '../services/state.service';
import { CupWithHandleHistory } from '../models/cup-with-handle-history';

@Component( {
  selector: 'app-cup-with-handle',
  templateUrl: './cup-with-handle.component.html',
  styleUrls: ['./cup-with-handle.component.scss']
})
export class CupWithHandleComponent implements OnInit {

  public histories: Array<CupWithHandleHistory>;
  public maturities: Array<string>;

  public tickerForm: FormGroup;
  public hasError: boolean = false;
  public errorMsg: string;
    
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

  ngOnInit(): void {
    this.title.setTitle("Cup with Handle");

    this.createForm();
    this.tickerForm.get('formMaturity').valueChanges
      .subscribe(content=>{
        this.changeMaturity(content);
      })
    
    let observable$: Observable<Array<CupWithHandleHistory>> =
      this.data.getCupWithHandleHistory("01/01/1900");
    observable$.subscribe(
      response => {
        let dates: Array<string> = [];
        for (let cwh of response) 
        {
          //console.log(cwh.midnight + "\n" + typeof(cwh.midnight));
          let maturity: Date =new Date(cwh.midnight.toString());
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
      "formMaturity": new FormControl(-1, [Validators.min(0)]),
    });
  }

  bindForm(maturityStr): void {
    this.tickerForm.controls["formMaturity"].setValue(maturityStr);
  }
  
  changeMaturity(maturityStr: string): boolean {
    if(!maturityStr) return false;
    this.fetchHistory(maturityStr);
    return true;
  } 

  fetchHistory(maturityStr: string) {
    let observable$: Observable<Array<CupWithHandleHistory>> =
      this.data.getCupWithHandleHistory(maturityStr);
    observable$.subscribe(
      response => {
        this.histories = response;
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
}


