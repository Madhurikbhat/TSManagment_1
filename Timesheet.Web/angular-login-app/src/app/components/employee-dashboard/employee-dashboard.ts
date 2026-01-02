import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Manager } from '../../services/manager';
import { Employee } from '../../services/employee';
import { WeekNavigator } from './week-navigator/week-navigator';
import { forkJoin, of } from 'rxjs';

interface TimeRow {
  id?: number;
  code: string;
  projectName: string;
  description: string;
  hours: Record<string, string>; // keyed by day label e.g. "Mon"
  status?: string | number;
}
@Component({
  selector: 'app-employee-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, WeekNavigator],
  templateUrl: './employee-dashboard.html',
  styleUrls: ['./employee-dashboard.css'],
})
export class EmployeeDashboard implements OnInit {
  weekDays = this.buildWeekLabels(new Date());
  users: { id: string; name: string }[] = [];
  selectedUser = '';
  projects: { id: string; name: string; code: string }[] = [];
  weekStart?: string;
  weekEnd?: string;
  loadingEntries = false;
  private originalRows: TimeRow[] = [];

  private managerServ = inject(Manager);
  private employeeServ = inject(Employee); // added


  ngOnInit(): void {
    // existing init (if any)
    this.loadUsers();
    this.rows.forEach(r => this.migrateRowHours(r));
  }

  rows: TimeRow[] = [
    {
      code: '',
      projectName: '',
      description: '',
      hours: this.emptyHours()
    }
  ];

  private loadUsers() {
    this.managerServ.getUserData().subscribe({
      next: (res: any) => {
        this.users = Array.isArray(res)
          ? res.map((u: any) => ({ id: u.id, name: u.name }))
          : [];
      },
      error: (err) => {
        console.error('Failed to load users', err);
        // fallback sample
        this.users = [{ id: '1', name: 'User1' }, { id: '2', name: 'User2' }];
      }
    });
  }

  addRow() {
    this.rows.push({
      code: '',
      projectName: '',
      description: '',
      hours: this.emptyHours()
    });
  }
  onUserChange(userId: string) {
    this.selectedUser = userId;
    if (userId) this.loadProjectsForUser(userId);
    else this.projects = [];

    this.loadTimesheetEntries(userId);
  }
  private loadTimesheetEntries(userId?: number | string) {
    const id = userId ?? this.selectedUser;
    if (!id || !this.weekStart || !this.weekEnd) {
      this.rows = [];
      return;
    }
    this.loadingEntries = true;
    this.employeeServ.getTimesheetEntries(id, this.weekStart, this.weekEnd).subscribe(
      (res: any[]) => {
        this.rows = Array.isArray(res) ? res.map(e => {
          const hoursObj: Record<string, any> = {};
          (this.weekDays || []).forEach(d => hoursObj[d.date] = '');
          (e.dailyHours || []).forEach((dh: any) => hoursObj[dh.date] = dh.hours);
          return {
            id: e.entryId,
            code: e.projectCodeId,
            projectName: e.projectName,
            description: e.description,
            hours: hoursObj,
            status: e.status
          };
        }) : [];
         this.originalRows = JSON.parse(JSON.stringify(this.rows));
         const last = this.rows[this.rows.length - 1];
        if (!last || this.isRowEmpty(last)) {
          // if no rows or last is already empty ensure at least one empty row exists
          if (!last) this.rows.push({ code: '', projectName: '', description: '', hours: this.emptyHours() });
        } else {
          // last has data => append an empty row
          this.rows.push({ code: '', projectName: '', description: '', hours: this.emptyHours() });
        }
        this.loadingEntries = false;
      },
      (err: any) => {
        console.error('Failed to load timesheet entries', err);
        this.rows = [];
        this.loadingEntries = false;
      }
    );
  }
   private isRowEmpty(row: TimeRow) {
    if (!row) return true;
    if (row.code) return false;
    if ((row.projectName || '').trim()) return false;
    if ((row.description || '').trim()) return false;
    for (const d of Object.values(row.hours || {})) {
      if (d !== '' && d !== null && d !== undefined && String(d).trim() !== '') return false;
    }
    return true;
  }
  private isRowChanged(row: TimeRow): boolean {
    if (!row) return false;
    if (!row.id) return true; // new row -> considered changed
    const orig = this.originalRows.find((o: any) => String(o.id) === String(row.id));
    if (!orig) return true;
    if (String(orig.code) !== String(row.code)) return true;
    if ((orig.projectName || '') !== (row.projectName || '')) return true;
    if ((orig.description || '') !== (row.description || '')) return true;
    // compare hours for current weekDays
    for (const d of this.weekDays || []) {
      const k = d.date;
      const a = (orig.hours && orig.hours[k]) ?? '';
      const b = (row.hours && row.hours[k]) ?? '';
      if (String(a) !== String(b)) return true;
    }
    return false;
  }

 
  private loadProjectsForUser(userId: string) {
    this.employeeServ.getAssignedProjectData(userId).subscribe(
      (res: any) => {
        this.projects = Array.isArray(res)
          ? res.map(p => ({ id: String(p.id), name: p.projectName, code: p.code }))
          : [];
      },
      (err: any) => {
        console.error('Failed to load projects for user', userId, err);
        this.projects = [];
      }
    );
  }

  onProjectChange(projectId: string, row: TimeRow) {
    row.code = projectId;
    const found = this.projects.find(p => p.id === projectId);
    row.projectName = found ? found.name : '';
  }

  // new: save draft (client-side for now)
  saveDraft() {
    console.log('Save draft requested', this.rows);
    const weekStartDate = this.weekDays?.[0]?.date ?? '';
    const weekEndDate = this.weekDays?.[this.weekDays.length - 1]?.date ?? '';

    const createEntries: any[] = [];
    const updateCalls: any[] = [];
    for (const r of this.rows) {
      if (r.id) {
        // only update if changed
        if (this.isRowChanged(r)) {
          const payload = this.buildEntryPayloadForRow(r, 1); // status 1 = draft
          updateCalls.push(this.employeeServ.updateTimesheetEntry(r.id, payload));
        }
      } else {
        // new row -> create as draft
        createEntries.push(this.buildEntryPayloadForRow(r, 1));
      }
    }

    const ops = [];
    if (updateCalls.length) ops.push(...updateCalls);
    if (createEntries.length) {
      const createPayload = {
        userId: Number(this.selectedUser) || 0,
        weekStartDate,
        weekEndDate,
        entries: createEntries
      };
      ops.push(this.employeeServ.ceateTimeSheetEntry(createPayload));
    }
    if (!ops.length) {
      console.log('Nothing to save as draft');
      return;
    }
    forkJoin(ops.map(op => op || of(null))).subscribe({
      next: (res) => {
        // refresh or update snapshot after successful save
        this.loadTimesheetEntries(this.selectedUser);
        console.log('Draft save results', res);
      },
      error: (err) => {
        console.error('Failed to save draft', err);
      }
    });
  }
  onHoursChange(value: any, row: TimeRow, date: string) {
    if (!row) return;
    const raw = value === '' || value === null ? '' : value;
    if (raw === '') {
      row.hours = row.hours || {};
      row.hours[date] = '';
      return;
    }
    let num = Number(raw);
    if (!isFinite(num) || isNaN(num)) {
      row.hours = row.hours || {};
      row.hours[date] = '';
      return;
    }
    if (num < 0) num = 0;
    if (num > 24) num = 24;
    // round to 2 decimals (adjust if you prefer quarters)
    num = Math.round(num * 100) / 100;
    row.hours = row.hours || {};
    row.hours[date] = num.toString();
  }

  // new: submit timesheet
  submitTimesheet() {
    console.log('Timesheet submit requested', this.rows);
    const weekStartDate = this.weekDays?.[0]?.date ?? '';
    const weekEndDate = this.weekDays?.[this.weekDays.length - 1]?.date ?? '';

    const newEntries: any[] = [];
    const updateCalls: any[] = [];
    for (const r of this.rows) {
      const hoursArr = (this.weekDays || []).map(d => {
        const raw = r.hours?.[d.date];
        const hrs = raw === undefined || raw === '' ? 0 : Number(raw);
        return { date: d.date, hours: Number.isFinite(hrs) ? hrs : 0 };
      });

      if (r.id) {
        // existing entry -> if changed update full entry, otherwise only update status
        if (this.isRowChanged(r)) {
          updateCalls.push(this.employeeServ.updateTimesheetEntry(r.id, this.buildEntryPayloadForRow(r, 2)));
        } else if(r.status !== 3) {
          // only status update
          updateCalls.push(this.managerServ.approveOrRejectTimesheet(r.id, { status: 2 }));
        }
      } else {
        // new entry -> create later as part of payload
        newEntries.push(this.buildEntryPayloadForRow(r, 2));
      }
    }

    const ops = [];
    if (updateCalls.length) ops.push(...updateCalls);
    if (newEntries.length) {
      const createPayload = {
        userId: Number(this.selectedUser) || 0,
        weekStartDate,
        weekEndDate,
        entries: newEntries
      };
      // reuse existing create method (keeps original name)
      ops.push(this.employeeServ.ceateTimeSheetEntry(createPayload));
    }

    if (!ops.length) {
      console.log('Nothing to submit');
      return;
    }

    forkJoin(ops.map(op => op || of(null))).subscribe({
      next: (res) => {
        // mark rows as submitted locally
        this.rows.forEach(r => { if (!r.status || r.status !== 2) r.status = 2; });
        this.loadTimesheetEntries(this.selectedUser);
        console.log('Submit/update results', res);
      },
      error: (err) => {
        console.error('Failed to submit/update timesheet entries', err);
      }
    });
  }
   private buildEntryPayloadForRow(row: TimeRow, status: number) {
    const hoursArr = (this.weekDays || []).map(d => {
      const raw = row.hours?.[d.date];
      const hrs = raw === undefined || raw === '' ? 0 : Number(raw);
      return { date: d.date, hours: Number.isFinite(hrs) ? hrs : 0 };
    });
    return {
      projectCodeId: Number(row.code) || 0,
      description: row.description ?? row.projectName ?? '',
      hours: hoursArr,
      status,
      comment: '',
      projectName:''
    };
  }


  removeRow(i: number) {
    this.rows.splice(i, 1);
  }

  private emptyHours(): Record<string, string> {
    const r: Record<string, string> = {};
    for (const d of this.weekDays) r[d.date] = '';
    return r;
  }

  private buildWeekLabels(ref: Date) {
    // returns array like [{ short: 'Mon', label: 'Mon (22-12)', date: '2025-12-22' }, ...]
    const dayStart = new Date(ref);
    const day = dayStart.getDay(); // 0..6 Sun..Sat
    // compute Monday of the current week
    const diffToMon = ((day + 6) % 7); // 0 if Mon
    const monday = new Date(dayStart);
    monday.setDate(dayStart.getDate() - diffToMon);

    const names = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
    return names.map((n, idx) => {
      const d = new Date(monday);
      d.setDate(monday.getDate() + idx);
      const yyyy = d.getFullYear();
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      const dd = String(d.getDate()).padStart(2, '0');
      const date = `${yyyy}-${mm}-${dd}`;
      return { short: n, label: `${n} (${dd}-${mm})`, date };
    });
  }
  private migrateRowHours(row: TimeRow) {
    if (!row.hours) row.hours = {};
    for (const d of this.weekDays) {
      const shortKey = d.short;
      const dateKey = d.date;
      const shortVal = row.hours[shortKey];
      if (shortVal !== undefined && shortVal !== '') {
        row.hours[dateKey] = shortVal;
      }
      // remove short-key to avoid duplicates
      if (shortKey in row.hours) {
        delete row.hours[shortKey];
      }
    }
  }
  onWeekChange(evt: { weekStart: string; weekEnd: string; dates: string[] }) {
    // build weekDays objects compatible with existing code: { short, label, date }
    const names = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
    this.weekDays = evt.dates.map((iso, idx) => {
      const d = new Date(iso);
      const dd = String(d.getDate()).padStart(2, '0');
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      return { short: names[idx] ?? '', label: `${names[idx]} (${dd}-${mm})`, date: iso };
    });
    this.weekStart = evt.weekStart;
    this.weekEnd = evt.weekEnd;
    this.loadTimesheetEntries();
    // migrate existing row hours to new date keys
    this.rows.forEach(r => this.migrateRowHours(r));
  }
  isEntryApproved(row: TimeRow): boolean {
     if (!row) return false;
    const s = row.status;
    if (s === undefined || s === null) return false;
    if (typeof s === 'number') return s === 3; // treat numeric 3 as 'Approved' (adjust if different)
    return String(s).toLowerCase() === 'approved';
  }
}
