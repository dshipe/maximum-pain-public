export interface OutsideOIWall {
    id: number;
    ticker: string;
    maturity: Date;
    isMonthlyExp: boolean;
    sumOI: number;
    putOI: number;
    callOI: number;
    stockPrice: number;
    putStrike: number;
    callStrike: number;
} 