import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { BlogEntry } from "../models/blog-entry";

@Component( {
  selector: 'app-blog',
  templateUrl: './blog.component.html',
  styleUrls: ['./blog.component.scss']
})
export class BlogComponent implements OnInit {

  public entry: BlogEntry;
  public description: string;
  public content: string;

  constructor(
    private http: HttpClient,
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
    this.useDatabase();
  }

  useFileSystem() {
    let filetitle: string = this.actRoute.snapshot.params.id;
    let filepath: string = "assets/" + filetitle + ".html";

    this.description = filetitle.replace(/-/g, ' ');
    this.description = this.utils.ToPascalCase(this.description);

    this.title.setTitle(this.description);

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


































