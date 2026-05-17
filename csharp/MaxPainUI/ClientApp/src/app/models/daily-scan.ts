import { Data } from "@angular/router";

export class DailyScan{
  id: number;
  adr: number;
  base64: string;
  bbUpper: number;
  bbMiddle: number;
  bbLower: number;
  bbw: number;
  createdOn: Date;
  date: Date;
  flagAtrDrop: boolean;
  flagFlatChannel: boolean;
  flagHigherLows: boolean;
  flagMovingAverages: boolean;
  flagPricePattern: boolean;
  flagVolumeRequirements: boolean;
  flagMarketCap: boolean;
  flagAvoidGapDown: boolean;
  flagRsiMomentum: boolean;
  hasAlerted: boolean;
  model: string;
  price: number;
  currentPrice: number;
  progressBase64: string;
  progressCurrentPrice: number;
  progressModifiedOn: Date;
  progressPercent: number;
  rsi: number;
  sector: string;
  source: string;
  ticker: string;
  volume: number;
  volume20: number;
  watchFlag: boolean;

  constructor(jsonObj: any) {
    Object.keys(jsonObj).map((key) => { this[key] = jsonObj[key] });
    this.progressPercent = this.progressCurrentPrice && this.price ? 
      (this.progressCurrentPrice - this.price) / this.price * 100 : 0;
  }
}
