import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'

import { MostActive } from "../models/most-active";

@Component( {
  selector: 'app-screener-child',
  templateUrl: './screener-child.component.html',
  styleUrls: ['./screener-child.component.scss']
})
export class ScreenerChildComponent implements OnInit {

  @Input() inJson: string;
  @Input() screenerType: string;
  @Input() nextMaturity: string;
  mostActives: MostActive[];

  constructor() { }

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
              this.mostActives = JSON.parse(this.inJson)
              //console.log("child " + this.mostActives)
            }
          }
        }
      }
    }
  }  
}
