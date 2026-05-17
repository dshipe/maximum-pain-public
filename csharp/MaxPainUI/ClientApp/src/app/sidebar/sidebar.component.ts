import { Component, EventEmitter, Input, OnInit, Output, Inject, PLATFORM_ID } from "@angular/core";
import { isPlatformBrowser } from '@angular/common';
import { ThemeService } from '../services/theme.service';
import { SidebarService } from '../services/sidebar.service';

@Component( {
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {

  is_active: boolean; // = false
  theme: string = 'bootstrap-dark';

  constructor(
    private themeService: ThemeService,
    private sidebarService: SidebarService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.toggleTheme();
    }

    this.sidebarService.sidebarChanges().subscribe(isActive => {
      //console.log("sidebar.component.ts: sidebarChanges isActive=" + isActive);
      this.is_active = isActive;
    })
  }

  toggleTheme() {
    if (this.theme === 'bootstrap') {
      this.theme = 'bootstrap-dark';
    } else {
      this.theme = 'bootstrap';
    }

    //console.log("theme-toggle.component.ts: this.theme="+this.theme);
    this.themeService.setTheme(this.theme);
  }
}
