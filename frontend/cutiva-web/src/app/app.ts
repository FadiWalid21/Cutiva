import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PwaInstallBanner } from './shared/pwa-install-banner/pwa-install-banner';

@Component({
  imports: [RouterOutlet, PwaInstallBanner],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {}
