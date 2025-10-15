import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { UtilsService } from '../../services/utils.service';
import { DataService } from '../../services/data.service';
import { Hop, HopSummary, HopAgent } from '../../models/hop';

@Component( {
  selector: 'app-hop',
  templateUrl: './hop.component.html',
  styleUrls: ['./hop.component.scss'],
  providers: [DataService],
})
export class HopComponent implements OnInit {

  public hops: Array<Hop>;
  public summaries: Array<HopSummary>;
  public agents: Array<HopAgent>;
  public isLoading: boolean = true;

  constructor(
    private data: DataService,
    private utils: UtilsService
  ) { }

  ngOnInit() {
    this.isLoading = true;

    let observable1$: Observable<Array<HopSummary>> = 
      this.data.getHopSummary();
    observable1$.subscribe(response => {
      this.summaries = response;
    });

    let observable2$: Observable<Array<HopAgent>> = 
    this.data.getHopAgent();
    observable2$.subscribe(response => {
      this.agents = response;
      this.isLoading = false;
    });


    let observable3$: Observable<Array<Hop>> = 
    this.data.getHopDetail();
    observable3$.subscribe(response => {
      this.hops = response;
      this.isLoading = false;
    });


  }
}
