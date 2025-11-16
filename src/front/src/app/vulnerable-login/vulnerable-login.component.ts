import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-vulnerable-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vulnerable-login.component.html',
  styleUrls: ['./vulnerable-login.component.css']
})
export class VulnerableLoginComponent implements OnInit {
  username: string = '';
  password: string = '';
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;
  attemptCount: number = 0;
  logs: { type: 'info' | 'success' | 'error', message: string }[] = [];

  regUsername: string = '';
  regPassword: string = '';
  registerError: string = '';
  registerSuccess: string = '';
  isRegisterLoading: boolean = false;

  passwords: string[] = [];
  isLoadingPasswords: boolean = false;

  private apiUrl = 'http://localhost:8080/api/vulnerable';

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadPasswords();
  }

  loadPasswords(): void {
    this.http.get('assets/100k-most-used-passwords-NCSC.txt', { responseType: 'text' }).subscribe({
      next: (data) => {
        this.passwords = data.split('\n').filter(p => p.trim().length > 0);
        console.log(`Loaded ${this.passwords.length} passwords`);
      },
      error: (error) => {
        console.error('Failed to load passwords:', error);
        this.passwords = ['123456', 'password', '123', 'admin', 'qwerty', '12345', 'letmein'];
      }
    });
  }

  onSubmit(): void {
    if (!this.username || !this.password) {
      this.errorMessage = 'Please fill all fields';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.attemptCount++;

    const credentials = {
      username: this.username,
      password: this.password
    };

    const timestamp = new Date().toLocaleTimeString();
    this.addLog(`[${timestamp}] Tentative ${this.attemptCount}: ${this.username}`);

    this.http.post(`${this.apiUrl}/login`, credentials).subscribe({
      next: (response: any) => {
        this.isLoading = false;
        this.successMessage = 'Login successful';
        this.addLog(`[${timestamp}] SUCCESS - Token received`, 'success');
        console.log('Login successful:', response);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Connection error';
        this.addLog(`[${timestamp}] FAILED - ${error.error?.message || 'Error'}`, 'error');
      }
    });
  }

  bruteForce(): void {
    if (this.passwords.length === 0) {
      this.errorMessage = 'Passwords not loaded yet';
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';
    this.addLog('=== BRUTE FORCE START ===');
    this.addLog(`Testing ${Math.min(100, this.passwords.length)} passwords...`);
    
    // Use first 100 passwords for demo
    const passwordsToTest = this.passwords.slice(0, 100);
    let delay = 0;

    passwordsToTest.forEach((pwd, index) => {
      setTimeout(() => {
        this.password = pwd;
        this.onSubmit();
      }, delay);
    });
  }

  showPasswordsInfo(): void {
    this.addLog(`First 10 passwords: ${this.passwords.slice(0, 10).join(', ')}`);
    console.log('All passwords:', this.passwords, '(check browser console for full list)');
  }

  private addLog(message: string, type: 'info' | 'success' | 'error' = 'info'): void {
    this.logs.unshift({ type, message });
    if (this.logs.length > 100) {
      this.logs.pop();
    }
  }

  clearLogs(type?: 'info' | 'success' | 'error'): void {
    if (type) {
      this.logs = this.logs.filter(log => log.type !== type);
    } else {
      this.logs = [];
      this.attemptCount = 0;
    }
  }

  getLogsByType(type: 'info' | 'success' | 'error'): { type: string, message: string }[] {
    return this.logs.filter(log => log.type === type);
  }

  onRegister(): void {
    if (!this.regUsername || !this.regPassword) {
      this.registerError = 'Please fill all fields';
      return;
    }

    this.isRegisterLoading = true;
    this.registerError = '';
    this.registerSuccess = '';

    const credentials = {
      username: this.regUsername,
      password: this.regPassword
    };

    this.http.post(`${this.apiUrl}/register`, credentials).subscribe({
      next: () => {
        this.isRegisterLoading = false;
        this.registerSuccess = 'Account created successfully';
        this.regUsername = '';
        this.regPassword = '';
      },
      error: (error) => {
        this.isRegisterLoading = false;
        this.registerError = error.error?.message || 'Registration failed';
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}
