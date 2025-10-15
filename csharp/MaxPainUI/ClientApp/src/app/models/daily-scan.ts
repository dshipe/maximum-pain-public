import { Data } from "@angular/router";

export class DailyScan{
  id: number;
  ticker: string;
  source: string;
  currentPrice: number;
  rsRating: number; 
  sma10Day: number; 
  sma20Day: number; 
  sma50Day: number; 
  sma150Day: number; 
  sma200Day: number; 
  week52Low: number; 
  week52High: number; 
  volume: number;
  volume20: number;
  volumePerc: number;
  adr: number;
  bbUpper: number;
  bbMiddle: number;
  bbLower: number;
  bbw: number;
  date: Date;
  createdOn: Date;
  base64: string;
  progressCurrentPrice: Date;
  progressBase64: string;
  progressModifiedOn: Date;
  watchFlag: boolean
}
