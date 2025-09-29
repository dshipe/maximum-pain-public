import { UtilsService } from '../services/utils.service';

export class Spread {

  ticker: string;
  maturity: Date;
  optionType: string;
  modifiedOn: Date;

  longStrike: number;
  longPrice: number;
  longBid: number;
  longAsk: number;

  shortStrike: number;
  shortPrice: number;
  shortBid: number;
  shortAsk: number;

  description: string;
  cost: number;
  value: number;
  profit: number;
  roi: number;
  breakEven: number;

  constructor(private utils: UtilsService) { }

  public maturityStr(): string {
    const format = 'MM/dd/yyyy';
    return this.utils.FormatDate(this.maturity, format);
  }

  public dateStr(): string {
    const format = 'MM/dd/yyyy';
    return this.utils.FormatDate(this.modifiedOn, format);
  }
} 
