import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';


@Component( {
  selector: 'app-scheduled-task',
  templateUrl: './scheduled-task.component.html',
  styleUrls: ['./scheduled-task.component.scss']
})
export class ScheduledTaskComponent implements OnInit {

  scheduledTaskResponse: string;
  @Input() isHidden: boolean = true;

  constructor(private data: DataService, private utils: UtilsService) { }

  ngOnInit() {
    let observable$: Observable<string> = 
      this.data.scheduledTask();
    observable$.subscribe(response => {
      this.scheduledTaskResponse = response;
    });
  }

  onClick(event) {
    this.isHidden = !this.isHidden;
  }  
}
