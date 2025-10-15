import { Subject, Observable, forkJoin, Subscription, throwError as observableThrowError } from 'rxjs'
import { catchError,  map, retry } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import * as pako from 'pako';
import { Buffer } from "buffer";

import { UtilsService } from './utils.service';
import { StateService } from './state.service';
import { SdlChn, Sdl } from '../models/straddle';
import { MPChn, MPItem } from '../models/MaxPainItem';
import { MostActive } from '../models/most-active';
import { OutsideOIWall } from '../models/OutsideOIWall';
import { Spread } from '../models/Spread';
import { ChartInfo } from "../models/chart-info";
import { MaxpainHistory } from '../models/maxpain-history';
import { EmailMessage } from '../models/email-message';
import { EmailStat } from '../models/email-stat';
import { Message } from '../models/message';
import { Hop, HopSummary, HopAgent } from '../models/hop';
import { BlogEntry } from "../models/blog-entry";
import { ImportMaxPain } from "../models/import-max-pain";
import { CupWithHandleHistory } from "../models/cup-with-handle-history";
import { DailyScan } from "../models/daily-scan";
import { MarketDirection } from "../models/market-direction";
import { ServerDetails } from "../models/server-details";

declare var window: any; // Needed on Angular 8+

@Injectable()
export class DataService {
  

  domainUrl : string = "https://maximum-pain.com";
  //domainUrl: string = "https://localhost:44301";
  lambaUrl: string = "https://hcapr4ndhwksq5dq7ird3yujpq0edbbt.lambda-url.us-east-1.on.aws";
  //lambaUrl: string = "";
  debug: boolean = false;
  allowCompression: boolean = true;
  headers: HttpHeaders;
  
  // added the http parameter
  constructor(private http: HttpClient, private utils: UtilsService, private state: StateService) 
  {	  
    //const parsedUrl = new URL(window.location.href);
    //this.lambaUrl = parsedUrl.origin;
    
    this.debug = this.utils.IsDebugMode();
    if(this.debug)
    {
      this.lambaUrl = this.lambaUrl.replace("https", "http") + ":81";
    }

    this.headers = new HttpHeaders({
      // 'Content-Encoding': 'gzip',
      // 'Content-Type': 'application/json'
      'Content-Type': 'application/x-www-form-urlencoded'
    });
  }

  // ********* ********* ********* ********* *********
  // messages

  getMessages(): Observable<Array<Message>> {
    let u = this.lambaUrl + "/api/message";
    return this.http.get<Array<Message>>(u)
      .pipe(map((response: Array<Message>) => response));
  }

  postMessage(subject: string, body: string): void {
    let u = this.lambaUrl + "/api/message/create";
  	var content: string = "subject=" + encodeURIComponent(subject)
      + "&body=" + encodeURIComponent(body)
    let headers = new HttpHeaders({'Content-Type': 'application/x-www-form-urlencoded'});
    this.http.post(u, content, {headers : headers}).pipe(map((response: any) => response));
  }  

  truncateMessages(): Observable<Array<Message>> {
    let u = this.lambaUrl + "/api/message/truncate";
    return this.http.get<Array<Message>>(u)
      .pipe(map((response: Array<Message>) => response));
  }
  
  // ********* ********* ********* ********* *********
  // option 

  getStraddle(ticker: string) : Observable<SdlChn> {
    let u = this.lambaUrl + "/api/options/straddle/" + ticker;
    console.log("getStraddle " + u);
    return this.http.get<SdlChn>(u)
      .pipe(map((response:SdlChn) => new SdlChn(response) ));
  }

  getStraddlePost(json: string, maturity: Date) : Observable<SdlChn> {
    let u: string = this.lambaUrl + "/api/options/straddlepost";
    let content: string = 
      "m=" +  encodeURIComponent(this.utils.FormatDateStd(maturity))
      + "&json=" + this.compress(json);
    return this.http.post<SdlChn>(u, content, {headers : this.headers})
      .pipe(map((response:SdlChn) => new SdlChn(response) ));
  }  

  getMaxpain(ticker: string) : Observable<MPChn> {
    let u = this.lambaUrl + "/api/options/maxpain/" + ticker + this.pw("?");
    return this.http.get<MPChn>(u)
      .pipe(map((response:MPChn) => new MPChn(response) ));
  }

  getMaxpainPost(json: string, maturityStr: string) : Observable<MPChn> {
    let u: string = this.lambaUrl + "/api/options/maxpainpost";
    const content: string = "json=" + this.compress(json);
    return this.http.post<MPChn>(u, content, {headers : this.headers})
      .pipe(map((response:MPChn) => new MPChn(response) ));
    }  

  getSpreads(ticker: string) : Observable<Array<Spread>> {
    let u = this.lambaUrl + "/api/options/spreads/" + ticker;
    return this.http.get<Array<Spread>>(u)
      .pipe(map((response: Array<Spread>) => response));
  }  

  getMostActive() : Observable<Array<MostActive>> {
    let u = this.lambaUrl + "/api/remotedata/mostactive";
    return this.http.get<Array<MostActive>>(u)
      .pipe(map((response: Array<MostActive>) => response));
  }  

  getOutsideOIWalls() {
    let u = this.lambaUrl + "/api/remotedata/outsideoiwalls"
    return this.http.get<Array<OutsideOIWall>>(u)
      .pipe(map((response: Array<OutsideOIWall>) => response));
  } 

  getOptionHistory(ticker: string) : Observable<SdlChn> {
    let u = this.lambaUrl + "/api/history/straddle/" + ticker;
    return this.http.get<SdlChn>(u)
    .pipe(map((response:SdlChn) => new SdlChn(response) ));
  }  

  getMaxpainHistory(ticker: string) : Observable<Array<MaxpainHistory>> {
    let u = this.lambaUrl + "/api/history/MaxPain/" + ticker;
    return this.http.get<Array<MaxpainHistory>>(u)
      .pipe(map((response: Array<MaxpainHistory>) => response));
  }

  getMaxpainHistory2(json: string) : Observable<Array<MaxpainHistory>> {
    let u = this.lambaUrl + "/api/history/MaxPainPost";
    let content: string =  "json=" + this.compress(json);
    return this.http.post<Array<MaxpainHistory>>(u, content, {headers : this.headers})
      .pipe(map((response: Array<MaxpainHistory>) => response));
  }

  tdacsv(ticker: string) : Observable<any> {
    //let u = this.lambaUrl + "/api/options/tdacsvobj/" + ticker;
    let u = this.lambaUrl + "/api/options/csv/" + ticker;
    return this.http.get<any>(u)
      .pipe(map((response: any) => response));
  }

  // ********* ********* ********* ********* *********
  // charts
  getChartPost(apiName: string, json: string, title: string, key: string): Observable<ChartInfo> {
    let u: string = this.lambaUrl + "/api/chartinfo/"+apiName;
    let content: string =
      "title=" +  encodeURIComponent(title)
      + "&key=" +  encodeURIComponent(key)
      + "&json=" + this.compress(json);
	  + this.pw("&")
    return this.http.post<ChartInfo>(u, content, {headers : this.headers})
      .pipe(map((response: ChartInfo) => response));
  }  

  getChartIVPredictPost(apiName: string, json: string, title: string, key: string, degree: number): Observable<ChartInfo> {
    if (!degree) degree=1;

    let u: string = this.lambaUrl + "/api/chartinfo/"+apiName;
    let content: string = 
      "title=" +  encodeURIComponent(title)
      + "&key=" +  encodeURIComponent(key)
      + "&degree=" +  encodeURIComponent(degree)
      + "&json=" + this.compress(json) 
	  + this.pw("&");	  
    return this.http.post<ChartInfo>(u, content, {headers : this.headers})
      .pipe(map((response: ChartInfo) => response));
  }  

  // ********* ********* ********* ********* *********
  // email

  sendMail(msg: EmailMessage): Observable<EmailMessage> {
    let u = this.lambaUrl + "/api/Email/Send";
		//let headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    let content : string = "json=" + encodeURIComponent(JSON.stringify(msg));
    let headers = new HttpHeaders({'Content-Type': 'application/x-www-form-urlencoded'});

    //let content: string = JSON.stringify(msg);
    return this.http.post(u, content, {headers : headers})
      .pipe(map((response: EmailMessage) => response));
  }    

  subscribe(name: string, email: string) {
    console.log("subscribe")
    let u = this.lambaUrl + "/api/emaillist/subscribe";
    var content: string = "name=" + encodeURIComponent(name)
        + "&email=" + encodeURIComponent(email)
    let headers = new HttpHeaders({'Content-Type': 'application/x-www-form-urlencoded'});
    return this.http.post(u, content, {headers : headers}).pipe(map((response: any) => response));
  } 

  getEmailStats(): Observable<Array<EmailStat>>  {
    let u = this.lambaUrl + "/api/emaillist/stats";
    return this.http.get<Array<EmailStat>>(u)
      .pipe(map((response: Array<EmailStat>) => response));
  }

  // ********* ********* ********* ********* *********
  // screener
  screener(useShortUrls: boolean, msg: string) {
    let u = this.lambaUrl + "/api/EmailList/screener/"+this.timestamp()+"?debug="+this.debug+"&useShortUrls="+useShortUrls;
    return this.http
     .get(u).pipe(
     catchError(error => { 
       return this.handleError(error);
     }));
   } 

   screenerMaxPain(): Observable<Array<ImportMaxPain>>  {
    let u = this.lambaUrl + "/api/FinImport/ImportMaxPain";
    return this.http.get<Array<ImportMaxPain>>(u)
      .pipe(map((response:Array<ImportMaxPain>) => {
        let result: Array<ImportMaxPain> = new Array<ImportMaxPain>();
        for (let x of response) { result.push(new ImportMaxPain(x)) };
        return result;
      } ));
  } 

  // ********* ********* ********* ********* *********
  // twitter

  twitter() {
    let u = this.lambaUrl + "/api/twitter/execute/"+this.timestamp();
    return this.http
      .get(u).pipe(
      catchError(error => { 
        return this.handleError(error);
      }));
  }

  getUserTweets(userNames:string, count:string) {
    let u = this.lambaUrl + "/api/twitter/UserTweets/"+this.timestamp()+"?userNames="+userNames +"&count="+count;
    return this.http
      .get(u).pipe(
      catchError(error => { 
        return this.handleError(error);
      }));
  }

  // ********* ********* ********* ********* *********
  // scheduled task

  scheduledTask() : Observable<string> {
    let u = this.lambaUrl + "/api/settings/ScheduledTask/"+this.timestamp()+"?debug="+this.debug;
    return this.http.get<string>(u)
      .pipe(map((response: any) => response.result));
  }    
  
  healthCheck() : Observable<string> {
    let u = this.lambaUrl + "/api/healthcheck/"+this.timestamp()+"?debug="+this.debug;
    return this.http.get<string>(u)
      .pipe(map((response: string) => response));
  }  
 
  settingsRead() : Observable<string> {
    let u = this.lambaUrl + "/api/settings/ReadJson";
    return this.http.get<string>(u)
      .pipe(map((response: string) => response));
  }  
  
  settingsSave(json: string) {
    let u = this.lambaUrl + "/api/settings/SaveJson";
    var content: string = "json=" + encodeURIComponent(json) + this.pw("&");	  
    let headers = new HttpHeaders({'Content-Type': 'application/x-www-form-urlencoded'});
    return this.http.post(u, content, {headers : headers}).pipe(map((response: any) => response));
  }  

  getServerDetails(): Observable<ServerDetails> {
    let u = this.lambaUrl + "/api/settings/ServerDetail";
    return this.http.get<ServerDetails>(u)
      .pipe(map((response: ServerDetails) => response));
  }

  // ********* ********* ********* ********* *********
  // import

  distributeEmail(password: string, debug: boolean, useShortUrls: boolean, runNow: boolean) : Observable<any> {
    let encoded: string = encodeURIComponent(password);
    let u = this.lambaUrl + "/api/EmailList/Screener/012345?saveMessage=true&debug="+debug+"&useShortUrls="+useShortUrls+"&runNow="+runNow+"&pw="+encoded;
    return this.http.get<any>(u)
      .pipe(map((response: any) => response));
  }

  import(password: string, debug: boolean, sendEmail: boolean) : Observable<any> {
    let encoded: string = encodeURIComponent(password);
    let u = this.domainUrl + "/api/FinImport/RunImport?saveMessage=true&debug="+debug+"&sendEmail="+sendEmail+"&pw="+encoded;
    return this.http.get<any>(u)
      .pipe(map((response: any) => response));
  }

  importDateCount() : Observable<any> {
    let u = this.lambaUrl + "/api/finimport/importdatecount";
    return this.http.get<any>(u)
      .pipe(map((response: any) => response));
  }

  cacheDateCount() : Observable<any> {
    let u = this.lambaUrl + "/api/finimport/cachedatecount";
    return this.http.get<any>(u)
      .pipe(map((response: any) => response));
  }

  marketCalendar() : Observable<any> {
    let u = this.lambaUrl + "/api/finimport/ShowMarketCalendar/true";
    return this.http.get<any>(u)
      .pipe(map((response: any) => response));
  }

  importLog(count: number) : Observable<any> {
    let u = this.lambaUrl + "/api/finimport/importlog?count=" + count;
    return this.http.get<any>(u)
      .pipe(map((response: any) => response));
  }

  // ********* ********* ********* ********* *********
  // blog
  getBlogEntries(): Observable<Array<BlogEntry>>  {
    let u = this.lambaUrl + "/api/blog/entries";
    return this.http.get<Array<BlogEntry>>(u)
      .pipe(map((response: Array<BlogEntry>) => response));
  }

  getBlogEntry(id: number): Observable<BlogEntry>  {
    let u = this.lambaUrl + "/api/blog/entry?id=" + encodeURIComponent(id);
    return this.http.get<BlogEntry>(u)
      .pipe(map((response: BlogEntry) => response));
  }

  getBlogEntryByTitle(title: string): Observable<BlogEntry>  {
    let u = this.lambaUrl + "/api/blog/entrybytitle?title=" + encodeURIComponent(title);
    return this.http.get<BlogEntry>(u)
      .pipe(map((response: BlogEntry) => response));
  } 

  upsertBlog(entry: BlogEntry): Observable<BlogEntry>  {
    let u: string = this.lambaUrl + "/api/blog/upsert";
    let content: string = 
      "json=" + encodeURIComponent(JSON.stringify(entry))
	  + this.pw("&");	  
    let headers: HttpHeaders = new HttpHeaders({'Content-Type': 'application/x-www-form-urlencoded'});
    return this.http.post<BlogEntry>(u, content, {headers : headers})
      .pipe(map((response: BlogEntry) => response));
  }

  // ********* ********* ********* ********* *********
  // hops
  getHopDetail(): Observable<Array<Hop>>  {
    let u = this.lambaUrl + "/api/hop/detail";
    return this.http.get<Array<Hop>>(u)
      .pipe(map((response: Array<Hop>) => response));
  }

  getHopSummary(): Observable<Array<HopSummary>>  {
    let u = this.lambaUrl + "/api/hop/summary";
    return this.http.get<Array<HopSummary>>(u)
      .pipe(map((response: Array<HopSummary>) => response));
  }

  getHopAgent(): Observable<Array<HopAgent>>  {
    let u = this.lambaUrl + "/api/hop/agent";
    return this.http.get<Array<HopAgent>>(u)
      .pipe(map((response: Array<HopAgent>) => response));
  }

  // ********* ********* ********* ********* *********
  // python
  getCupWithHandleHistory(midnight: string): Observable<Array<CupWithHandleHistory>>  {
    let u = this.lambaUrl + "/api/python/CupWithHandleHistory?midnight=";
    if(midnight && midnight.length>0)
    {
      u = this.lambaUrl + "/api/python/CupWithHandleHistory?midnight=" + encodeURIComponent(midnight);
    }
    return this.http.get<Array<CupWithHandleHistory>>(u)
      .pipe(map((response: Array<CupWithHandleHistory>) => response));
  }

  getDailyScanDates(): Observable<Array<DailyScan>> {
    let u = this.lambaUrl + "/api/python/GetDailyScanDates";
    return this.http.get<Array<DailyScan>>(u)
      .pipe(map((response: Array<DailyScan>) => response));
  }

  getDailyScan(midnight: string): Observable<Array<DailyScan>> {
    let u = this.lambaUrl + "/api/python/GetDailyScan?midnight=";
    if (midnight && midnight.length > 0) {
      u = this.lambaUrl + "/api/python/GetDailyScan?midnight=" + encodeURIComponent(midnight);
    }
    return this.http.get<Array<DailyScan>>(u)
      .pipe(map((response: Array<DailyScan>) => response));
  }

  addDailyScan(ticker: string): Observable<Array<DailyScan>> {
    let u = this.lambaUrl + "/api/python/AddDailyScan?ticker=" + encodeURIComponent(ticker);
    return this.http.get<Array<DailyScan>>(u)
      .pipe(map((response: Array<DailyScan>) => response));
  }

  dailyScanUpdateWatch(id: number, flag: boolean): Observable<Array<DailyScan>> {
    let u = this.lambaUrl + "/api/python/DailyScanUpdateWatch?id=" + id + "&flag=" + flag;
    return this.http.get<Array<DailyScan>>(u)
      .pipe(map((response: Array<DailyScan>) => response));
  }

  getMarketDirection(): Observable<Array<MarketDirection>> {
    let u = this.lambaUrl + "/api/python/GetMarketDirection";
    return this.http.get<Array<MarketDirection>>(u)
      .pipe(map((response: Array<MarketDirection>) => response));
  }


  // ********* ********* ********* ********* *********
  // private methods
 
  private handleError(error: Response) {
      //console.log(error.status);
      //console.log(error.toString());
      return observableThrowError(error);
  }

  private timestamp() {
    let current: Date = new Date();
    return this.utils.FormatDate(current, "yyyyMMddhhnnss");
  }

  private pw(delimiter: string): string {
    return delimiter + this.encodePW();
  }

	public encodePW()
	{
		let result: string = "";
		let alphabet: string = "abcdefghijklmnopqrstuvwxyz";
		let passPhrase: string = "volley";
		
		let utc: Date = this.utils.getUTC();
		let timeStr: string = this.utils.FormatDate(utc,"ssmmhh");

		for(let i=0; i<timeStr.length; i++)
		{
			let digit: number = parseInt(timeStr.substr(i,1));
			let passDigit: number =  parseInt(timeStr.substr(i,1));
			let combined: number = digit + passDigit;
			if (combined>25) combined = combined - 26;
			
			let chr: string = alphabet.substr(combined,1) 
			result = result.concat(chr);
		}			
		return result;
  }

  private compress(content: string) {
    if (!this.allowCompression) return encodeURIComponent(content);

    const json = JSON.stringify(content);
    var compressed_uint8array = pako.gzip(json);
    var base64 = btoa(String.fromCharCode.apply(null, compressed_uint8array));
    //console.log(base64);
    return base64;
  }
}

