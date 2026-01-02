import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-week-navigator',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './week-navigator.html',
  styleUrls: ['./week-navigator.css']
})
export class WeekNavigator {
  @Input() referenceIso?: string; // optional initial date (yyyy-mm-dd)
  @Output() weekChange = new EventEmitter<{ weekStart: string; weekEnd: string; dates: string[] }>();

  private currentMonday = this.getMonday(this.referenceIso ? new Date(this.referenceIso) : new Date());

  ngOnInit() { this.emitWeek(); }

  prevWeek() { this.currentMonday.setDate(this.currentMonday.getDate() - 7); this.emitWeek(); }
  nextWeek() { this.currentMonday.setDate(this.currentMonday.getDate() + 7); this.emitWeek(); }
  goToToday() { this.currentMonday = this.getMonday(new Date()); this.emitWeek(); }

  get displayLabel() {
    const start = this.toIso(this.currentMonday);
    const endDate = new Date(this.currentMonday); endDate.setDate(this.currentMonday.getDate() + 6);
    return `${this.formatShort(start)} — ${this.formatShort(this.toIso(endDate))}`;
  }

  private emitWeek() {
    const dates: string[] = [];
    for (let i = 0; i < 7; i++) {
      const d = new Date(this.currentMonday);
      d.setDate(this.currentMonday.getDate() + i);
      dates.push(this.toIso(d));
    }
    const weekStart = dates[0];
    const weekEnd = dates[dates.length - 1];
    this.weekChange.emit({ weekStart, weekEnd, dates });
  }

  private getMonday(d: Date) {
    const copy = new Date(d);
    const day = copy.getDay(); // 0 Sun .. 6 Sat
    const diffToMon = (day + 6) % 7;
    copy.setDate(copy.getDate() - diffToMon);
    copy.setHours(0, 0, 0, 0);
    return copy;
  }

  private toIso(d: Date) {
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  private formatShort(iso: string) {
    const d = new Date(iso);
    const dd = String(d.getDate()).padStart(2,'0');
    const mm = d.toLocaleString(undefined, { month: 'short' });
    return `${dd} ${mm}`;
  }
}