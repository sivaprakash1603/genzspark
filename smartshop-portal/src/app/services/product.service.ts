import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
    private readonly http = inject(HttpClient);

    getProducts(): Observable<Product[]> {
        return this.http.get<ProductsResponse>('https://dummyjson.com/products').pipe(
            map((response) => response.products),
            catchError((error: HttpErrorResponse) => {
                console.error('Failed to load products', error);
                return of([]);
            })
        );
    }

    getProductById(id: number): Observable<Product | null> {
        return this.http.get<Product>(`https://dummyjson.com/products/${id}`).pipe(
            catchError((error: HttpErrorResponse) => {
                console.error(`Failed to load product ${id}`, error);
                return of(null);
            })
        );
    }
}

export interface ProductsResponse {
    products: Product[];
    total: number;
    skip: number;
    limit: number;
}

export interface Product {
    id: number;
    title: string;
    description: string;
    category: string;
    brand?: string;
    price: number;
    rating: number;
    thumbnail: string;
    images?: string[];
}