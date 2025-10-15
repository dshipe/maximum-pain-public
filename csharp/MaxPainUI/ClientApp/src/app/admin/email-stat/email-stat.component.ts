import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { DataService } from '../../services/data.service';
import { EmailStat } from '../../models/email-stat';


@Component( {
  selector: 'app-email-stat',
  templateUrl: './email-stat.component.html',
  styleUrls: ['./email-stat.component.scss']
})
export class EmailStatComponent implements OnInit {

  public stats: Array<EmailStat>;
  
  //added the data parameter
  constructor(private data: DataService) { }

  ngOnInit() {
    let observable$: Observable<Array<EmailStat>> = 
      this.data.getEmailStats();
    observable$.subscribe(response => {
      this.stats = response;
    });
  }
}
