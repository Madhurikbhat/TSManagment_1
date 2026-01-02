import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { EmployeeDashboard } from './components/employee-dashboard/employee-dashboard';
import { ManagerDashboard } from './components/manager-dashboard/manager-dashboard';

export const routes: Routes = [
	{ path: '', component: Login },
	{ path: 'login', component: Login },
	{ path: 'employee', component: EmployeeDashboard },
	{ path: 'manager', component: ManagerDashboard },
	{ path: '**', redirectTo: '' }
];
