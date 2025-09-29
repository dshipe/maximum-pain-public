import { Injectable } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router'


import { UtilsService } from "./utils.service";
import { Ticker } from "../models/ticker";
import { SdlChn } from "../models/straddle";


@Injectable()
export class StateService {
  private tickerKey: string = "tickerObj";
  private straddleKey: string = "straddleObj";

  constructor() { }

  initialize(actRoute: ActivatedRoute, utils: UtilsService): Ticker {
    // the default
    let tickerObj: Ticker = new Ticker(utils);
    tickerObj.Ticker = "AAPL";

    // get the Ticker Object from localstorage
    let stateObj: Ticker = JSON.parse(this.getString(this.tickerKey));
    if (stateObj && stateObj.Ticker) 
    {
      tickerObj.Ticker = stateObj.Ticker;
      if (stateObj.Maturity) tickerObj.Maturity = stateObj.Maturity;
    }

    // override Ticker with values from QueryString
    if(actRoute.snapshot.params.id) 
    {
      tickerObj.Ticker = actRoute.snapshot.params.id.toUpperCase();
    }
    // check for a Maturity in the QueryString
    actRoute.queryParams.subscribe(params => {
      tickerObj.Maturity = utils.ParseDateDelimiter(params["m"], "-");
    });
    
    //this.set(tickerObj);
    return tickerObj;
  }

  setTickerObj(tickerObj: Ticker): void {
    this.setString(this.tickerKey, JSON.stringify(tickerObj));
  }
  
  getStraddleObj(ticker: string): SdlChn {
  	// nothing cached
    let json: any  = this.getString(this.straddleKey);
    if (!json) return null;
	
    // different ticker
    let chain: SdlChn = new SdlChn(JSON.parse(json));
    if (chain.straddles[0].ticker!=ticker) return null;
	
    // old date
    let utc: Date = new Date();
    utc = new Date(utc.getTime()+(utc.getTimezoneOffset()*60000));
    let expired: Date = new Date(new Date().toISOString());
    expired.setMinutes(expired.getMinutes()-30);
    let chainDate: Date = new Date(chain.createdOn.toString());
    //chainDate = new Date(chainDate.toISOString());
    //console.log("state:  expired="+expired);
    //console.log("state:  chainDate="+chainDate);
    //console.log("state: bool="+(chain.createdOn < expired));
    if (chainDate < expired) return null;	

    return chain;
  }

  setStraddleObj(chain: SdlChn): void {
    this.setString(this.straddleKey, JSON.stringify(chain));
  }

  setString(key: string, content: string): void {
    localStorage.setItem(key, content);
  }

  getString(key: string): string {
    return localStorage.getItem(key);
  }  

  remove(key: string) {
    localStorage.removeItem(key);
  }

  clear() {
    localStorage.clear()
  }
}
