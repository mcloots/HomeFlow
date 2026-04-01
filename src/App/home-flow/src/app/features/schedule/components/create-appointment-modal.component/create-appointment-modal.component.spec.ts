import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AppContextStore } from '../../../../core/context/app-context.store';
import { AppointmentApiService } from '../../data-access/appointment-api.service';
import { CreateAppointmentModalComponent } from './create-appointment-modal.component';

describe('CreateAppointmentModalComponent', () => {
  let component: CreateAppointmentModalComponent;
  let fixture: ComponentFixture<CreateAppointmentModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateAppointmentModalComponent],
      providers: [
        {
          provide: AppointmentApiService,
          useValue: {
            createAppointment: () => of({}),
          },
        },
        {
          provide: AppContextStore,
          useValue: {
            hasTenantId: () => true,
            hasHouseholdId: () => true,
            tenantId: () => 'tenant-id',
            householdId: () => 'household-id',
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateAppointmentModalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
