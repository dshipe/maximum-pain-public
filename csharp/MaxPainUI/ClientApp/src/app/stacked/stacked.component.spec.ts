import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { StackedComponent } from './stacked.component';

describe('StackedComponent', () => {
  let component: StackedComponent;
  let fixture: ComponentFixture<StackedComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ StackedComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StackedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
