import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface ThemeObject {
  oldValue: string;
  newValue: string;
};

@Injectable({
  providedIn: 'root'
})
export class ThemeService {

  initialSetting: ThemeObject = {
    oldValue: null, 
    newValue: 'dark'
  };

  themeSelection: BehaviorSubject<ThemeObject> =  new BehaviorSubject<ThemeObject>(this.initialSetting);

  constructor() { }

  setTheme(theme: string) {
    //console.log("theme.service.ts: theme="+theme);

    this.themeSelection.next(
      {
        oldValue: this.themeSelection.value.newValue, 
        newValue: theme
      });
  }

  themeChanges(): Observable<ThemeObject> {
    return this.themeSelection.asObservable();
  }
}
