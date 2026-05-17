
import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges, Inject, PLATFORM_ID } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title, Meta } from "@angular/platform-browser";
import { SeoService } from '../services/seo.service';

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { BlogEntry } from "../models/blog-entry";

@Component( {
  selector: 'app-bloghome',
  templateUrl: './bloghome.component.html',
  styleUrls: ['./bloghome.component.scss']
})
export class BloghomeComponent implements OnInit {
  public entries: Array<BlogEntry>;

  constructor(
    private data: DataService,
    private actRoute: ActivatedRoute,
    private route: Router,
    private utils: UtilsService,
    private title: Title,
    private meta: Meta,
    private seo: SeoService,
    @Inject(PLATFORM_ID) private platformId: Object) {

    // override the route reuse strategy
    this.route.routeReuseStrategy.shouldReuseRoute = function () {
      return false;
    };
  }
	
  ngOnInit() {
    this.seo.updateMetaTags({
      title: 'Options Trading Blog | Maximum-Pain.com',
      description: 'Articles and analysis on options trading, max pain theory, open interest, and market direction. Insights for active options traders.',
      keywords: 'options trading blog, max pain analysis, open interest analysis, options strategy, options market insights',
      url: 'https://maximum-pain.com/blog'
    });

    if (!isPlatformBrowser(this.platformId)) { return; }
    
    let observable$: Observable<Array<BlogEntry>> =
      this.data.getBlogEntries();
    observable$.subscribe(response => {
      this.entries = response.filter(x=>x.isActive==true).sort((a,b)=>a.ordinal - b.ordinal);
    });    
  }

  stripHtml(content: string): string
  {
    return content.replace(/<[^>]*>/g, '');
  }

  getSummary(content: string): string
  {
    let size: number = 100;

    let summary: string = this.stripHtml(content);
    if (summary.length < size) return summary;
    return summary.substr(0, size) + "...";
  }

  addDashes(content: string): string
  {
    if (content==null) return null;
    return content.replace(/\s/g, '-');
  }
}
