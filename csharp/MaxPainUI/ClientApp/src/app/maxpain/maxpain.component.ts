import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { Ticker } from "../models/ticker";
import { MPChn, MPItem } from "../models/MaxPainItem";

@Component( {
  selector: 'app-maxpain',
  templateUrl: './maxpain.component.html',
  styleUrls: ['./maxpain.component.scss'],
  providers: [DataService, UtilsService],
})
export class MaxpainComponent implements OnInit {

  @Input() inJson: string
  public chain: MPChn;


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
