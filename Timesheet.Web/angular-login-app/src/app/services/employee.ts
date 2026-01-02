import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class Employee {
   constructor(private http: HttpClient) {}

   getAssignedProjectData(userId: string) {
    return this.http.get(`https://localhost:44374/api/projectAssignments/user/${userId}/projects`);
  }

  ceateTimeSheetEntry(entry: any) {
    return this.http.post('https://localhost:44374/api/timesheet', entry);
  }
  getTimesheetEntries(userId: number | string, startDate?: string, endDate?: string) {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
        return this.http.get<any[]>(`https://localhost:44374/api/timesheet/user/${userId}/week`, { params });
  }
  updateTimesheetEntry(entryId: number | string, entry: any) {
    return this.http.post(`https://localhost:44374/api/timesheet/${entryId}/full-update`, entry);
  }
}
