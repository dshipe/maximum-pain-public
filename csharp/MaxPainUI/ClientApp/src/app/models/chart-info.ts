export class ChartInfo {
    chartType: String;
    dataType: String;
    title: String;
    hAxisTitle: String;
    hAxisFormat: String;
    vAxisTitle: String;
    vAxisFormat: String;
    width: Number;
    height: Number;
    interval: Number;
    enable3D: boolean;
    isDarkMode: boolean;
    isTransparent: boolean;
    viewMin: 1;
    viewMax: 100;

    series: ChartSeries [] = [];

    dataArray : Array<{strike: Number, call: Number, put: Number}>;
}

export class ChartSeries {
    title: String;
    color: String;

    points: ChartPoint [] = [];
}

export class ChartPoint {
    x: Number;
    y: Number;
}
