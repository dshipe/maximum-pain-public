import { Data } from "@angular/router";

export class CupWithHandleHistory {
    id: number;
    title: string;
    ticker: string;
    startDate: Date;
    endDate: Date;
    rightPrice: number;
    handlePrice: number;
    currentPrice: number;
    isFailuer: boolean;
    gamma: number;
    base64: string;
    createdOn: Date;
    midnight: Date;
}
