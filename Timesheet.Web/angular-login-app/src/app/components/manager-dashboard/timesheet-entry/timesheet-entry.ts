import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Manager } from '../../../services/manager';
import { FormsModule } from '@angular/forms';

interface Timesheet {
  id: number;
  userId?: number;
  userName?: string;
  weekStartDate: string;
  weekEndDate: string;
  entries: TSEntry[];
}
interface TSEntry {
  id: number;
  timesheetId: number;
  status?: string;
  projectCodeId: number;
  projectName?: string;
  description: string;
  hours: { date: string; hours: number }[];
}
@Component({
  selector: 'app-timesheet-entry',
  imports: [CommonModule, FormsModule],
  templateUrl: './timesheet-entry.html',
  styleUrl: './timesheet-entry.css',
})
export class TimesheetEntry {
  private managerServ = inject(Manager);
  timesheets: Timesheet[] = [];
  projectsMap = new Map<number, string>();

  showRejectModal = false;
  rejectingTimesheetId: number | null = null;
  rejectComment = '';
  submittingReject = false;

  ngOnInit(): void {
    this.loadPending();
    // optional: preload projects mapping if Manager service exposes it
    if ((this.managerServ as any).getAllProjects) {
      (this.managerServ as any).getAllProjects().subscribe((ps: any[]) => {
        for (const p of ps || []) this.projectsMap.set(Number(p.id ?? p.projectId), p.name ?? p.projectName ?? p.code ?? String(p.id));
      });
    }
  }

   loadPending() {
    // expects manager.getPendingTimesheets() -> Observable<Timesheet[]>
    this.managerServ.getPendingTimesheets().subscribe((res: any) => {
      this.timesheets = Array.isArray(res) ? res : [];
    }, (err: any) => {
      console.error('Failed to load pending timesheets', err);
      this.timesheets = [];
    });
  }
  openRejectModal(tsId: number) {
    this.rejectingTimesheetId = tsId;
    this.rejectComment = '';
    this.showRejectModal = true;
  }

  cancelReject() {
    this.showRejectModal = false;
    this.rejectingTimesheetId = null;
    this.rejectComment = '';
  }

  submitReject() {
     if (!this.rejectingTimesheetId) return;
  this.submittingReject = true;
  const dto = { status: 4, comment: this.rejectComment }; // 4 = rejected (adjust if your API uses different code)
  this.managerServ.approveOrRejectTimesheet(this.rejectingTimesheetId, dto).subscribe(
    () => {
      this.submittingReject = false;
      this.cancelReject();
      this.loadPending();
    },
    (err: any) => {
      console.error('Failed to reject timesheet', err);
      this.submittingReject = false;
    }
  );
  }

  getWeekDates(t: Timesheet) {
    const start = new Date(t.weekStartDate);
    const dates: string[] = [];
    for (let i = 0; i < 5; i++) {
      const d = new Date(start);
      d.setDate(start.getDate() + i);
      const yyyy = d.getFullYear();
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      const dd = String(d.getDate()).padStart(2, '0');
      dates.push(`${yyyy}-${mm}-${dd}`);
    }
    return dates;
  }
   formatDayName(isoDate: string) {
    const d = new Date(isoDate);
    if (isNaN(d.getTime())) return isoDate;
    const names = ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'];
    return names[d.getDay()];
  }
  hoursFor(entry: TSEntry, date: string) {
    const h = (entry.hours || []).find(x => x.date === date);
    return h ? h.hours : 0;
  }

  projectNameFor(id: number) {
    return this.projectsMap.get(id) ?? String(id);
  }

  approve(tsId: number) {
    const dto = { status: 3, comment: '' }; // use correct "approved" status code if different
   this.managerServ.approveOrRejectTimesheet(tsId, dto).subscribe(
    () => this.loadPending(),
    (err: any) => console.error('Failed to approve timesheet', err)
  );
  }

}
