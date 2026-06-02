import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe, PercentPipe } from '@angular/common';
import { SalesService, Sale } from '../../../core/services/sales.service';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-sales-detail',
  standalone: true,
  imports: [RouterLink, DatePipe, CurrencyPipe, PercentPipe, ConfirmDialogComponent],
  templateUrl: './sales-detail.html',
  styleUrl: './sales-detail.scss'
})
export class SalesDetailComponent implements OnInit {
  sale = signal<Sale | null>(null);
  loading = signal(true);
  error = signal('');

  confirmOpen = signal(false);
  confirmTitle = signal('');
  confirmMessage = signal('');
  private pendingAction: (() => void) | null = null;

  constructor(
    private salesService: SalesService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadSale();
  }

  private get saleId(): string {
    return this.route.snapshot.params['id'];
  }

  private loadSale() {
    this.loading.set(true);
    this.salesService.getSale(this.saleId).subscribe({
      next: (res) => {
        this.sale.set(res.data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Sale not found');
        this.loading.set(false);
      }
    });
  }

  askCancelSale() {
    this.confirmTitle.set('Cancel sale');
    this.confirmMessage.set(`Cancel sale ${this.sale()?.saleNumber}? All items will be cancelled.`);
    this.pendingAction = () => {
      this.salesService.cancelSale(this.saleId).subscribe({
        next: () => this.loadSale()
      });
    };
    this.confirmOpen.set(true);
  }

  askCancelItem(itemId: string, productName: string) {
    this.confirmTitle.set('Cancel item');
    this.confirmMessage.set(`Cancel item "${productName}"?`);
    this.pendingAction = () => {
      this.salesService.cancelItem(this.saleId, itemId).subscribe({
        next: () => this.loadSale()
      });
    };
    this.confirmOpen.set(true);
  }

  onConfirm() {
    this.confirmOpen.set(false);
    this.pendingAction?.();
    this.pendingAction = null;
  }

  onCancelDialog() {
    this.confirmOpen.set(false);
    this.pendingAction = null;
  }
}
