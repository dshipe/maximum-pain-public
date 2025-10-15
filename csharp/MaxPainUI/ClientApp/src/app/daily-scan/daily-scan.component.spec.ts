import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DailyScanComponent } from './daily-scan.component';

describe('DailyScanComponent', () => {
  let component: DailyScanComponent;
  let fixture: ComponentFixture<DailyScanComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DailyScanComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DailyScanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
