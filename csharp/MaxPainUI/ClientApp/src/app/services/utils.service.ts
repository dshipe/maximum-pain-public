import { Injectable, Pipe, PipeTransform } from '@angular/core';
import { DecimalPipe } from '@angular/common';

import { SdlChn, Sdl } from "../models/straddle";

@Injectable()
export class UtilsService {

  constructor() { }

  public IsDebugMode() {
    return false;
  }

  public getUTC(): Date {
    const now = new Date();
    return new Date(now.getTime() + (now.getTimezoneOffset() * 60000));
  }

  public ParseDate(value: any): Date | null {
    return this.ParseDateDelimiter(value, "/");
  }

  public ParseDateDelimiter(value: any, delimiter: string): Date | null {
    return this.ParseDateDelimiter2(value, delimiter, false);
  }

  public ParseDateDelimiter2(value: any, delimiter: string, convertUTC: boolean): Date | null {
    if ((typeof value === 'string') && (value.indexOf(delimiter) > -1)) {
      const str = value.split(delimiter);

      const month = Number(str[0]) - 1;
      const date = Number(str[1]);
      const year = Number(str[2]);

      let d: Date = new Date(year, month, date);

      if (convertUTC) {
        let dteStr: string = "yyyy-MM-ddT00:00:00.000Z";
        dteStr.replace("yyyy", year.toString()).replace("MM", month.toString()).replace("dd", date.toString())
        let d: Date = new Date(dteStr);
      }

      return d;

    } else if ((typeof value === 'string') && value === '') {
      return new Date();
    }

    const timestamp = typeof value === 'number' ? value : Date.parse(value);
    return isNaN(timestamp) ? null : new Date(timestamp);
  }

  public utcToEst(utc: Date): Date {
    let hours: number = -5;
    if (this.isDST(utc)) hours = -4;

    let est: Date = new Date(utc.getTime());
    est.setHours(est.getHours() + hours);
    return est;
  }

  public isDST(d: Date): boolean {
    let jan = new Date(d.getFullYear(), 0, 1).getTimezoneOffset();
    let jul = new Date(d.getFullYear(), 6, 1).getTimezoneOffset();
    return Math.max(jan, jul) != d.getTimezoneOffset();
  }

  public FormatDateStd(value: Date): string {
    return this.FormatDate(value, "MM/dd/yyyy");
  }

  public FormatDate(value: Date, format: string): string {
    let yyyy: string = value.getFullYear().toString();
    let MM: string = (value.getMonth() + 1).toString();
    let dd: string = value.getDate().toString();
    let hh: string = value.getHours().toString();
    let nn: string = value.getMinutes().toString();
    let ss: string = value.getSeconds().toString();

    MM = "00" + MM;
    MM = MM.substr(MM.length - 2, 2);

    dd = "00" + dd;
    dd = dd.substr(dd.length - 2, 2);

    hh = "00" + hh;
    hh = hh.substr(hh.length - 2, 2);

    nn = "00" + nn;
    nn = nn.substr(nn.length - 2, 2);

    ss = "00" + ss;
    ss = ss.substr(ss.length - 2, 2);

    return format.replace("yyyy", yyyy).replace("MM", MM).replace("dd", dd).replace("hh", hh).replace("nn", nn).replace("ss", ss);
  }

  public ToCamelCase(content: string): string {
    return content
      .replace(/\s(.)/g, function ($1) { return $1.toUpperCase(); })
  }

  public ToPascalCase(content: string): string {
    let transformed = this.ToCamelCase(content);
    return transformed.charAt(0).toUpperCase() + transformed.slice(1);
  }

  public ReformatYMD(dte: string): string {
    if (dte.length != 8 ) return "";
    let yyyy: string = dte.substr(0, 4);
    let MM: string = dte.substr(4, 2);
    let dd: string = dte.substr(6, 2);
    return "MM/dd/yyyy".replace("yyyy", yyyy).replace("MM", MM).replace("dd", dd);
  }

  public distinctMaturityStraddle(straddles: Array<Sdl>): Array<string> {
    console.log("distinctMaturityStraddle");

    let maturities: Array<string> = [];
    let map: any = new Map();
    for (let s of straddles) {
      let straddle: Sdl = <Sdl>s;
      if (!map.has(straddle.mymd)) {
        map.set(straddle.mymd, true);
        maturities.push(straddle.mymd);
      }
    }
    maturities.sort();
    //console.log(maturities);

    let formatted: Array<string> = [];
    for (let m of maturities) {
      formatted.push(this.ReformatYMD(m));
    }
    //console.log(formatted);
    return formatted;
  }

  public validateMaturity(maturities: string[], maturityStr: string): boolean {
    for (let i: number = 1; i <= maturities.length; i++) {
      if (maturityStr == maturities[i]) return true;
    }
    return false;
  }

  public filterStraddle(straddles: Array<Sdl>, maturityStr: string): Array<Sdl> {
    let result: Array<Sdl> = [];
    for (let s of straddles) {
      let straddle: Sdl = <Sdl>s;
      if (straddle.mstr == maturityStr) {
        result.push(straddle);
      }
    }
    return result;
  }
}

/*
@Pipe({ name: 'toNumber' })
export class ToNumberPipe implements PipeTransform {
  transform(value: string): any {
    let num: number = Number(value);
    return isNaN(num) ? 0 : num;
  }
}
*/
