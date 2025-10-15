import { AfterViewInit, OnInit, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'
import { Title } from "@angular/platform-browser";

import { UtilsService } from '../../services/utils.service';
import { DataService } from '../../services/data.service';
import { Message } from '../../models/message';

@Component( {
  selector: 'app-import',
  templateUrl: './import.component.html',
  styleUrls: ['./import.component.scss']
})
export class ImportComponent implements OnInit {

  public MyForm: FormGroup;

  constructor(
    private data: DataService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.createForm();
    this.bindForm();
  }

  createForm(): void {
    this.MyForm = new FormGroup({
      "formPassword": new FormControl(null, [Validators.required]),
      "formImportDebug": new FormControl(),
      "formImportSendEmail": new FormControl(),
      "formEmailDebug": new FormControl(),
      "formEmailUseShortUrls": new FormControl(),
      "formEmailRunNow": new FormControl(),
    });
  }

  bindForm(): void {
    this.MyForm.controls["formImportDebug"].setValue(true);
    this.MyForm.controls["formEmailUseShortUrls"].setValue(true);
    this.MyForm.controls["formEmailRunNow"].setValue(true);
  }

  onClickImport(event) {
      this.callImport();
  }  
  callImport() {
    let password: string = this.MyForm.controls["formPassword"].value;

    let debug: boolean = this.MyForm.controls["formImportDebug"].value;
    let sendEmail: boolean = this.MyForm.controls["formImportSendEmail"].value;

    let observable$: Observable<Array<Message>> = 
      this.data.import(password, debug, sendEmail);
    observable$.subscribe(response => {
    });
    this.router.navigate(["/admin/message"]);
  }

  onClickEmail(event) {
    this.callEmail();
  }  
  callEmail() {
    let password: string = this.MyForm.controls["formPassword"].value;

    let debug: boolean = this.MyForm.controls["formEmailDebug"].value;
    let useShortUrls: boolean = this.MyForm.controls["formEmailUseShortUrls"].value;
    let runNow: boolean = this.MyForm.controls["formEmailRunNow"].value;

    let observable$: Observable<Array<Message>> = 
      this.data.distributeEmail(password, debug, useShortUrls, runNow);
    observable$.subscribe(response => {
    });
    this.router.navigate(["/admin/message"]);
  }

}
