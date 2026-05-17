import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, DOCUMENT } from '@angular/common';
import { Title, Meta } from '@angular/platform-browser';
import { getCompanyName } from '../models/ticker-companies';

export interface SeoConfig {
  title?: string;
  description?: string;
  keywords?: string;
  url?: string;
  image?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SeoService {
  private readonly baseUrl = 'https://maximum-pain.com';
  private readonly defaultImage = 'https://maximum-pain.com/assets/maxpain.png';

  constructor(
    private titleService: Title,
    private metaService: Meta,
    @Inject(PLATFORM_ID) private platformId: Object,
    @Inject(DOCUMENT) private document: Document
  ) {}

  updateTitle(title: string): void {
    this.titleService.setTitle(title);
  }

  updateMetaTags(config: SeoConfig): void {
    const title = config.title ?? 'Stock Option Max Pain Calculator | Maximum-Pain.com';
    const description = config.description ?? 'Free stock options max pain calculator with real-time open interest data.';
    const image = config.image ?? this.defaultImage;
    const url = config.url ?? this.baseUrl;

    this.titleService.setTitle(title);
    this.metaService.updateTag({ name: 'description', content: description });

    if (config.keywords) {
      this.metaService.updateTag({ name: 'keywords', content: config.keywords });
    }

    // Open Graph
    this.metaService.updateTag({ property: 'og:title', content: title });
    this.metaService.updateTag({ property: 'og:description', content: description });
    this.metaService.updateTag({ property: 'og:image', content: image });
    this.metaService.updateTag({ property: 'og:url', content: url });

    // Twitter Card
    this.metaService.updateTag({ name: 'twitter:title', content: title });
    this.metaService.updateTag({ name: 'twitter:description', content: description });
    this.metaService.updateTag({ name: 'twitter:image', content: image });

    this.updateCanonical(url);
  }

  /**
   * Convenience method for /options/:ticker pages.
   * Includes company name (e.g. "Apple Inc.") in title, description and keywords
   * — replicating and improving on what the Lambda@Edge function was doing.
   */
  updateTickerSeo(ticker: string, routeType: string = 'options'): void {
    const t = ticker.toUpperCase();
    const company = getCompanyName(t);                      // e.g. "Apple Inc."
    const nameWithCompany = company ? `${t} (${company})` : t;  // "AAPL (Apple Inc.)"
    const companyKeywords = company ? `, ${t}, ${company}` : `, ${t}`;

    const routeLabel: Record<string, string> = {
      'options':        'Max Pain Calculator',
      'stacked':        'Stacked Open Interest',
      'greeks':         'Options Greeks',
      'iv':             'Implied Volatility',
      'history':        'Max Pain History',
      'maxpain-history':'Max Pain History',
      'spreads':        'Options Spreads',
    };
    const label = routeLabel[routeType] ?? 'Max Pain Calculator';

    this.updateMetaTags({
      title: `${nameWithCompany} ${label} | Maximum-Pain.com`,
      description:
        `${nameWithCompany} ${label.toLowerCase()} for the current options expiration. ` +
        `Live open interest data, charts, and strike analysis — free.`,
      keywords: `max pain calculator, stock options, open interest${companyKeywords}`,
      url: `${this.baseUrl}/${routeType}/${t}`,
    });

    this.addStructuredData([
      {
        '@context': 'https://schema.org',
        '@type': 'WebApplication',
        name: `${t} Max Pain Calculator`,
        url: `${this.baseUrl}/options/${t}`,
        applicationCategory: 'FinanceApplication',
        description: `Calculate the max pain price for ${t} options`,
        offers: { '@type': 'Offer', price: '0', priceCurrency: 'USD' }
      },
      {
        '@context': 'https://schema.org',
        '@type': 'BreadcrumbList',
        itemListElement: [
          { '@type': 'ListItem', position: 1, name: 'Home', item: this.baseUrl },
          { '@type': 'ListItem', position: 2, name: `${t} Options`, item: `${this.baseUrl}/options/${t}` }
        ]
      }
    ]);
  }

  addStructuredData(data: object | object[]): void {
    // Remove previously injected dynamic blocks
    const existing = this.document.querySelectorAll('script[type="application/ld+json"][data-dynamic]');
    existing.forEach(el => el.remove());

    const items = Array.isArray(data) ? data : [data];
    items.forEach(item => {
      const script = this.document.createElement('script');
      script.type = 'application/ld+json';
      script.setAttribute('data-dynamic', 'true');
      script.text = JSON.stringify(item);
      this.document.head.appendChild(script);
    });
  }

  private updateCanonical(url: string): void {
    let link: HTMLLinkElement = this.document.querySelector("link[rel='canonical']");
    if (!link) {
      link = this.document.createElement('link');
      link.setAttribute('rel', 'canonical');
      this.document.head.appendChild(link);
    }
    link.setAttribute('href', url);
  }
}
