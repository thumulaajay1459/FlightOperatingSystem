import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ChatPopupComponent } from './chat-popup/chat-popup.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ChatPopupComponent],
  template: `
    <router-outlet></router-outlet>
    <app-chat-popup></app-chat-popup>
  `,
  standalone: true
})
export class RootComponent {}
