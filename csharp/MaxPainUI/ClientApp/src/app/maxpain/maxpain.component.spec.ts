import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { MaxpainComponent } from './maxpain.component';

describe('MaxpainComponent', () => {
  let component: MaxpainComponent;
  let fixture: ComponentFixture<MaxpainComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ MaxpainComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MaxpainComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
