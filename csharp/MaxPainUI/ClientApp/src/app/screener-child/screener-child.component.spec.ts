import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ScreenerChildComponent } from './screener-child.component';

describe('ScreenerChildComponent', () => {
  let component: ScreenerChildComponent;
  let fixture: ComponentFixture<ScreenerChildComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ScreenerChildComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ScreenerChildComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
