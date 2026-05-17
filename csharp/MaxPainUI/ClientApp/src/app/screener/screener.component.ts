import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { ActivatedRoute, Router} from '@angular/router';
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { SeoService } from '../services/seo.service';
import { MostActive } from "../models/most-active";
import { ScreenerChildComponent } from '../screener-child/screener-child.component';

@Component( {
  selector: 'app-screener',
  templateUrl: './screener.component.html',
  styleUrls: ['./screener.component.scss']
})
export class ScreenerComponent implements OnInit {

  public screenerType: string;
  public description: string;
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
    private title: Title,
    private seo: SeoService,
    @Inject(PLATFORM_ID) private platformId: Object) { }

  ngOnInit() {
    // Route params are available synchronously on server and browser
    const type = this.actRoute.snapshot.params.id || '';
    this.description = '';
    this.screenerType = type;
    if (type.toLowerCase() === 'changeprice') { this.screenerType = 'ChangePrice'; this.description = 'Change Price'; }
    if (type.toLowerCase() === 'openinterest') { this.screenerType = 'OpenInterest'; this.description = 'Open Interest'; }
    if (type.toLowerCase() === 'changeopeninterest') { this.screenerType = 'ChangeOpenInterest'; this.description = 'Change Open Interest'; }
    if (type.toLowerCase() === 'volume') { this.screenerType = 'Volume'; this.description = 'Volume'; }
    if (type.toLowerCase() === 'changevolume') { this.screenerType = 'ChangeVolume'; this.description = 'Change Volume'; }

    const descriptions: Record<string, string> = {
      'ChangePrice': 'Screen stocks by largest options price changes. Find stocks making significant moves with active options trading.',
      'OpenInterest': 'Discover stocks with highest options open interest. Identify where institutional money is positioned in the options market.',
      'ChangeOpenInterest': 'Track stocks with rapidly changing open interest. Spot new institutional positions and shifting market sentiment.',
      'Volume': 'Screen stocks by options trading volume. Find the most actively traded options in the market.',
      'ChangeVolume': 'Track stocks with surging options volume. Identify unusual options activity and potential breakouts.'
    };

    // SEO runs on server and browser
    this.seo.updateMetaTags({
      title: `${this.description} Option Screener | Maximum-Pain.com`,
      description: descriptions[this.screenerType] || 'Screen stocks by options activity and open interest.',
      keywords: `${this.description.toLowerCase()}, options screener, stock options screener, open interest screener, options trading`,
      url: `https://maximum-pain.com/screener/${type.toLowerCase()}`
    });

    if (!isPlatformBrowser(this.platformId)) { return; }

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
