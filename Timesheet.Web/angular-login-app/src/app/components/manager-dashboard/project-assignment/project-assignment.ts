import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Manager } from '../../../services/manager';
import { Observable, of } from 'rxjs';
import { catchError, concatMap, map, tap } from 'rxjs/operators';

interface AssignmentPayload {
  projectCodeId: string;
  userId: string;
  startDate?: string;
  endDate?: string;
}

interface DisplayAssignment {
  projectCodeId: string;
  projectName: string;
  userId: string;
  assigneeName: string;
  startDate?: string;
  endDate?: string;
}

@Component({
  selector: 'app-project-assignment',
   imports: [CommonModule, FormsModule],
  templateUrl: './project-assignment.html',
  styleUrl: './project-assignment.css',
})
export class ProjectAssignment implements OnInit {
  assignments: DisplayAssignment[] = [];
  newAssign: AssignmentPayload = { projectCodeId: '', userId: '', startDate: '', endDate: '' };
  assigneeOptions: any[] = [];
  projectOptions: any[] = [];
  isSaving = false;

  private manager = inject(Manager);

  ngOnInit(): void {
    this.loadAssignees().pipe(
      concatMap(() => this.loadProjects()),
      concatMap(() => this.loadProjectAssignments())
    ).subscribe({
      next: () => { /* all loaded sequentially */ },
      error: (err) => console.error('Failed loading project-assignment data', err)
    });
  }
  private loadAssignees(): Observable<any[]> {
    return this.manager.getUserData().pipe(
      map((res: any) => Array.isArray(res) ? res.map((u: any) => ({ id: u.id, name: u.name })) : []),
      tap((list: any[]) => this.assigneeOptions = list),
      catchError((err) => {
        console.error('Failed to load users', err);
        this.assigneeOptions = [];
        return of([]);
      })
    );
  }

   private loadProjects(): Observable<any[]> {
    return this.manager.getProjectData().pipe(
      map((res: any) => Array.isArray(res) ? res.map((p: any) => ({ id: p.id, name: p.projectName })) : []),
      tap((list: any[]) => this.projectOptions = list),
      catchError((err) => {
        console.error('Failed to load projects', err);
        this.projectOptions = [];
        return of([]);
      })
    );
  }
  private loadProjectAssignments(): Observable<any[]> {
    return this.manager.getProjectAssignments().pipe(
      map((res: any) => Array.isArray(res) ? res : []),
      tap((list: any[]) => {
        this.assignments = list.map((a: any) => ({
          projectCodeId: a.projectCodeId,
          projectName: this.getProjectName(a.projectCodeId),
          userId: a.userId,
          assigneeName: this.getAssigneeName(a.userId),
          startDate: a.startDate,
          endDate: a.endDate
        }));
      }),
      catchError((err) => {
        console.error('Failed to load project assignments', err);
        this.assignments = [];
        return of([]);
      })
    );
  }

  addAssignment() {
    if (!this.newAssign.projectCodeId || !this.newAssign.userId) return;
    this.isSaving = true;

    const payload: AssignmentPayload = {
      projectCodeId: this.newAssign.projectCodeId,
      userId: this.newAssign.userId,
      startDate: this.newAssign.startDate,
      endDate: this.newAssign.endDate
    };

    this.manager.createProjectAssignment(payload).subscribe({
      next: (res: any) => {
        const added = res ?? payload;
        const display: DisplayAssignment = {
          projectCodeId: added.projectCodeId ?? added.projectCode ?? added.projectCodeId,
          projectName: this.getProjectName(added.projectCodeId),
          userId: added.userId ?? added.assignee ?? added.userId,
          assigneeName: this.getAssigneeName(added.userId),
          startDate: added.startDate,
          endDate: added.endDate
        };
        this.assignments = [...this.assignments, display];
        this.reset();
        this.isSaving = false;
      },
      error: (err) => {
        console.error('Failed to create project assignment', err);
        alert("System.Exception: Project already assigned to user" );
        this.isSaving = false;
      }
    });
  }
  private getAssigneeName(id: string) {
    const found = this.assigneeOptions.find(a => String(a.id) === String(id));
    return found ? found.name : id;
  }

  private getProjectName(id: string) {
    const found = this.projectOptions.find(p => String(p.id) === String(id));
    return found ? found.name : id;
  }

  private reset() {
    this.newAssign = { projectCodeId: '', userId: '', startDate: '', endDate: '' };
  }
}
