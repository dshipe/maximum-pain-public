import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';

import { DataService } from './services/data.service';
import { UtilsService } from './services/utils.service';
import { StateService } from './services/state.service';

import { OptionsComponent } from './options/options.component';
import { StackedComponent } from './stacked/stacked.component';
import { MaxpainComponent } from './maxpain/maxpain.component';
import { StraddleComponent } from './straddle/straddle.component';
import { ChartComponent } from './chart/chart.component';
import { ScreenerComponent } from './screener/screener.component';
import { ScreenerChildComponent } from './screener-child/screener-child.component';
import { ScreenerMaxPainComponent } from './screener-max-pain/screener-max-pain.component';
import { HistoryComponent } from './history/history.component';
import { MaxpainHistoryComponent } from './maxpain-history/maxpain-history.component';
import { SpreadComponent } from './spread/spread.component';
import { GreeksComponent } from './greeks/greeks.component';
import { DownloadCsvComponent } from './download-csv/download-csv.component';

import { ContactComponent } from './contact/contact.component';
import { SubscribeComponent } from './subscribe/subscribe.component';
import { ScheduledTaskComponent } from './scheduled-task/scheduled-task.component';
import { BlogComponent } from './blog/blog.component';
import { BloghomeComponent } from './bloghome/bloghome.component';
import { AdsenseComponent } from './adsense/adsense.component';
import { SummaryComponent } from './summary/summary.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { AdsenseModule } from 'ng2-adsense';

import { BlogmanagerComponent } from './admin/blogmanager/blogmanager.component';
import { EmailStatComponent } from './admin/email-stat/email-stat.component';
import { ImportLogComponent } from './admin/import-log/import-log.component';
import { MessageComponent } from './admin/message/message.component';
import { UserTweetsComponent } from './admin/user-tweets/user-tweets.component';
import { IvComponent } from './iv/iv.component';
import { HopComponent } from './admin/hop/hop.component';
import { ImportComponent } from './admin/import/import.component';
import { OutsideoiwallsComponent } from './outsideoiwalls/outsideoiwalls.component';
import { CupWithHandleComponent } from './cup-with-handle/cup-with-handle.component';
import { DailyScanComponent } from './daily-scan/daily-scan.component';
import { MarketDirectionComponent } from './market-direction/market-direction.component';
import { SidebarComponent } from './sidebar/sidebar.component';


@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    MessageComponent,
    StraddleComponent,
    MaxpainComponent,
    OptionsComponent,
    StackedComponent,
    ChartComponent,
    ScreenerComponent,
    ScreenerChildComponent,
    ContactComponent,
    HistoryComponent,
    MaxpainHistoryComponent,
    SubscribeComponent,
    ScheduledTaskComponent,
    BlogComponent,
    BloghomeComponent,
    AdsenseComponent,
    EmailStatComponent,
    SummaryComponent,
    NotFoundComponent,
    UserTweetsComponent,
    SpreadComponent,
    GreeksComponent,
    ImportLogComponent,
    DownloadCsvComponent,
    BlogmanagerComponent,
    IvComponent,
    ScreenerMaxPainComponent,
    HopComponent,
    ImportComponent,
    OutsideoiwallsComponent,
    CupWithHandleComponent,
    DailyScanComponent,
    MarketDirectionComponent,
    SidebarComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
  	AdsenseModule.forRoot(),
    RouterModule.forRoot([
    { path: '', component: HomeComponent, data: { title: 'Stock Option Max Pain' }, pathMatch: 'full' },
    { path: 'cup-with-handle', component: CupWithHandleComponent },
    { path: 'daily-scan', component: DailyScanComponent },
    { path: 'market-direction', component: MarketDirectionComponent },
    { path: 'options', component: OptionsComponent },
    { path: 'options/:id', component: OptionsComponent },
    { path: 'stock-options', component: OptionsComponent },
    { path: 'stock-options-maximum-pain', component: OptionsComponent },
    { path: 'stacked', component: StackedComponent },
    { path: 'stacked/:id', component: StackedComponent },
    { path: 'max-pain.aspx', component: OptionsComponent },
    { path: 'greeks', component: GreeksComponent },
    { path: 'greeks/:id', component: GreeksComponent },
    { path: 'iv', component: IvComponent },
    { path: 'iv/:id', component: IvComponent },
    { path: 'history', component: HistoryComponent },
    { path: 'history/:id', component: HistoryComponent },
    { path: 'maxpain-history', component: MaxpainHistoryComponent },
    { path: 'maxpain-history/:id', component: MaxpainHistoryComponent },
    { path: 'spreads', component: SpreadComponent },
    { path: 'spreads/:id', component: SpreadComponent },
    { path: 'screener/:id', component: ScreenerComponent },
    { path: 'screenerMaxPain', component: ScreenerMaxPainComponent },
    { path: 'outside-oi-walls', component: OutsideoiwallsComponent },
    { path: 'contact', component: ContactComponent },
    { path: 'blog', component: BloghomeComponent },
    { path: 'blog/archive/:id', component: BlogComponent },
    { path: 'scheduled-task', component: ScheduledTaskComponent, data: { title: 'Scheduled Task - Stock Option Max Pain' } },
    { path: 'download-csv', component: DownloadCsvComponent, data: { title: 'Download CSV - Stock Option Max Pain' } },
    { path: 'download-csv/:id', component: DownloadCsvComponent, data: { title: 'Download CSV - Stock Option Max Pain' } },
    { path: 'not-found', component: NotFoundComponent },
    { path: 'admin/blogmanager', component: BlogmanagerComponent },
    { path: 'admin/email-stat', component: EmailStatComponent, data: { title: 'Email - Stock Option Max Pain' } },
    { path: 'admin/import', component: ImportComponent },
    { path: 'admin/import-log', component: ImportLogComponent },
    { path: 'admin/message', component: MessageComponent, data: { title: 'Message - Stock Option Max Pain' } },
    { path: 'admin/hop', component: HopComponent, data: { title: 'Hop - Stock Option Max Pain' } },
    { path: 'admin/usertweets', component: UserTweetsComponent, data: { title: 'User Tweets - Stock Option Max Pain' } },
    { path: '**', redirectTo: 'not-found' },
], { relativeLinkResolution: 'legacy' })
  ],
  providers: [DataService, UtilsService, StateService], 
  bootstrap: [AppComponent]
})

export class AppModule { }
