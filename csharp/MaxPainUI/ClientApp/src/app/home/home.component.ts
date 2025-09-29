import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { BlogEntry } from "../models/blog-entry";

@Component( {
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  public entries: Array<BlogEntry>;

  constructor(
    private data: DataService,
    private actRoute: ActivatedRoute,
    private route: Router,
    private utils: UtilsService,
    private title: Title) {

    // override the route reuse strategy
    this.route.routeReuseStrategy.shouldReuseRoute = function () {
      return false;
    };
  }
	
  ngOnInit() {
    //this.title.setTitle("Blog");
    
    let observable$: Observable<Array<BlogEntry>> =
      this.data.getBlogEntries();
    observable$.subscribe(response => {
      this.entries = response.filter(x=>x.showOnHome==true).sort((a,b)=>a.ordinal - b.ordinal);
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
    if (content==null) return content;
    return content.replace(/\s+/g, '-');
  }
}
