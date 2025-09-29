import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';
import { UtilsService } from '../services/utils.service';

export class Message {
  Id: Number;
  Subject: String;
  Body: String;
  CreatedOn: Date;

  constructor(private utils: UtilsService) { }

  get est(): Date {
    return this.utils.utcToEst(this.CreatedOn);
  }
}
