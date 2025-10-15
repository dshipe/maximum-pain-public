import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SidebarService {

  sidebarSelection: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  constructor() {
    this.setSidebar();
  }

  setSidebar() {
    let value: boolean = this.sidebarSelection.value;
    //console.log("sidebar.service.ts: value=" + !value);
    this.sidebarSelection.next(!value);
  }

  sidebarChanges(): Observable<boolean> {
    return this.sidebarSelection.asObservable();
  }
}
