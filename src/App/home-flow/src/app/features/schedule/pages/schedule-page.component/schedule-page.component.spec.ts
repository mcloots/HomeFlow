import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SchedulePageComponent } from './schedule-page.component';

describe('SchedulePageComponent', () => {
  let component: SchedulePageComponent;
  let fixture: ComponentFixture<SchedulePageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SchedulePageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SchedulePageComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
