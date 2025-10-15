import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MarketDirectionComponent } from './market-direction.component';

describe('MarketDirectionComponent', () => {
  let component: MarketDirectionComponent;
  let fixture: ComponentFixture<MarketDirectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MarketDirectionComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MarketDirectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
