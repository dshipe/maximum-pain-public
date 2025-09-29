import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CupWithHandleComponent } from './cup-with-handle.component';

describe('CupWithHandleComponent', () => {
  let component: CupWithHandleComponent;
  let fixture: ComponentFixture<CupWithHandleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CupWithHandleComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CupWithHandleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
