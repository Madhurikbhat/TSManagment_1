import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectCode } from './project-code';

describe('ProjectCode', () => {
  let component: ProjectCode;
  let fixture: ComponentFixture<ProjectCode>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProjectCode]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProjectCode);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
