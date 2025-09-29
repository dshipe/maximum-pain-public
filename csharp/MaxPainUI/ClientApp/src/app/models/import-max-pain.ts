export class ImportMaxPain {
    ticker: string;
    maturity: string;
    stockPrice: number;
    maxPain: number;
    totalCallOI: number;
    totalPutOI: number;
    highCallOI: number;
    highPutOI: number;
    createdOn: Date;
    
    public totalOI(): number {
        return this.totalCallOI+this.totalPutOI;
    }

    public difference(): number {
        return Math.abs(this.maxPain-this.stockPrice);
    }

    public percentDifference(): number {
        return this.difference()/this.stockPrice*100.0;
    }

    constructor(jsonObj: ImportMaxPain) {
        Object.keys(jsonObj).map((key)=>{ this[key] = jsonObj[key] });
    }    
}
