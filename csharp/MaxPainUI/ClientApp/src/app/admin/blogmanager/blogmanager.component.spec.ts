import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { BlogmanagerComponent } from './blogmanager.component';

describe('BlogmanagerComponent', () => {
  let component: BlogmanagerComponent;
  let fixture: ComponentFixture<BlogmanagerComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ BlogmanagerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BlogmanagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
