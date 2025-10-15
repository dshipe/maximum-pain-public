import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ScreenerMaxPainComponent } from './screener-max-pain.component';

describe('ScreenerMaxPainComponent', () => {
  let component: ScreenerMaxPainComponent;
  let fixture: ComponentFixture<ScreenerMaxPainComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ScreenerMaxPainComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ScreenerMaxPainComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
