import { Component, OnInit } from '@angular/core';
import { SeoService } from '../services/seo.service';

@Component({
  selector: 'app-glossary',
  templateUrl: './glossary.component.html'
})
export class GlossaryComponent implements OnInit {
  
  constructor(private seo: SeoService) {}
  
  ngOnInit() {
    this.seo.updateMetaTags({
      title: 'Options Trading Glossary - Max Pain Terms and Definitions',
      description: 'Comprehensive glossary of options trading terms including max pain, open interest, Greeks, and more. Learn the terminology used in options analysis.',
      keywords: 'options glossary, max pain definition, open interest explained, options terms, trading terminology',
      url: 'https://maximum-pain.com/glossary'
    });
    
    this.seo.addStructuredData({
      "@context": "https://schema.org",
      "@type": "FAQPage",
      "mainEntity": [
        {
          "@type": "Question",
          "name": "What is Max Pain?",
          "acceptedAnswer": {
            "@type": "Answer",
            "text": "Max pain is the strike price where option holders experience the greatest financial loss at expiration, while option writers maximize profit. It's calculated by finding the price point where the total value of all outstanding call and put options is minimized."
          }
        },
        {
          "@type": "Question",
          "name": "What is Open Interest?",
          "acceptedAnswer": {
            "@type": "Answer",
            "text": "Open interest is the total number of outstanding option contracts that have not been closed or exercised. It represents the number of active positions in the market and indicates where traders have established positions."
          }
        }
      ]
    });
  }
}
