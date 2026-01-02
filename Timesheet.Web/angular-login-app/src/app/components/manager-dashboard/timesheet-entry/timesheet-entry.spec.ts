import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimesheetEntry } from './timesheet-entry';

describe('TimesheetEntry', () => {
  let component: TimesheetEntry;
  let fixture: ComponentFixture<TimesheetEntry>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TimesheetEntry]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TimesheetEntry);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
