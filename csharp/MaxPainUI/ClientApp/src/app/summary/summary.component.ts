import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'

import { DataService } from '../services/data.service';
import { Ticker } from "../models/ticker";
import { MPChn, MPItem } from "../models/MaxPainItem";
import { UtilsService } from '../services/utils.service';

@Component( {
  selector: 'app-summary',
  templateUrl: './summary.component.html',
  styleUrls: ['./summary.component.scss']
})
export class SummaryComponent implements OnInit {

  @Input() inJson: string
  public chain: MPChn;
  public totalOI: number;
  
  //added the data parameter
  constructor(private data: DataService, private utils: UtilsService) { }

  ngOnInit() {
  }

  ngOnChanges(changes: SimpleChanges)
  {
    for(const propName in changes) {    
      if(changes.hasOwnProperty(propName)) {
        //console.log("ngOnChanges: propName="+propName);
        switch(propName) {
          case "inJson": {
            this.parseInput(this.inJson);
          }
        }
      }
    }
  }

  parseInput(json: string): boolean
  {
    if (json.length<20) return false;
    this.chain = new MPChn(JSON.parse(json));
  }
}  