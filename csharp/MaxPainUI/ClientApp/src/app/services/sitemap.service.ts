import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SitemapService {
  
  generateSitemap(tickers: string[], blogPosts: any[]): string {
    const baseUrl = 'https://maximum-pain.com';
    const now = new Date().toISOString().split('T')[0];
    
    let xml = '<?xml version="1.0" encoding="UTF-8"?>\n';
    xml += '<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n';
    
    // Homepage
    xml += this.createUrl(baseUrl, now, '1.0', 'daily');
    
    // Static pages
    const staticPages = [
      '/screener/changeprice',
      '/screener/openinterest',
      '/screener/changeopeninterest',
      '/outside-oi-walls',
      '/contact',
      '/blog',
      '/daily-scan',
      '/market-direction'
    ];
    
    staticPages.forEach(page => {
      xml += this.createUrl(`${baseUrl}${page}`, now, '0.8', 'weekly');
    });
    
    // Ticker pages
    tickers.forEach(ticker => {
      xml += this.createUrl(`${baseUrl}/options/${ticker}`, now, '0.9', 'daily');
    });
    
    // Blog posts
    blogPosts.forEach(post => {
      const slug = post.title.replace(/\s+/g, '-');
      xml += this.createUrl(`${baseUrl}/blog/archive/${slug}`, post.lastModified || now, '0.7', 'monthly');
    });
    
    xml += '</urlset>';
    return xml;
  }
  
  private createUrl(loc: string, lastmod: string, priority: string, changefreq: string): string {
    return `  <url>\n` +
           `    <loc>${loc}</loc>\n` +
           `    <lastmod>${lastmod}</lastmod>\n` +
           `    <changefreq>${changefreq}</changefreq>\n` +
           `    <priority>${priority}</priority>\n` +
           `  </url>\n`;
  }
}
