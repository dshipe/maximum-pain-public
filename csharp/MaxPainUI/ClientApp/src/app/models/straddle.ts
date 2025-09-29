export class SdlChn {
    stock: string;
    stockPrice: number;
    interestRate: number;
    volatility: number;
    createdOn: Date;

    straddles: Array<Sdl>;
    prices: Array<StkPrc>;

    constructor(jsonObj: SdlChn) {
        Object.keys(jsonObj).map((key)=>{ this[key] = jsonObj[key] });
        this.straddles = [];
        for (let s of jsonObj.straddles) { this.straddles.push(new Sdl(s)) };
    }
}

export class Sdl {
    ot : string;

    clp: number;
    ca: number;
    cb: number;
    coi: number;
    cv: number;

    civ: number;
    c1sd: number;
    cde: number;
    cga: number;
    cth: number;
    cve: number;
    crh: number;

    plp: number;
    pa: number;
    pb: number;
    poi: number;
    pv: number;

    piv: number;
    p1sd: number;
    pde: number;
    pga: number;
    pth: number;
    pve: number;
    prh: number;

    get ticker(): string {
        return this.ot.substr(0, this.ot.length - 15);
    }    

    get mint(): number {
        const y: string = this.ot.substr(this.ot.length - 15, 2);
        const m: string = this.ot.substr(this.ot.length - 13, 2);
        const d: string = this.ot.substr(this.ot.length - 11, 2);
        return Number("20" + y + m + d);
    }

    get mstr(): string {
        const y: string = this.ot.substr(this.ot.length - 15, 2);
        const m: string = this.ot.substr(this.ot.length - 13, 2);
        const d: string = this.ot.substr(this.ot.length - 11, 2);
        return m+"/"+d+"/20"+y;
    }

    get mymd(): string {
      const y: string = this.ot.substr(this.ot.length - 15, 2);
      const m: string = this.ot.substr(this.ot.length - 13, 2);
      const d: string = this.ot.substr(this.ot.length - 11, 2);
      return "20" + y +  m + d;
    }


    get maturity(): Date {
        const y: string = this.ot.substr(this.ot.length - 15, 2);
        const m: string = this.ot.substr(this.ot.length - 13, 2);
        const d: string = this.ot.substr(this.ot.length - 11, 2);
        return new Date(Number("20"+y), Number(m)-1, Number(d));
    }

    get type(): string {
        return this.ot.substr(this.ot.length - 9, 1);
    }

    get strike(): number {
        const fragment: string = this.ot.substr(this.ot.length - 8, 8);
        return Number(fragment)/1000;
    }

    get ga0(): number {
        return this.cga-this.pga;
    }       
    
    get daysToExpiration(): number {
        let current: Date = new Date();
        current.setHours(0,0,0,0);
        let diff: number = Math.abs(this.maturity.getTime() - current.getTime());
        //return Number(Math.ceil(diff / (1000 * 3600 * 24)));         
        return Number(diff / (1000 * 3600 * 24));         
    }

    public oneStdDev(stockPrice: number, isPut: boolean): number {
        let days: number = this.daysToExpiration;
        let sqroot: number = Math.sqrt(days/365.0); 
        let iv: number = this.civ/100.0;
        if (isPut) iv = this.piv/100.0
        let result = stockPrice * iv * sqroot;
        /*
        if (this.strike==385)
        {
            console.log("strike="+this.strike
                +"\r\nmaturity="+this.maturity
                +"\r\ndays="+days
                +"\r\nsqroot="+sqroot
                +"\r\niv="+iv
                +"\r\nstockPrice="+stockPrice
                +"\r\nresult="+result);
        }
        */
        return result;
    }

    public intrinsicValue(stockPrice: number, isPut: boolean): number {
        let value: number = stockPrice-this.strike;
        if (isPut) value = 0-(this.strike-stockPrice);
        if (value<0) value = 0;
        return value;
    }

    public timeValue(stockPrice: number, isPut: boolean): number {
        let value: number = this.ca - this.intrinsicValue(stockPrice, isPut);
        if (isPut) value = this.pa - this.intrinsicValue(stockPrice, isPut);
        if (value<0) value = 0;
        return value;
    }

    constructor(jsonObj: any) {
        Object.keys(jsonObj).map((key)=>{ this[key] = jsonObj[key] });
    }
}

export class StkPrc {
    d: string;
    p: number;
}
