import { Subject } from "rxjs";

export const usernameSubject = new Subject<string|undefined>();

export const Logout = () => {
    sessionStorage.removeItem("token");
    usernameSubject.next(undefined);
}

export const ChangeUsername = (username: string) => {
    const token = sessionStorage.getItem("token");
    if(token){
        const name = JSON.parse(atob(token.split('.')[1])).firstName+" "+JSON.parse(atob(token.split('.')[1])).lastName;
        usernameSubject.next(username);
    }
}