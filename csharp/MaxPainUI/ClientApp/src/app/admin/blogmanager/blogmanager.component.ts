import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { DataService } from '../../services/data.service';
import { UtilsService } from '../../services/utils.service';
import { BlogEntry } from "../../models/blog-entry";

@Component( {
  selector: 'app-blogmanager',
  templateUrl: './blogmanager.component.html',
  styleUrls: ['./blogmanager.component.scss']
})
export class BlogmanagerComponent implements OnInit {

  public blogForm: FormGroup;
  public entries: Array<BlogEntry>;
  public showModal = false;

  constructor(
    private data: DataService, 
    private actRoute: ActivatedRoute, 
    private route: Router, 
    private utils: UtilsService,
    private title: Title,
    private el: ElementRef) { 
          
    // override the route reuse strategy
    this.route.routeReuseStrategy.shouldReuseRoute = function() {
      return false;
    };
  }

  ngOnInit() {	
    this.createForm();
    //this.bindForm();
    /*
    this.tickerForm.get('formMaturity').valueChanges
      .subscribe(content=>{
        this.logTickerObj("formMaturity valueChanges content="+content);
        this.changeMaturity(content);
      })
    */
    
    let observable$: Observable<Array<BlogEntry>> = 
      this.data.getBlogEntries();
    observable$.subscribe(response => {
      this.entries = response;
    });
  }

  onKeydown(event: any) {
    if (event.key === "Enter") {
      //this.onSubmit(event);
    }
  } 

  onNewEntry(event: any) {
    console.log("onNewEntry");
    let entry: BlogEntry = new BlogEntry();
    entry.id=0;
    entry.isActive=true;
    entry.isStockPick=false;
    entry.showOnHome=true;
    this.bindForm(entry);
    this.showModal = true;
  }

  onRowClick(event: any, entry: BlogEntry) {
    console.log("onRowClick");
    this.bindForm(entry);
    this.showModal = true;
  }

  onModalSave(event: any) {
    let entry: BlogEntry = this.blogForm.value;
    console.log("onModalSave " + JSON.stringify(entry));

    let isNew: boolean = false;
    if (entry.id==0) isNew = true;

    let observable$: Observable<BlogEntry> = 
      this.data.upsertBlog(entry);
    observable$.subscribe(response => {
      if (isNew)
      {
        this.entries.push(response);
      }
      else
      {
        let dst = this.entries.find(x=>x.id==response.id);
        this.copyObject(response, dst);
      }
      this.showModal = false;
    });
  }

  onModalClose(event: any) {
    console.log("onModalClose");
    this.showModal = false;
  }

  createForm(): void {
    this.blogForm = new FormGroup({
      "id": new FormControl(),
      "title": new FormControl(),
      "imageUrl": new FormControl(),
      "ordinal": new FormControl(),
      "isActive": new FormControl(),
      "isStockPick": new FormControl(),
      "showOnHome": new FormControl(),
      "content": new FormControl(),
    });
  }

  bindForm(entry: BlogEntry): void {
    this.blogForm.controls["id"].setValue(entry.id);
    this.blogForm.controls["title"].setValue(entry.title);
    this.blogForm.controls["imageUrl"].setValue(entry.imageUrl);
    this.blogForm.controls["ordinal"].setValue(entry.ordinal);
    this.blogForm.controls["isActive"].setValue(entry.isActive);
    this.blogForm.controls["isStockPick"].setValue(entry.isStockPick);
    this.blogForm.controls["showOnHome"].setValue(entry.showOnHome);
    this.blogForm.controls["content"].setValue(entry.content);
  }

  copyObject(src: BlogEntry, dst: BlogEntry)
  {
    dst.title = src.title;
    dst.imageUrl = src.imageUrl;
    dst.ordinal = src.ordinal;
    dst.isActive = src.isActive;
    dst.isStockPick = src.isStockPick;
    dst.showOnHome = src.showOnHome;
    dst.createdOn = src.createdOn;
    dst.modifiedOn = src.modifiedOn;
    dst.content = src.content;    
  }
}
