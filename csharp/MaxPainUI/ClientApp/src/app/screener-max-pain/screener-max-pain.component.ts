import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { ActivatedRoute, Router} from '@angular/router';
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { ImportMaxPain } from "../models/import-max-pain";

@Component( {
  selector: 'app-screener-max-pain',
  templateUrl: './screener-max-pain.component.html',
  styleUrls: ['./screener-max-pain.component.scss']
})
export class ScreenerMaxPainComponent implements OnInit {
  public pageForm: FormGroup;
  public maturities: Array<string>;
  public pains: Array<ImportMaxPain>;
  public filtered: Array<ImportMaxPain>;

 //added the data parameter
 constructor(
  private data: DataService, 
  private actRoute: ActivatedRoute, 
  private route: Router,
  private title: Title) { }

  ngOnInit() {
    this.title.setTitle("Max Pain Option Screener");
    this.createForm();

    let observable$: Observable<Array<ImportMaxPain>> = 
      this.data.screenerMaxPain();
    observable$.subscribe(response => {
      this.pains = response;

      this.maturities = this.distinctMaturity();
      this.bindForm();
      this.filtered = this.filterPains();
    });

    this.pageForm.valueChanges
      .subscribe(content=>{
        this.filtered = this.filterPains();
      })
  }


  createForm(): void {
    this.pageForm = new FormGroup({
      "formMaturity": new FormControl(-1, [Validators.min(0)]),
      "formMinimumOI": new FormControl(),
      "formMinPercDiff": new FormControl()
    });
  }

  bindForm(): void {
    this.pageForm.controls["formMaturity"].setValue(this.maturities[0]);
    this.pageForm.controls["formMinimumOI"].setValue("10000");
    this.pageForm.controls["formMinPercDiff"].setValue("10");
  }

  filterPains(): Array<ImportMaxPain> {
    let m: string = this.pageForm.controls["formMaturity"].value;
    let minOI: number = Number(this.pageForm.controls["formMinimumOI"].value);
    let percDiff: number = Number(this.pageForm.controls["formMinPercDiff"].value);
    let result: Array<ImportMaxPain> = this.pains.filter(x => x.maturity==m && x.totalOI()>=minOI && x.percentDifference()>=percDiff);
    return result;
  }

  distinctMaturity() {
    let result: Array<string> = [];
    let map: any = new Map();
    for (let jsonObj of this.pains) {
      let pain: ImportMaxPain = new ImportMaxPain(jsonObj);
      let m: string = pain.maturity;
      if (!map.has(m)) {
        map.set(m, true);    // set any value to Map
        result.push(m);
      }
    }
    return result;
  }  
}


