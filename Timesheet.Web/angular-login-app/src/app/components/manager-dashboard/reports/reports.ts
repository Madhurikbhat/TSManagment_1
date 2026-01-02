import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { Manager } from '../../../services/manager';
import { FormsModule } from '@angular/forms';

export interface UserHours {
  userId: number;
  userName: string;
  totalHours: number;
}
export interface ProjectHours {
  projectCodeId: number;
  projectName: string;
  isBillable: boolean;
  totalHours: number;
}
export interface BillableHours {
  isBillable: boolean;
  totalHours: number;
}

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule,FormsModule],
  templateUrl: './reports.html',
  styleUrl: './reports.css',
})
export class Reports implements OnInit {
  // UI controls
  selectedView: 'employee' | 'project' | 'billable' = 'employee';
  startDate: string | null = null; // yyyy-mm-dd
  endDate: string | null = null;

  // data
  rows: UserHours[] = [];
  projectRows: ProjectHours[] = [];
  billableRows: BillableHours[] = [];

  // state
  loading = false;
  private manager = inject(Manager);

  ngOnInit(): void {
    // do not auto-load — load only on Search
  }

  search() {
    if (!this.selectedView || !this.startDate || !this.endDate) return;
    this.clearData();
    this.loading = true;

    if (this.selectedView === 'employee') {
      // expected Manager.getEmployeeHoursReport(startDate, endDate)
      (this.manager as any).getEmployeeHoursReport?.(this.startDate, this.endDate)?.subscribe?.(
        (res: any) => {
          this.rows = Array.isArray(res)
            ? res.map(r => ({ userId: Number(r.userId), userName: r.userName, totalHours: Number(r.totalHours) }))
            : [];
          this.loading = false;
        },
        (err: any) => { console.error(err); this.rows = []; this.loading = false; }
      );
      return;
    }

    if (this.selectedView === 'project') {
      (this.manager as any).getProjectHoursReport?.(this.startDate, this.endDate)?.subscribe?.(
        (res: any) => {
          this.projectRows = Array.isArray(res)
            ? res.map((r: any) => ({
                projectCodeId: Number(r.projectCodeId),
                projectName: r.projectName,
                isBillable: Boolean(r.isBillable),
                totalHours: Number(r.totalHours)
              }))
            : [];
          this.loading = false;
        },
        (err: any) => { console.error(err); this.projectRows = []; this.loading = false; }
      );
      return;
    }

    if (this.selectedView === 'billable') {
      (this.manager as any).getBillableHoursReport?.(this.startDate, this.endDate)?.subscribe?.(
        (res: any) => {
          this.billableRows = Array.isArray(res)
            ? res.map((r: any) => ({ isBillable: Boolean(r.isBillable), totalHours: Number(r.totalHours) }))
            : [];
          this.loading = false;
        },
        (err: any) => { console.error(err); this.billableRows = []; this.loading = false; }
      );
      return;
    }
  }

  private clearData() {
    this.rows = [];
    this.projectRows = [];
    this.billableRows = [];
  }

  trackById(_i: number, r: any) {
    return r?.userId ?? r?.projectCodeId ?? r?.isBillable ?? _i;
  }
}