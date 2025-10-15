import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { Ticker } from "../models/ticker";
import { SdlChn, Sdl } from "../models/straddle";

@Component( {
  selector: 'app-straddle',
  templateUrl: './straddle.component.html',
  styleUrls: ['./straddle.component.scss'],
  providers: [DataService],
})

export class StraddleComponent implements OnInit {

  @Input() inJson: string;
  public chain: SdlChn;
  public isEmpty: boolean = false;

  //added the data parameter
  constructor(private data: DataService, private utils: UtilsService) { }

  ngOnInit() {
  }

  ngOnChanges(changes: SimpleChanges)
  {
    for(const propName in changes) {    
      if(changes.hasOwnProperty(propName)) {
        switch(propName) {
          case "inJson": {

            if (this.inJson.length>0)
            {
              let jsonObj: SdlChn = JSON.parse(this.inJson);
              this.chain = new SdlChn(jsonObj);
              this.changeMaturityPost(this.chain);
            }
          }
        }
      }
    }
  }  

  changeMaturityPost(chain: SdlChn) {
  }
}  

