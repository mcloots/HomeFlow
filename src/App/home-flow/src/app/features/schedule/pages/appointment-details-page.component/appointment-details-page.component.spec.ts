import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppointmentDetailsPageComponent } from './appointment-details-page.component';

describe('AppointmentsDetailsPageComponent', () => {
  let component: AppointmentDetailsPageComponent;
  let fixture: ComponentFixture<AppointmentDetailsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentDetailsPageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentDetailsPageComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
