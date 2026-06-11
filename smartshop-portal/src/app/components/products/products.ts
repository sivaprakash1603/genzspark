import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Product, ProductService } from '../../services/product.service';

@Component({
  selector: 'app-products',
  imports: [AsyncPipe, CurrencyPipe, RouterLink],
  templateUrl: './products.html',
  styleUrl: './products.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Products {
  private readonly productService = inject(ProductService);

  protected readonly products$ = this.productService.getProducts();

  protected trackByProductId(_: number, product: Product): number {
    return product.id;
  }
}
