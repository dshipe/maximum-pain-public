import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';
import { UtilsService } from '../services/utils.service';

export class Hop {
  id: Number;
  destination: String;
  referrer: String;
  createdOn: Date;

  constructor(private utils: UtilsService) { }

  get est(): Date {
    return this.utils.utcToEst(this.createdOn);
  }
}

export class HopSummary {
    createdOn: Date;
    destination: String;
    hops: number;
  
    constructor(private utils: UtilsService) { }
}
  
export class HopAgent {
  userAgent: String;
  hops: number;

  constructor(private utils: UtilsService) { }
}
