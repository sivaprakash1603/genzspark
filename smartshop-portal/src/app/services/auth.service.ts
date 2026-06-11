import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {LoginModel} from "../models/login.model"

@Injectable({
  providedIn: 'root'
})
export class AuthService {
    constructor(private http: HttpClient) {}
        public login(LoginModel: LoginModel) {
            return this.http.post('https://dummyjson.com/auth/login', LoginModel);
        }
}

  