import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

interface ChatMessage {
  role: string;
  content: string;
  createdAt: Date;
}

@Component({
  selector: 'app-chat-popup',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './chat-popup.component.html',
  styleUrls: ['./chat-popup.component.css']
})
export class ChatPopupComponent implements OnInit {
  isOpen = false;
  isMinimized = false;
  messages: ChatMessage[] = [];
  userMessage = '';
  isLoading = false;
  currentSessionId: number | null = null;
  isAuthenticated = false;

  private apiUrl = 'http://localhost:5000';

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit() {
    this.checkAuthentication();
  }

  private checkAuthentication() {
    const token = localStorage.getItem('token') || localStorage.getItem('accessToken');
    this.isAuthenticated = !!token;
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || localStorage.getItem('accessToken');
    if (token) {
      return new HttpHeaders().set('Authorization', `Bearer ${token}`);
    }
    return new HttpHeaders();
  }

  toggleChat() {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.isMinimized = false;
    }
  }

  minimizeChat() {
    this.isMinimized = !this.isMinimized;
  }

  closeChat() {
    this.isOpen = false;
    this.isMinimized = false;
  }

  sendMessage() {
    if (!this.userMessage.trim()) return;

    const userMsg = this.userMessage;
    this.messages.push({ role: 'User', content: userMsg, createdAt: new Date() });
    this.userMessage = '';
    this.isLoading = true;

    if (!this.isAuthenticated && this.requiresAuth(userMsg)) {
      this.isLoading = false;
      this.messages.push({ 
        role: 'Assistant', 
        content: 'To access your bookings and personal information, please login first.', 
        createdAt: new Date() 
      });
      return;
    }

    const body = {
      message: userMsg,
      sessionId: this.currentSessionId
    };

    const headers = this.isAuthenticated ? this.getHeaders() : new HttpHeaders();

    this.http.post<{ sessionId: number, reply: string, timestamp: Date }>(
      `${this.apiUrl}/chat`,
      body,
      { headers }
    ).subscribe({
      next: (response) => {
        this.currentSessionId = response.sessionId;
        this.messages.push({ role: 'Assistant', content: response.reply, createdAt: response.timestamp });
        this.isLoading = false;
        setTimeout(() => this.scrollToBottom(), 100);
      },
      error: (err) => {
        console.error('Chat error', err);
        if (err.status === 401) {
          this.messages.push({ 
            role: 'Assistant', 
            content: 'Your session has expired. Please login again to continue.', 
            createdAt: new Date() 
          });
        } else {
          this.messages.push({ role: 'Assistant', content: 'Sorry, I encountered an error. Please try again.', createdAt: new Date() });
        }
        this.isLoading = false;
      }
    });
  }

  private requiresAuth(message: string): boolean {
    const authKeywords = ['my booking', 'my bookings', 'my ticket', 'my tickets', 'my reservation', 
                          'my flight', 'my trips', 'my account', 'my profile', 'cancel booking'];
    const lowerMsg = message.toLowerCase();
    return authKeywords.some(keyword => lowerMsg.includes(keyword));
  }

  newChat() {
    this.currentSessionId = null;
    this.messages = [];
  }

  goToLogin() {
    this.router.navigate(['/login']);
    this.closeChat();
  }

  private scrollToBottom() {
    const container = document.querySelector('.popup-messages-container');
    if (container) {
      container.scrollTop = container.scrollHeight;
    }
  }
}
