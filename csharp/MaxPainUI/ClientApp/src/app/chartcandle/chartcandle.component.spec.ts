import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChartcandleComponent } from './chartcandle.component';

describe('ChartcandleComponent', () => {
  let component: ChartcandleComponent;
  let fixture: ComponentFixture<ChartcandleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ChartcandleComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ChartcandleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
