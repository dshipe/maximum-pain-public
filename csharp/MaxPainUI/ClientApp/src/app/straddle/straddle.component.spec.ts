import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { StraddleComponent } from './straddle.component';

describe('StraddleComponent', () => {
  let component: StraddleComponent;
  let fixture: ComponentFixture<StraddleComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ StraddleComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StraddleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
