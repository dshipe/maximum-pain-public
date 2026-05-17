
import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges, Inject, PLATFORM_ID } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { SeoService } from '../services/seo.service';
import { BlogEntry } from "../models/blog-entry";

@Component( {
  selector: 'app-blog',
  templateUrl: './blog.component.html',
  styleUrls: ['./blog.component.scss']
})
export class BlogComponent implements OnInit, AfterViewInit {

  public entry: BlogEntry;
  public description: string;
  public content: string;

  constructor(
    private http: HttpClient,
    private data: DataService,
    private actRoute: ActivatedRoute,
    private route: Router,
    private utils: UtilsService,
    private title: Title,
    private seo: SeoService,
    @Inject(PLATFORM_ID) private platformId: Object) {

    // override the route reuse strategy
    this.route.routeReuseStrategy.shouldReuseRoute = function () {
      return false;
    };
  }

  ngOnInit() {
    if (!isPlatformBrowser(this.platformId)) { return; }

    this.useDatabase();
  }

  ngAfterViewInit() {
    if (this.description) {
      this.title.setTitle(this.description);
    }
  }

  useFileSystem() {
    let filetitle: string = this.actRoute.snapshot.params.id;
    let filepath: string = "assets/" + filetitle + ".html";

    this.description = filetitle.replace(/-/g, ' ');
    this.description = this.utils.ToPascalCase(this.description);

    this.http.get(filepath, { responseType: 'text' as 'json' }).subscribe(data => {
      this.content = data.toString();
      if (data == null || data.toString().length == 0) {
        let subject: string = "blog is missing page " + filetitle;
        let body: string = "blog is missing page " + filepath;

        this.data.postMessage(subject, body);
        this.redirect();
      }
    })
  }

  useDatabase() {
    let dash: string = this.actRoute.snapshot.params.id;
    let title = dash.replace(/-/g, ' ');

    let observable$: Observable<BlogEntry> =
      this.data.getBlogEntryByTitle(title);
    observable$.subscribe(response => {
      this.entry = response;
      if (!this.entry || !this.entry.id) {
        let subject: string = "blog is missing page " + title;

        this.data.postMessage(subject, subject);
        this.redirect();
      }
      this.title.setTitle(this.entry.title);
      
      let description = this.entry.content ? this.entry.content.replace(/<[^>]*>/g, '').substring(0, 155) : this.entry.title;
      this.seo.updateMetaTags({
        title: this.entry.title,
        description: description,
        url: `https://maximum-pain.com/blog/archive/${dash}`
      });
      
      this.seo.addStructuredData({
        "@context": "https://schema.org",
        "@type": "Article",
        "headline": this.entry.title,
        "description": description,
        "author": {
          "@type": "Organization",
          "name": "Maximum Pain"
        },
        "publisher": {
          "@type": "Organization",
          "name": "Maximum Pain",
          "logo": {
            "@type": "ImageObject",
            "url": "https://maximum-pain.com/assets/maxpain.png"
          }
        }
      });
    });
  }


  redirect() {
    this.route.navigate(['/', 'blog'], { relativeTo: this.actRoute }).then(e => {
      if (e) {
        console.log("Navigation is successful!");
      } else {
        console.log("Navigation has failed!");
      }
    });
  }
}


































