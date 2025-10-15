// https://www.thecodecampus.de/blog/angular-universal-handle-404-set-status-codes/
// https://stackblitz.com/edit/ng-smart-404-page

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { take } from 'rxjs/operators';

@Component( {
  selector: 'app-not-found',
  templateUrl: './not-found.component.html',
  styleUrls: ['./not-found.component.scss']
})
export class NotFoundComponent implements OnInit {
  path: string;

  constructor(private route: ActivatedRoute) {}

  ngOnInit() {
	//this.path = this.route.toString();

	/* 
    this.route.data.pipe(take(1))
      .subscribe((data: { path: string }) => {
        this.path = data.path;
      });
	*/
  }
}

