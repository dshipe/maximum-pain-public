import {Component,OnInit,AfterViewInit} from '@angular/core'

@Component( {
  selector: 'app-adsense',
  templateUrl: './adsense.component.html',
  styleUrls: ['./adsense.component.scss']
})
export class AdsenseComponent implements OnInit {

  // large rectangle 7363818743
  // banner 5981741182

  constructor() { }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.withTimeout();
  }

  withTimeout() {
    setTimeout(()=>this.noTimeout(),250);
  }

  noTimeout() {
    try{
      (window['adsbygoogle'] = window['adsbygoogle'] || []).push({});
    }catch(e){
      console.error(e);
    }
  }
}
