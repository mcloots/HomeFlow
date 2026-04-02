import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  readonly navItems = [
    { label: 'Schedule', path: '/schedule' },
    { label: 'Chores', path: '/chores' },
    { label: 'Bills', path: '/billing' },
  ];
}
