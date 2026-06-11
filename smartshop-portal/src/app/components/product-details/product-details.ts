import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { filter, map, switchMap } from 'rxjs';
import { Product, ProductService } from '../../services/product.service';

@Component({
  selector: 'app-product-details',
  imports: [AsyncPipe, CurrencyPipe, RouterLink],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductDetails {
  private readonly route = inject(ActivatedRoute);
  private readonly productService = inject(ProductService);

  protected readonly product$ = this.route.paramMap.pipe(
    map((params) => Number(params.get('id'))),
    filter((id) => !Number.isNaN(id)),
    switchMap((id) => this.productService.getProductById(id))
  );

  protected isProduct(product: Product | null): product is Product {
    return product !== null;
  }
}
