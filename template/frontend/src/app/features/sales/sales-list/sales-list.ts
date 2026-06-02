import { Component, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { SalesService, Sale, SalesQueryParams } from '../../../core/services/sales.service';
import { PaginationComponent } from '../../../shared/pagination/pagination';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [RouterLink, FormsModule, DatePipe, CurrencyPipe, PaginationComponent, ConfirmDialogComponent],
  templateUrl: './sales-list.html',
  styleUrl: './sales-list.scss'
})
export class SalesListComponent implements OnInit {
  sales = signal<Sale[]>([]);
  loading = signal(false);
  totalCount = signal(0);
  currentPage = signal(1);
  totalPages = signal(1);
  pageSize = 10;

  filterCustomer = '';
  filterBranch = '';
  filterStatus = '';
  sortOrder = 'saleDate desc';

  confirmOpen = signal(false);
  confirmMessage = signal('');
  private cancelId = '';

  constructor(private salesService: SalesService, private router: Router) {}

  ngOnInit() {
    this.loadSales();
  }

  loadSales() {
    this.loading.set(true);
    const params: SalesQueryParams = {
      _page: this.currentPage(),
      _size: this.pageSize,
      _order: this.sortOrder || undefined,
      CustomerName: this.filterCustomer || undefined,
      BranchName: this.filterBranch || undefined,
      Status: this.filterStatus || undefined
    };

    this.salesService.getSales(params).subscribe({
      next: (res) => {
        // The unwrap interceptor already flattened the envelope: res.data is SalesListResponse.
        this.sales.set(res.data.data);
        this.totalCount.set(res.data.totalCount);
        this.currentPage.set(res.data.currentPage);
        this.totalPages.set(res.data.totalPages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPageChange(page: number) {
    this.currentPage.set(page);
    this.loadSales();
  }

  onFilter() {
    this.currentPage.set(1);
    this.loadSales();
  }

  onSort(field: string) {
    if (this.sortOrder === `${field} asc`) {
      this.sortOrder = `${field} desc`;
    } else {
      this.sortOrder = `${field} asc`;
    }
    this.loadSales();
  }

  getSortIcon(field: string): string {
    if (this.sortOrder === `${field} asc`) return '▲';
    if (this.sortOrder === `${field} desc`) return '▼';
    return '';
  }

  askCancel(sale: Sale) {
    this.cancelId = sale.id;
    this.confirmMessage.set(`Cancel sale ${sale.saleNumber}?`);
    this.confirmOpen.set(true);
  }

  onConfirmCancel() {
    this.confirmOpen.set(false);
    this.salesService.cancelSale(this.cancelId).subscribe({
      next: () => this.loadSales()
    });
  }

  onCancelDialog() {
    this.confirmOpen.set(false);
  }
}
