// responsive chart
// https://www.codexworld.com/make-responsive-pie-chart-with-google-charts/

import { AfterViewInit, OnInit, Renderer2, Input, Component, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, Observable, forkJoin, Subscription } from 'rxjs'
import { takeUntil, switchMap, tap, map } from 'rxjs/operators'

import { DataService } from '../services/data.service';
import { UtilsService } from '../services/utils.service';
import { Ticker } from "../models/ticker";
import { ThemeService } from '../services/theme.service';
import { Daily } from '../models/daily';

declare var google: any;

@Component({
  selector: 'app-chartcandle',
  templateUrl: './chartcandle.component.html',
  styleUrls: ['./chartcandle.component.scss'],
  host: {
    '(window:resize)': 'onResize($event)'
  }
})
export class ChartcandleComponent implements OnInit, AfterViewInit {

@Input() ticker: string = "";
@Input() drop20: boolean = false;
@Input() title: string = "";
@Input() startDate: string = "";
@Input() numDays: number = 0;
  
public json: string = "";
public containerWidth: number;
public useMaterialChart: boolean = false;
public isLoading: boolean = false;
public isDarkMode: boolean = false;
public tableData: Array<any> = [];

  @ViewChild('myChart') myChart: ElementRef;

  //https://hcapr4ndhwksq5dq7ird3yujpq0edbbt.lambda-url.us-east-1.on.aws/api/python/daily?ticker=hood

  drawChart = () => {
    if (!this.myChart) return false;
    if (!this.myChart.nativeElement) return false;
    if (!this.json || this.json.length === 0) return false;

    var green = '#009900'
    var red = '#cc0000'
    var blue = '#99ccff'
    var lightblue = '#cce0ff'
    var darkblue = '#001933'
    var white = '#ffffff'
    var black = '#000000'
    var dark = '#212529'
    var darkgray = '#333333'
    var gray = '#999999'
    var silver = '#eeeeee'
    var orange = '#ff9900'

    // Create a combined data table for price and volume
    var comboData = new google.visualization.DataTable();

    comboData.addColumn('string', 'Day');
    comboData.addColumn('number', 'Low');
    comboData.addColumn('number', 'Open');
    comboData.addColumn('number', 'Close');
    comboData.addColumn('number', 'High');
    comboData.addColumn('number', 'SMA10');
    comboData.addColumn('number', 'SMA20');
    comboData.addColumn('number', 'Volume');
    comboData.addColumn('number', 'Volume SMA 20');

    var jsonData = JSON.parse(this.json);
    console.log("ticker=" + this.ticker + " drop20=" + this.drop20);

    // Find the max and min price (for y-axis scaling)
    var maxPrice = 0;
    var minPrice = Infinity;
    for (var i = 0; i < jsonData.length; i++) {
        maxPrice = Math.max(maxPrice, jsonData[i].high);
        minPrice = Math.min(minPrice, jsonData[i].low);
    }

    // Find the max volume
    var maxVolume = 0;
    for (var i = 0; i < jsonData.length; i++) {
        maxVolume = Math.max(maxVolume, jsonData[i].volume);
    }

    // Calculate 20-day SMA for close price
    var sma20 = [];
    for (var i = 0; i < jsonData.length; i++) {
        if (i < 19) {
            sma20.push(null);
        } else {
            var sum = 0;
            for (var j = i - 19; j <= i; j++) {
                sum += jsonData[j].close;
            }
            sma20.push(sum / 20);
        }
    }

    // Calculate 10-day SMA for close price
    var sma10 = [];
    for (var i = 0; i < jsonData.length; i++) {
        if (i < 9) {
            sma10.push(null);
        } else {
            var sum = 0;
            for (var j = i - 9; j <= i; j++) {
                sum += jsonData[j].close;
            }
            sma10.push(sum / 10);
        }
    }

    // Calculate scale factor so max volume bar is 1/6 of max price (shorter bars)
    var volumeScale = (maxPrice / 12) / maxVolume;

    // Calculate 20-day SMA for volume
    var volumeSMA = [];
    for (var i = 0; i < jsonData.length; i++) {
        if (i < 19) {
            volumeSMA.push(null);
        } else {
            var sum = 0;
            for (var j = i - 19; j <= i; j++) {
                sum += jsonData[j].volume;
            }
            volumeSMA.push((sum / 20) * volumeScale);
        }
    }

    // Populate combined data with scaled volume and SMA
    var start = 0;
    if (this.drop20) start = 20;
    for (var i = start; i < jsonData.length; i++) {
        var dateObj = new Date(jsonData[i].date);
        var day = dateObj.getDay();
        if (day === 0 || day === 6) continue;
        var label = (dateObj.getMonth()+1) + '/' + dateObj.getDate() + '/' + String(dateObj.getFullYear()).slice(-2);
        comboData.addRow([
            label,
            jsonData[i].low,
            jsonData[i].open,
            jsonData[i].close,
            jsonData[i].high,
            sma10[i], // SMA 10
            sma20[i], // SMA 20
            jsonData[i].volume * volumeScale,
            volumeSMA[i]
        ]);
    }


    // Combined chart options
    var comboOptions = {
        legend: 'none',
        color: this.isDarkMode ? white : gray,
        backgroundColor: this.isDarkMode ? dark : white,
        seriesType: 'candlesticks',
        series: {
            0: {type: 'candlesticks', color: gray}, // Price
            1: {type: 'line', color: gray , lineWidth: 1}, // SMA 10
            2: {type: 'line', color: this.isDarkMode ? white : black, lineWidth: 1}, // SMA 20
            3: {type: 'bars', color: this.isDarkMode ? darkgray : lightblue, targetAxisIndex: 1, visibleInLegend: false, enableInteractivity: false}, // Volume (scaled, blends in)
            4: {type: 'line', color: this.isDarkMode ? darkgray : lightblue, lineWidth: 2, targetAxisIndex: 1} // Volume SMA 20 (blue line)
        },
        candlestick: {
            risingColor: { strokeWidth: 1, fill: green, stroke: green },
            fallingColor: { strokeWidth: 1, fill: red, stroke: red }
        },
        chartArea: {left: 80, top: 30, width: '90%', height: '85%'},
        vAxes: {
            0: { title: 'Price', gridlines: { color: this.isDarkMode ? darkgray : silver }, textStyle: {color: this.isDarkMode ? white : black}, viewWindow: { min: minPrice } },
            1: { textPosition: 'none', gridlines: { color: 'transparent' }, textStyle: {color: 'transparent'}, viewWindow: { min: null, max: maxPrice / 6 } }
        },
        hAxis: {
            gridlines: { color: this.isDarkMode ? darkgray : gray},
            textStyle: { fontSize: 10, color: this.isDarkMode ? white : black},
            // Format dates on the horizontal axis
            slantedText: true,
            slantedTextAngle: 45,
            showTextEvery: Math.ceil(jsonData.length / 20),
        },
        explorer: {
            axis: 'horizontal',
            keepInBounds: true,
            maxZoomIn: 6.0
        },
    };

    // Draw combined chart
    var chartElement = this.myChart.nativeElement;
    var comboChart = new google.visualization.ComboChart(chartElement);
    comboChart.draw(comboData, comboOptions);
}

  //added the data parameter
  constructor(
    private themeService: ThemeService, 
    private renderer: Renderer2,    
    private data: DataService,
    private utils: UtilsService,
  ) {
  }

  candleData(ticker: string)
  {
    let observable$: Observable<Array<Daily>>;
    
    if (this.startDate && this.numDays > 0) {
      observable$ = this.data.getTickerDataForDateRange(ticker, this.startDate, this.numDays);
    } else {
      observable$ = this.data.getDaily(this.ticker);
    }
    
    observable$.subscribe(
      response => {
        let dailies = response.map(item => new Daily(item));
        this.json = JSON.stringify(dailies);
        this.tableData = dailies.slice().reverse().slice(0, 30);
        this.initializeChart();
      },
      error => {
        console.log(error);
      });
  }  

  ngOnInit() {
    this.themeService.themeChanges().subscribe(theme => {
      console.log("chart.component.ts: themeChanges theme.newValue="+theme.newValue)
      let isDarkMode: boolean = false;
      if (theme.newValue == "bootstrap-dark") {
        isDarkMode = true;
      }
      if (isDarkMode != this.isDarkMode) {
        this.isDarkMode = isDarkMode;
        this.repaint();
      }
    })
  }

  ngAfterViewInit() {
    this.repaint()
  }  

  ngOnChanges(changes: SimpleChanges) {
    // Reload chart when ticker changes
    if (changes['ticker'] && this.ticker && this.ticker.length > 0) {
      this.candleData(this.ticker);
      return;
    }
    
    // Reload chart when date range changes (startDate or numDays)
    if (changes['startDate'] && this.startDate && this.startDate.length > 0 && this.ticker && this.ticker.length > 0) {
      this.candleData(this.ticker);
      return;
    }
    
    if (changes['numDays'] && this.numDays > 0 && this.ticker && this.ticker.length > 0) {
      this.candleData(this.ticker);
      return;
    }
  }

  onResize(event) {
    this.containerWidth = event.target.innerWidth;
    this.drawChart();
  }

  repaint(): boolean {
    this.initializeChart();
    return true;
  }

  initializeChart(): void {
    if (!this.myChart?.nativeElement) return;
    if (typeof google === 'undefined' || !google.charts) {
      setTimeout(() => this.initializeChart(), 100);
      return;
    }
    google.charts.load('current', { 'packages': ['corechart'] });
    google.charts.setOnLoadCallback(() => this.drawChart());
    this.containerWidth = this.myChart.nativeElement.offsetWidth;
  }
}
