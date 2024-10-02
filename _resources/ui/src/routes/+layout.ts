export const ssr = false;
export const trailingSlash = 'always';

import { getMe } from '$lib/api/auth/getMe';
import { getShoppingCart } from '$lib/api/cart/getShoppingCart';
import type { User } from '$lib/types/User';
import type { LayoutLoad } from './$types';

export const load: LayoutLoad = async ({ fetch, depends }) => {
	let name: string | undefined;
	try {
		name = await getMe(fetch);
	} catch (error) {
		console.error(error);
	}

	const cart = await getShoppingCart(fetch);
	depends('app:cart');

	return {
		user: {
			name,
			isAuthenticated: !!name
		} as User,
		cart
	};
};
