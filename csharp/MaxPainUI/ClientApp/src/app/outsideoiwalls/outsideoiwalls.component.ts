import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { ActivatedRoute, Router} from '@angular/router';
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { OutsideOIWall } from "../models/outsideoiwall";

@Component( {
  selector: 'app-outsideoiwalls',
  templateUrl: './outsideoiwalls.component.html',
  styleUrls: ['./outsideoiwalls.component.scss']
})
export class OutsideoiwallsComponent implements OnInit {

  items: OutsideOIWall[];

  constructor(
    private data: DataService, 
    private actRoute: ActivatedRoute, 
    private route: Router,
    private title: Title) { }

  ngOnInit() {
    let type = this.actRoute.snapshot.params.id;  
        
    let observable$: Observable<Array<OutsideOIWall>> = 
      this.data.getOutsideOIWalls();
    observable$.subscribe(response => {
      this.items = response;
    });
   }
}
