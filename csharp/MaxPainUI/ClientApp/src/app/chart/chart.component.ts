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
import { MPChn } from "../models/MaxPainItem";
import { ChartInfo } from "../models/chart-info";
import { ThemeService } from '../services/theme.service';

declare var google: any;

@Component( {
  selector: 'app-chart',
  templateUrl: './chart.component.html',
  styleUrls: ['./chart.component.scss'],
  host: {
    '(window:resize)': 'onResize($event)'
  }
})
export class ChartComponent implements OnInit, AfterViewInit {

  @Input() apiName: string = "";
  @Input() json: string = "";
  @Input() title: string;
  @Input() key: string;
  @Input() degree: number;

  public chartForm: FormGroup;
  public chartInfo: ChartInfo;
  public containerWidth: number;
  public useMaterialChart: boolean = false;
  public isLoading: boolean = false;
  public isDarkMode: boolean = false;

  @ViewChild('myChart') myChart: ElementRef;

  drawChart = () => {
    if (!this.myChart) return false;
    if (!this.myChart.nativeElement) return false;

    let json: string = JSON.stringify(this.chartInfo);
    let localChartInfo: ChartInfo = JSON.parse(json);  

    var backColor="#FFFFFF";
    var foreColor="#000000";
    var gridColor="#cccccc"
    if (this.isDarkMode) {
      backColor ="#212529"
      foreColor="#999999";
      gridColor="#666666"
    }

    var data = new google.visualization.arrayToDataTable(localChartInfo.dataArray);
    if (localChartInfo.dataType == "date") {
      for (var i = 1; i < localChartInfo.dataArray.length; i++) {
        var element = localChartInfo.dataArray[i];
        element[0] = new Date(element[0] * 1000);
      }
      data = new google.visualization.arrayToDataTable(localChartInfo.dataArray);
      localChartInfo.hAxisFormat = "MMM dd";
    }

    var colorArray = [];
    for (var i = 0; i < localChartInfo.series.length; i++) {
      if (localChartInfo.series[i].color) colorArray.push(localChartInfo.series[i].color);
    }
	  if (colorArray.length==0) colorArray = null;
	
    var options = {
      title: localChartInfo.title,
      backgroundColor: backColor,
      titleTextStyle: { color: foreColor },
      legendTextStyle: { color: foreColor },
      colors: colorArray,
      vAxis: {
        title: localChartInfo.vAxisTitle,
        format: localChartInfo.vAxisFormat,
        textStyle: { color: foreColor },
        gridlines: { color: gridColor }
      },
      hAxis: {
        title: localChartInfo.hAxisTitle,
        format: localChartInfo.hAxisFormat,
        textStyle: { color: foreColor },
        gridlines: { color: gridColor, count: localChartInfo.series[0].points.length },
        slantedText: true,
        slantedTextAngle: 45,
        viewWindow: {
          min: localChartInfo.viewMin,
          max: localChartInfo.viewMax,
        }
      },
      //width: localChartInfo.width,
      width: this.containerWidth,
      //height: localChartInfo.height,
      height: this.containerWidth * .40,
      isStacked: false,
      explorer: {
        axis: 'horizontal',
        keepInBounds: true,
        maxZoomIn: 6.0
      },
    };
    //console.log(options);



    var chart;
    //console.log("drawChart: localChartInfo.chartType=" + localChartInfo.chartType);
    if (localChartInfo.chartType == "line") {
      if (this.useMaterialChart) {
        chart = new google.charts.Line(this.myChart.nativeElement);
        chart.draw(data, google.charts.Line.convertOptions(options));
      } else {
        chart = new google.visualization.LineChart(this.myChart.nativeElement);
        chart.draw(data, options);
      }
    }
    if (localChartInfo.chartType == "column" || localChartInfo.chartType == "stackedcolumn") {
      options.isStacked = false;
      if (localChartInfo.chartType == "stackedcolumn") options.isStacked = true;

      if (this.useMaterialChart) {
        chart = new google.charts.Bar(this.myChart.nativeElement);
        chart.draw(data, google.charts.Bar.convertOptions(options));
      } else {
        chart = new google.visualization.ColumnChart(this.myChart.nativeElement);
        chart.draw(data, options);
      }
    }
  }

  //added the data parameter
  constructor(
    private themeService: ThemeService, 
    private renderer: Renderer2,    
    private data: DataService,
    private utils: UtilsService,
  ) {
  }


  createForm(): void {
    this.chartForm = new FormGroup({
      "formChartType": new FormControl(),
      "formChartViewMin": new FormControl(),
      "formChartViewMax": new FormControl(),
    });
  }


  applyLocalOverride(): void {
    let chartType = this.chartForm.controls["formChartType"].value;
    //console.log("applyLocalOverride: chartType=" + chartType);
    if (chartType) this.chartInfo.chartType = chartType;

    if (this.chartForm.controls["formViewMin"]) {
      let viewMin = this.chartForm.controls["formViewMin"].value;
      if (viewMin) this.chartInfo.viewMin = viewMin;
    }
  }

  ngOnInit() {
    this.createForm();
    this.chartForm.valueChanges
      .subscribe(content => {
        console.log(content);
        this.repaint();
      });

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
    //this.containerWidth = this.myChart.nativeElement.offsetWidth;
    //this.isLoading = true;
  }

  ngOnChanges(changes: SimpleChanges) {
    this.repaint();
  }

  onResize(event) {
    if (this.apiName.length!=0 && this.json.length!=0)
    {
      this.containerWidth = event.target.innerWidth;
      this.drawChart();
    }
  }

  repaint(): boolean {
    if (this.apiName.length != 0 && this.json.length != 0)
    {
      console.log("repaint this.apiName=" + this.apiName);
      if (this.apiName=="MaxPainPost" || this.apiName=="HistoryMaxpainPost")
      {
        if (this.json)
        {
          console.log("repaint this.json=" + this.json);
          this.chartPost(this.apiName, this.json, this.title, this.key);
          return true;
        }
      }
      if (this.apiName=="IVPredictPost")
      {
        let tickerObj: Ticker = this.buildTickerObj(this.json);
        if (tickerObj.Maturity && tickerObj.JsonData) {
          this.chartIVPredictPost(this.apiName, tickerObj.JsonData, this.title, this.key, this.degree);
        }
        return true;
      }
      else
      {
        let tickerObj: Ticker = this.buildTickerObj(this.json);
        if (tickerObj.Maturity && tickerObj.JsonData) {
          this.chartPost(this.apiName, tickerObj.JsonData, this.title, this.key);
        }
        return true;
      }
    }
  }

  
  chartPost(apiName: string, json: string, title: string, key: string): boolean {
    this.isLoading = true;

    let observable$: Observable<ChartInfo> =
      this.data.getChartPost(apiName, json, title, key);

    observable$.subscribe(response => {
      this.chartInfo = response;
      this.applyLocalOverride();
      this.initializeChart();
      this.isLoading = false;
    });
    return true;
  }

  chartIVPredictPost(apiName: string, json: string, title: string, key: string, degree: number): boolean {
    this.isLoading = true;

    let observable$: Observable<ChartInfo> =
      this.data.getChartIVPredictPost(apiName, json, title, key, degree);

    observable$.subscribe(response => {
      this.chartInfo = response;
      this.initializeChart();
      this.isLoading = false;
    });
    return true;
  }

  initializeChart(): void {
    if (this.chartInfo.chartType == "stackedcolumn") this.useMaterialChart = false;

    if (this.useMaterialChart) {
      if (this.chartInfo.chartType == "line") google.charts.load('current', { 'packages': ['line'] });
      if (this.chartInfo.chartType == "stackedcolumn") google.charts.load('current', { 'packages': ['bar'] });
    } else {
      google.charts.load('current', { 'packages': ['corechart'] });
    }
    google.charts.setOnLoadCallback(this.drawChart);
    this.containerWidth = this.myChart.nativeElement.offsetWidth;
  }

  buildTickerObj(json: string): Ticker {
    let tickerObj: Ticker = new Ticker(this.utils);
    if (!json) return tickerObj;
    if (json.length < 20) return tickerObj;

    tickerObj.hydrate(json);
    tickerObj.Maturity = this.utils.ParseDate(tickerObj.Maturity);
    return tickerObj;
  }
}
