export interface MostActive {
  id: number;
  sortID: number;
  queryType: string;
  ticker: string;
  maturity: Date;
  strike: number;
  callPut: string;
  createdOn: Date;
  nextMaturity: boolean;

  openInterest: number;
  prevOpenInterest: number;
  changeOpenInterest: number;

  volume: number;
  prevVolume: number;
  changeVolume: number;

  price: number;
  prevPrice: number;
  changePrice: number;

  iv: number;
  prevIV: number;
  changeIV: number;
} 