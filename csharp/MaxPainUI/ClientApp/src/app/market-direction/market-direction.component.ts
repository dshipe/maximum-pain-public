import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges, HostListener } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { StateService } from '../services/state.service';
import { MarketDirection } from '../models/market-direction';

@Component( {
  selector: 'app-market-direction',
  templateUrl: './market-direction.component.html',
  styleUrls: ['./market-direction.component.scss']
})
export class MarketDirectionComponent implements OnInit {

  public directions: Array<MarketDirection>;
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
    this.title.setTitle("Daily Scan");

    this.createForm();
    this.tickerForm.get('formMaturity').valueChanges
      .subscribe(content=>{
      })

    let observable$: Observable<Array<MarketDirection>> =
      this.data.getMarketDirection();
    observable$.subscribe(
      response => {
        console.log(response);
        this.directions = response;
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
  }

  bindForm(maturityStr): void {
    this.tickerForm.controls["formMaturity"].setValue(maturityStr);
  }
 
}


