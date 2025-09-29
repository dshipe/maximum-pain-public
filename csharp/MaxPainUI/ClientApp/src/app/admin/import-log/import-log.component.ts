import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { DataService } from '../../services/data.service';

@Component( {
  selector: 'app-import-log',
  templateUrl: './import-log.component.html',
  styleUrls: ['./import-log.component.scss']
})
export class ImportLogComponent implements OnInit {

  ImportDateCount: any;
  CacheDateCount: any;
  MarketCalendar: any;
  ImportLog: any;
  logId: number;

  //added the data parameter
  constructor(private data: DataService) { }

  ngOnInit() {

    let observable$: Observable<any> =
      this.data.importDateCount();
    observable$.subscribe(response => {
      this.ImportDateCount = response;
    });

    let observable3$: Observable<any> =
      this.data.cacheDateCount();
    observable3$.subscribe(response => {
      this.CacheDateCount = response;
    });

    let observable4$: Observable<any> =
      this.data.marketCalendar();
    observable4$.subscribe(response => {
      this.MarketCalendar = response;
    });

    let observable2$: Observable<any> =
      this.data.importLog(10);
    observable2$.subscribe(response => {
      this.ImportLog = response;
    });
  }

  handleClickHome(event, obj) {
    this.logId = obj.id;
  }
}
