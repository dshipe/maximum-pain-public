import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { ActivatedRoute, Router} from '@angular/router';
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { MostActive } from "../models/most-active";
import { ScreenerChildComponent } from '../screener-child/screener-child.component';

@Component( {
  selector: 'app-screener',
  templateUrl: './screener.component.html',
  styleUrls: ['./screener.component.scss']
})
export class ScreenerComponent implements OnInit {

  public screenerType: string;
  public mostActives: Array<MostActive>;
  public filtered: Array<MostActive>;
  public filteredAny: Array<MostActive>;
  public filteredJson: string;
  public filteredAnyJson: string;

  //added the data parameter
  constructor(
    private data: DataService, 
    private actRoute: ActivatedRoute, 
    private route: Router,
    private title: Title) { }

  ngOnInit() {
    let type = this.actRoute.snapshot.params.id;  
    let description = "";		
    
     this.screenerType = type;
    if(type.toLowerCase()=="changeprice") { this.screenerType="ChangePrice"; description="Change Price"; }
    if(type.toLowerCase()=="openinterest") { this.screenerType="OpenInterest"; description="Open Interest"; }
    if(type.toLowerCase()=="changeopeninterest") { this.screenerType="ChangeOpenInterest"; description="Change Open Interest"; }
    if(type.toLowerCase()=="volume") { this.screenerType="Volume"; description="Volume"; }
    if(type.toLowerCase()=="changevolume") { this.screenerType="ChangeVolume"; description="Change Volume"; }
    
    this.title.setTitle(description + " Option Screener");

    let observable$: Observable<Array<MostActive>> = 
      this.data.getMostActive();
    observable$.subscribe(response => {
      this.mostActives = response;
      //console.log(this.mostActives);

      this.filtered = this.mostActives.filter(x=>x.nextMaturity==true && x.queryType==this.screenerType);
      this.filteredAny = this.mostActives.filter(x=>x.nextMaturity==false && x.queryType==this.screenerType);
      //console.log(this.filtered);

      this.filteredJson = JSON.stringify(this.filtered);
      this.filteredAnyJson = JSON.stringify(this.filteredAny);


    });
   }
}
