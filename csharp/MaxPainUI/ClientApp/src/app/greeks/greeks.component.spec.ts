import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { GreeksComponent } from './greeks.component';

describe('GreeksComponent', () => {
  let component: GreeksComponent;
  let fixture: ComponentFixture<GreeksComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ GreeksComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GreeksComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
