/*
set title
https://stackoverflow.com/questions/47900447/how-to-change-page-title-with-routing-in-angular-application
*/

import { Component, OnInit, Renderer2 } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { filter, map } from "rxjs/operators";
import { ThemeService } from './services/theme.service';
import { SidebarService } from './services/sidebar.service';

@Component( {
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit{

  sidebarExpanded = true;
  public sidebar_status: boolean = true;
  public bootstrap_theme: string = "dark";

  constructor (
      private themeService: ThemeService, 
      private sidebarService: SidebarService,
      private renderer: Renderer2,
      private router: Router, 
      private activatedRoute: ActivatedRoute, 
      private titleService: Title, 
      private metaService: Meta
  ) {
      this.router.events.pipe(
          filter(event => event instanceof NavigationEnd),
          map(() => {
              let child = this.activatedRoute.firstChild;
              while (child) {
                  if (child.firstChild) {
                      child = child.firstChild;
                  } else if (child.snapshot.data && child.snapshot.data['title']) {
                      return child.snapshot.data['title'];
                  } else {
                      return null;
                  }
              }
              return null;
          })
      ).subscribe( (data: any) => {
          if (data) {
              this.titleService.setTitle(data);
			this.metaService.updateTag({ name: 'description', content: data })
          }
      });
        
  }

  ngOnInit(): void {
      this.themeService.themeChanges().subscribe(theme => {
        console.log("app.component.ts: themeChanges theme.newValue="+theme.newValue)

        let local_theme = "dark";
        if (theme.newValue == "bootstrap") local_theme = "light";

        const body = document.body as HTMLElement
        body.setAttribute('data-bs-theme', local_theme);
      })
    }

    toggleSidebar(): void {
      //console.log("app.component.ts: toggleSidebar()");
      this.sidebarService.setSidebar();
    }          
}

