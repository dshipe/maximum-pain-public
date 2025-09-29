import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
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
    private sidebarService: SidebarService
  ) { }

  ngOnInit(): void {
    this.toggleTheme();

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
