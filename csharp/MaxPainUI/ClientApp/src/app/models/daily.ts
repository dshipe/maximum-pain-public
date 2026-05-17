export class Daily {
    ticker: string;
    source: string;
    date: Date;
    open: number;
    high: number;
    low: number; 
    close: number;
    adjClose: number;
    volume: number;

    constructor(jsonObj?: any) {
        if (jsonObj) {
            Object.keys(jsonObj).forEach(key => { this[key] = jsonObj[key]; });
        }
    }
} 
