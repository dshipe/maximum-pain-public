import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { UserTweetsComponent } from './user-tweets.component';

describe('UserTweetsComponent', () => {
  let component: UserTweetsComponent;
  let fixture: ComponentFixture<UserTweetsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ UserTweetsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserTweetsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
