import { Component, OnInit, AfterViewInit, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Component( {
  selector: 'app-adsense',
  templateUrl: './adsense.component.html',
  styleUrls: ['./adsense.component.scss']
})
export class AdsenseComponent implements OnInit, AfterViewInit {

  isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit() {}

  ngAfterViewInit() {
    if (this.isBrowser) {
      this.withTimeout();
    }
  }

  withTimeout() {
    setTimeout(() => this.noTimeout(), 250);
  }

  noTimeout() {
    try {
      (window['adsbygoogle'] = window['adsbygoogle'] || []).push({});
    } catch(e) {
      console.error(e);
    }
  }
}
