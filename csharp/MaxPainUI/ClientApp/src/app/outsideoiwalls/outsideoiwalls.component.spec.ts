import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OutsideoiwallsComponent } from './outsideoiwalls.component';

describe('OutsideoiwallsComponent', () => {
  let component: OutsideoiwallsComponent;
  let fixture: ComponentFixture<OutsideoiwallsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OutsideoiwallsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OutsideoiwallsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
