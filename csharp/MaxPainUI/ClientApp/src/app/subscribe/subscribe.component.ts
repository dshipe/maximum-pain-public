import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';


@Component( {
  selector: 'app-subscribe',
  templateUrl: './subscribe.component.html',
  styleUrls: ['./subscribe.component.scss']
})
export class SubscribeComponent implements OnInit {

  public subscribeForm: FormGroup;
  public submitted: boolean = true;
  public notification: string;
  public checksum: string;

  constructor(private data: DataService,
    private readonly formBuilder: FormBuilder,
    private utils: UtilsService) {
  }

  ngOnInit() {
    this.createForm();

    this.submitted = false;
    this.notification = "";
    this.checksum = "";
  }

  onSubmit() {
    console.log("onSubmit()");
    this.submitted = true;

    let email1: string = this.subscribeForm.get('formEmail1').value;
    let email2: string = this.subscribeForm.get('formEmail2').value;
    
    let isValid: boolean = true;
    if (email1 && email1.length!=0) isValid=false;
    if (!email2) isValid==false;
    if (isValid) isValid = this.validateEmail(email2);
    
    console.log("isValid=" + isValid);

    if (isValid) {
      let observable$: Observable<string> =
        this.data.subscribe("", email2);

      observable$.subscribe(response => {
        console.log(response);
      });
    }
    this.notification = "Please look for a confirmation email.";
  }

  onClick(event) {
    this.onSubmit();
  }

  onKeydown(event) {
    if (event.key === "Enter") {
      this.onSubmit();
    } else {
      this.checksum = this.subscribeForm.get('formEmail2').value;
    }
  }

  createForm(): void {
    this.subscribeForm = new FormGroup({
      "formEmail1": new FormControl(),
      "formEmail2": new FormControl(),
      "formName": new FormControl(),
    });
  }

  validateEmail(email: string): boolean {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(String(email).toLowerCase());
  }    
}

