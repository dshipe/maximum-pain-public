import {UtilsService} from '../services/utils.service';

export class MaxpainHistory {

    tk: string;
    m: Date;
    d: Date; 
    mp: number;
    coi: number; 
    poi: number; 
    sp: number; 
    cc: number; 
    pc: number;  

    constructor(private utils: UtilsService) { }

    public maturityStr(): string {
      const format = 'MM/dd/yyyy';
      return this.utils.FormatDate(this.m, format);
    }        

    public dateStr(): string {
      const format = 'MM/dd/yyyy';
      return this.utils.FormatDate(this.d, format);
    }
  } 
