export class MPChn {
    stock: string;
    stockPrice: number;
    maturity: Date;
    mint: number;
    maxPain: number;
    highCallOI: number;
    highPutOI: number;
    totalCallOI: number;
    totalPutOI: number;
    putCallRatio: number;
    minCash: number;
    maxCash: number;
    createdOn: Date;

    items: Array<MPItem>;

    get totalOI(): number {
        return this.totalCallOI + this.totalPutOI;
    }

    constructor(jsonObj: any) {
        Object.keys(jsonObj).map((key)=>{ this[key] = jsonObj[key] });
        this.items = [];
        for (let i of jsonObj.items) { this.items.push(new MPItem(i)) };
    }
}

export class MPItem {
    s: number;
    coi: number;
    cch: number;
    cpd: number;
    poi: number;
    pch: number;
    ppd: number;
    pd: number;

    get totalOI(): number {
        return this.coi + this.poi;
    }

    get totalCash(): number {
        return this.cch + this.pch;
    }

    constructor(jsonObj: any) {
        Object.keys(jsonObj).map((key)=>{ this[key] = jsonObj[key] });
    }
}


