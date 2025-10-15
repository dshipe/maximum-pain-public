import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { EmailStatComponent } from './email-stat.component';

describe('EmailStatComponent', () => {
  let component: EmailStatComponent;
  let fixture: ComponentFixture<EmailStatComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EmailStatComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EmailStatComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
