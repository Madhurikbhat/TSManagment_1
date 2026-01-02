import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
})
export class Login {
  username = '';
  password = '';

  constructor(private router: Router) {}

  login() {
    const user = this.username?.trim().toLowerCase();
    if (user === 'manager') {
      this.router.navigate(['/manager']);
    } else if (user === 'employee') {
      this.router.navigate(['/employee']);
    } else {
      alert('Invalid credentials. Use "employee" or "manager" as username.');
    }
  }
}
