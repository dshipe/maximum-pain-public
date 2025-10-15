import { Pipe, PipeTransform, Directive } from '@angular/core';
import { DatePipe } from '@angular/common';
import {UtilsService} from '../services/utils.service';


@Directive()
export class Ticker {
  Ticker: string = "AAPL";
  Maturity: Date;
  Maturities: Array<string>;
  Strike: string;
  Strikes: Array<string>;
  JsonData: string;

  constructor(private utils: UtilsService) {}
  
  ngOnInit() {
    //console.log(new Date().toISOString())
  }    

  get MaturityString(): string {
    if (!this.Maturity) return null;
    return this.utils.FormatDateStd(this.Maturity);
  }
  
  hydrate(json: string): void
  {
    let jsonObj: any = JSON.parse(json);
    for(let prop in jsonObj)
    {
      this[prop]=jsonObj[prop];
    } 
  }

}
