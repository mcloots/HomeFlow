import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AppointmentApiService } from '../../data-access/appointment-api.service';
import { AppointmentDetailsStore } from '../../data-access/appointment-details.store';
import { AppointmentDetailsPageComponent } from './appointment-details-page.component';

describe('AppointmentsDetailsPageComponent', () => {
  let component: AppointmentDetailsPageComponent;
  let fixture: ComponentFixture<AppointmentDetailsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentDetailsPageComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: () => 'appointment-id',
              },
            },
          },
        },
        {
          provide: AppointmentDetailsStore,
          useValue: {
            appointment: signal(null),
            isLoading: signal(false),
            error: signal(null),
            load: async () => {},
          },
        },
        {
          provide: AppointmentApiService,
          useValue: {
            updateAppointment: () => of(null),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentDetailsPageComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
