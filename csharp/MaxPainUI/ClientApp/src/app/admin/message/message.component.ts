import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { UtilsService } from '../../services/utils.service';
import { DataService } from '../../services/data.service';
import { Message } from '../../models/message';
import { ServerDetails } from '../../models/server-details';

@Component( {
  selector: 'app-message',
  templateUrl: './message.component.html',
  styleUrls: ['./message.component.scss'],
  providers: [DataService],
})
export class MessageComponent implements OnInit {

  public messages: Array<Message>;
  public details: ServerDetails;
  public messageId: number;
  public isLoading: boolean = true;

  //added the data parameter
  constructor(
    private data: DataService,
    private utils: UtilsService
  ) { }

  ngOnInit() {
    this.isLoading = true;

    let observable$: Observable<Array<Message>> = 
      this.data.getMessages();
    observable$.subscribe(response => {
      this.messages = response;
      this.isLoading = false;
    });

    let observable2$: Observable<ServerDetails> =
      this.data.getServerDetails();
    observable2$.subscribe(response => {
      this.details = response;
    });
  }

  handleClick(event, msg) {
    this.messageId = msg.id;
  }

  onClick(event) {
    this.isLoading = true;
    let observable$: Observable<Array<Message>> = 
      this.data.truncateMessages();
    observable$.subscribe(response => {
      this.messages = response;
      this.isLoading = false;
    });
  }

  utcToEst(d: Date) : Date {
    return this.utils.utcToEst(d);
  }
}
