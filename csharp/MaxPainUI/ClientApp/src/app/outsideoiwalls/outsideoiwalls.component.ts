import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { ActivatedRoute, Router} from '@angular/router';
import { Title } from "@angular/platform-browser";
import { SeoService } from '../services/seo.service';

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
    private title: Title,
    private seo: SeoService,
    @Inject(PLATFORM_ID) private platformId: Object) { }

  ngOnInit() {
    this.seo.updateMetaTags({
      title: 'Outside OI Walls Scanner | Maximum-Pain.com',
      description: 'Find stocks trading outside their open interest walls — a key signal for options-driven price moves.',
      url: 'https://maximum-pain.com/outside-oi-walls'
    })
    // Skip API calls during SSR — prerender only needs SEO metadata
    if (!isPlatformBrowser(this.platformId)) { return; }

    let type = this.actRoute.snapshot.params.id;  
        
    let observable$: Observable<Array<OutsideOIWall>> = 
      this.data.getOutsideOIWalls();
    observable$.subscribe(response => {
      this.items = response;
    });
   }
}
