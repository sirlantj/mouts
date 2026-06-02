import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  template: `
    @if (open()) {
      <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div class="fixed inset-0 bg-black/50 backdrop-blur-sm" (click)="onCancel()"></div>
        <div class="relative bg-white rounded-2xl shadow-2xl p-6 w-full max-w-sm animate-in">
          <div class="flex items-center gap-3 mb-3">
            <div class="flex items-center justify-center w-10 h-10 bg-red-100 rounded-full">
              <svg class="w-5 h-5 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"/>
              </svg>
            </div>
            <h3 class="text-lg font-semibold text-slate-900">{{ title() }}</h3>
          </div>
          <p class="text-sm text-slate-600 mb-6 ml-[52px]">{{ message() }}</p>
          <div class="flex gap-3 justify-end">
            <button (click)="onCancel()" class="px-4 py-2 text-sm font-medium text-slate-700 bg-slate-100 hover:bg-slate-200 rounded-lg transition-colors cursor-pointer">
              Back
            </button>
            <button (click)="onConfirm()" class="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors cursor-pointer">
              Confirm
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class ConfirmDialogComponent {
  open = input(false);
  title = input('Confirm');
  message = input('Are you sure?');
  confirmed = output<void>();
  cancelled = output<void>();

  onConfirm() { this.confirmed.emit(); }
  onCancel() { this.cancelled.emit(); }
}
