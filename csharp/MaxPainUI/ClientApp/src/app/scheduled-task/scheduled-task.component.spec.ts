import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ScheduledTaskComponent } from './scheduled-task.component';

describe('ScheduledTaskComponent', () => {
  let component: ScheduledTaskComponent;
  let fixture: ComponentFixture<ScheduledTaskComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ScheduledTaskComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ScheduledTaskComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
