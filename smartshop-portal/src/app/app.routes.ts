import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
	{
		path: '',
		pathMatch: 'full',
		redirectTo: 'login',
	},
	{
		path: 'login',
		loadComponent: () => import('./components/login/login').then((m) => m.Login),
	},
	{
		path: 'dashboard',
		canActivate: [authGuard],
		loadComponent: () => import('./components/dashboard/dashboard').then((m) => m.Dashboard),
	},
	{
		path: 'products',
		canActivate: [authGuard],
		loadComponent: () => import('./components/products/products').then((m) => m.Products),
	},
	{
		path: 'products/:id',
		canActivate: [authGuard],
		loadComponent: () => import('./components/product-details/product-details').then((m) => m.ProductDetails),
	},
	{
		path: 'profile',
		canActivate: [authGuard],
		loadComponent: () => import('./components/profile/profile').then((m) => m.Profile),
	},
	{
		path: '**',
		redirectTo: 'login',
	},
];
