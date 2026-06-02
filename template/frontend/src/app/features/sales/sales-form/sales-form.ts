import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CurrencyPipe, PercentPipe } from '@angular/common';
import { SalesService } from '../../../core/services/sales.service';

@Component({
  selector: 'app-sales-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, PercentPipe],
  templateUrl: './sales-form.html',
  styleUrl: './sales-form.scss'
})
export class SalesFormComponent implements OnInit {
  form!: FormGroup;
  isEdit = signal(false);
  saleId = '';
  loading = signal(false);
  saving = signal(false);
  error = signal('');

  constructor(
    private fb: FormBuilder,
    private salesService: SalesService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.buildForm();
    this.saleId = this.route.snapshot.params['id'] || '';

    if (this.saleId) {
      this.isEdit.set(true);
      this.loadSale();
    }
  }

  private buildForm() {
    this.form = this.fb.group({
      saleNumber: ['', Validators.required],
      saleDate: [new Date().toISOString().slice(0, 10), Validators.required],
      customerExternalId: ['', Validators.required],
      customerName: ['', Validators.required],
      branchExternalId: ['', Validators.required],
      branchName: ['', Validators.required],
      items: this.fb.array([])
    });
    this.addItem();
  }

  get items(): FormArray {
    return this.form.get('items') as FormArray;
  }

  addItem() {
    this.items.push(this.fb.group({
      productExternalId: ['', Validators.required],
      productName: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1), Validators.max(20)]],
      unitPrice: [0, [Validators.required, Validators.min(0.01)]]
    }));
  }

  removeItem(index: number) {
    if (this.items.length > 1) {
      this.items.removeAt(index);
    }
  }

  getDiscount(quantity: number): number {
    if (quantity >= 10 && quantity <= 20) return 0.2;
    if (quantity >= 4) return 0.1;
    return 0;
  }

  getItemTotal(index: number): number {
    const item = this.items.at(index);
    const qty = item.get('quantity')?.value || 0;
    const price = item.get('unitPrice')?.value || 0;
    const discount = this.getDiscount(qty);
    return qty * price * (1 - discount);
  }

  getQuantityError(quantity: number): string | null {
    if (quantity > 20) return 'Máximo 20 itens por produto';
    return null;
  }

  getGrandTotal(): number {
    let total = 0;
    for (let i = 0; i < this.items.length; i++) {
      total += this.getItemTotal(i);
    }
    return total;
  }

  private loadSale() {
    this.loading.set(true);
    this.salesService.getSale(this.saleId).subscribe({
      next: (res) => {
        const sale = res.data;
        this.form.patchValue({
          saleNumber: sale.saleNumber,
          saleDate: sale.saleDate.slice(0, 10),
          customerExternalId: sale.customerExternalId,
          customerName: sale.customerName,
          branchExternalId: sale.branchExternalId,
          branchName: sale.branchName
        });

        this.items.clear();
        for (const item of sale.items.filter(i => !i.isCancelled)) {
          this.items.push(this.fb.group({
            productExternalId: [item.productExternalId, Validators.required],
            productName: [item.productName, Validators.required],
            quantity: [item.quantity, [Validators.required, Validators.min(1), Validators.max(20)]],
            unitPrice: [item.unitPrice, [Validators.required, Validators.min(0.01)]]
          }));
        }
        if (this.items.length === 0) this.addItem();
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Erro ao carregar venda');
        this.loading.set(false);
      }
    });
  }

  /** YYYY-MM-DD -> ISO 8601 UTC ("YYYY-MM-DDT00:00:00.000Z") */
  private toIsoUtc(date: string): string {
    if (!date) return new Date().toISOString();
    // Se ja vier ISO completo (com T), so garante o Z
    if (date.includes('T')) {
      return date.endsWith('Z') ? date : date + 'Z';
    }
    return `${date}T00:00:00.000Z`;
  }

  onSubmit() {
    if (this.form.invalid) return;

    this.saving.set(true);
    this.error.set('');
    const val = this.form.value;

    if (this.isEdit()) {
      this.salesService.updateSale(this.saleId, {
        customerExternalId: val.customerExternalId,
        customerName: val.customerName,
        branchExternalId: val.branchExternalId,
        branchName: val.branchName,
        items: val.items
      }).subscribe({
        next: () => this.router.navigate(['/sales', this.saleId]),
        error: (err) => {
          this.saving.set(false);
          this.error.set(err.error?.message || 'Erro ao atualizar venda');
        }
      });
    } else {
      this.salesService.createSale({
        saleNumber: val.saleNumber,
        // input type="date" devolve YYYY-MM-DD; backend espera ISO 8601 com timezone
        saleDate: this.toIsoUtc(val.saleDate),
        customerExternalId: val.customerExternalId,
        customerName: val.customerName,
        branchExternalId: val.branchExternalId,
        branchName: val.branchName,
        items: val.items
      }).subscribe({
        next: (res) => this.router.navigate(['/sales', res.data.id]),
        error: (err) => {
          this.saving.set(false);
          this.error.set(err.error?.message || 'Erro ao criar venda');
        }
      });
    }
  }
}
