import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { MaxpainHistoryComponent } from './maxpain-history.component';

describe('MaxpainHistoryComponent', () => {
  let component: MaxpainHistoryComponent;
  let fixture: ComponentFixture<MaxpainHistoryComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ MaxpainHistoryComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MaxpainHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
