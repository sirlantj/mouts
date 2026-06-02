import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SaleItem {
  id: string;
  productExternalId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  discount: number;
  totalAmount: number;
  isCancelled: boolean;
}

export interface Sale {
  id: string;
  saleNumber: string;
  saleDate: string;
  customerExternalId: string;
  customerName: string;
  branchExternalId: string;
  branchName: string;
  totalAmount: number;
  status: string;
  items: SaleItem[];
}

export interface SalesListResponse {
  data: Sale[];
  totalCount: number;
  currentPage: number;
  totalPages: number;
}

export interface CreateSaleItemRequest {
  productExternalId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface CreateSaleRequest {
  saleNumber: string;
  saleDate: string;
  customerExternalId: string;
  customerName: string;
  branchExternalId: string;
  branchName: string;
  items: CreateSaleItemRequest[];
}

export interface UpdateSaleRequest {
  customerExternalId: string;
  customerName: string;
  branchExternalId: string;
  branchName: string;
  items: CreateSaleItemRequest[];
}

export interface SalesQueryParams {
  _page?: number;
  _size?: number;
  _order?: string;
  CustomerName?: string;
  BranchName?: string;
  Status?: string;
  StartDate?: string;
  EndDate?: string;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({ providedIn: 'root' })
export class SalesService {
  private readonly url = `${environment.apiUrl}/sales`;

  constructor(private http: HttpClient) {}

  getSales(params: SalesQueryParams): Observable<ApiResponse<SalesListResponse>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<SalesListResponse>>(this.url, { params: httpParams });
  }

  getSale(id: string): Observable<ApiResponse<Sale>> {
    return this.http.get<ApiResponse<Sale>>(`${this.url}/${id}`);
  }

  createSale(sale: CreateSaleRequest): Observable<ApiResponse<{ id: string; saleNumber: string; totalAmount: number }>> {
    return this.http.post<ApiResponse<{ id: string; saleNumber: string; totalAmount: number }>>(this.url, sale);
  }

  updateSale(id: string, sale: UpdateSaleRequest): Observable<ApiResponse<{ id: string; saleNumber: string; totalAmount: number; status: string }>> {
    return this.http.put<ApiResponse<{ id: string; saleNumber: string; totalAmount: number; status: string }>>(`${this.url}/${id}`, sale);
  }

  cancelSale(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.url}/${id}`);
  }

  cancelItem(saleId: string, itemId: string): Observable<ApiResponse<{ saleId: string; itemId: string; newTotalAmount: number }>> {
    return this.http.patch<ApiResponse<{ saleId: string; itemId: string; newTotalAmount: number }>>(
      `${this.url}/${saleId}/items/${itemId}/cancel`, {}
    );
  }
}
