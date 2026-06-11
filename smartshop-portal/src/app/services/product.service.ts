import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class ProductService {
    constructor(private http: HttpClient) {}
        public getProducts() {
            return this.http.get('https://dummyjson.com/products');
        }
        public getProductById(id: number) {
            return this.http.get(`https://dummyjson.com/products/${id}`);
        }
}