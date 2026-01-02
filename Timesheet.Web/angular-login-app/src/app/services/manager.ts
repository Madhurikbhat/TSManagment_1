import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class Manager {
  
  constructor(private http: HttpClient) {}

  getProjectData() {
    return this.http.get('https://localhost:44374/api/project-codes');
  }

  createProjectCode(projectCode: any) {
    return this.http.post('https://localhost:44374/api/project-codes', projectCode);
  }
  updateProjectCode(id?: string | number, projectCode?: any) {
    return this.http.put(`https://localhost:44374/api/project-codes/${id}`, projectCode);
  }
  createProjectAssignment(assignment: any) {
    return this.http.post('https://localhost:44374/api/projectAssignments', assignment);
  }
  getUserData() {
    return this.http.get('https://localhost:44374/api/user');
  }
  getProjectAssignments() {
    return this.http.get('https://localhost:44374/api/projectAssignments');
  }
  getPendingTimesheets() {
    return this.http.get('https://localhost:44374/api/timesheet/pendingTimesheet');
  }
  approveOrRejectTimesheet(tsId: number, status:any) {
    return this.http.post(`https://localhost:44374/api/timesheet/${tsId}/status`, status);
  }
  getEmployeeHoursReport(startDate?: string, endDate?: string) {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    return this.http.get('https://localhost:44374/api/report/employee-hours', { params });
  
  }
  getProjectHoursReport() {
    return this.http.get('https://localhost:44374/api/report/project-hours');
  }
  getBillableHoursReport() {
    return this.http.get('https://localhost:44374/api/report/billable-hours');
  }
}