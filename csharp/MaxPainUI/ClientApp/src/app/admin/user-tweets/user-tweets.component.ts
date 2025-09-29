import { Component, OnInit, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router'
import { DataService } from '../../services/data.service';
import { UtilsService } from '../../services/utils.service';
import { UserTweet } from "../../models/UserTweet";

@Component( {
  selector: 'app-user-tweets',
  templateUrl: './user-tweets.component.html',
  styleUrls: ['./user-tweets.component.scss']
})
export class UserTweetsComponent implements OnInit {

  public tweets: UserTweet[];
  public userNames: string = "super_trades";
  public count: string = "20";
  
  //added the data parameter
  constructor(private actRoute: ActivatedRoute, private data: DataService, private utils: UtilsService ) { }

  ngOnInit() {
    this.actRoute.queryParams.subscribe(params => {
      this.userNames = params["userNames"];
      this.count = params["count"];
    });
  }

  ngAfterViewInit() {
   this.data.getUserTweets(this.userNames, this.count)
      .subscribe((data: UserTweet[]) => {
          this.tweets = data; 
        });
  }
}
