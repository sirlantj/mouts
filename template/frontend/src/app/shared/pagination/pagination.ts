import { Component, input, output, computed } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  template: `
    <div class="flex flex-col sm:flex-row items-center justify-between gap-3 pt-4">
      <span class="text-sm text-slate-500">
        {{ totalCount() }} registros — Página {{ currentPage() }} de {{ totalPages() }}
      </span>
      <div class="flex items-center gap-1">
        <button [disabled]="currentPage() <= 1" (click)="pageChange.emit(currentPage() - 1)"
          class="px-3 py-1.5 text-sm border border-slate-200 rounded-lg hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors cursor-pointer">
          ‹
        </button>
        @for (p of visiblePages(); track p) {
          <button [class]="p === currentPage()
            ? 'px-3 py-1.5 text-sm rounded-lg bg-blue-600 text-white font-medium cursor-pointer'
            : 'px-3 py-1.5 text-sm border border-slate-200 rounded-lg hover:bg-slate-50 text-slate-700 cursor-pointer'"
            (click)="pageChange.emit(p)">
            {{ p }}
          </button>
        }
        <button [disabled]="currentPage() >= totalPages()" (click)="pageChange.emit(currentPage() + 1)"
          class="px-3 py-1.5 text-sm border border-slate-200 rounded-lg hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors cursor-pointer">
          ›
        </button>
      </div>
    </div>
  `
})
export class PaginationComponent {
  currentPage = input(1);
  totalPages = input(1);
  totalCount = input(0);
  pageChange = output<number>();

  visiblePages = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  });
}
