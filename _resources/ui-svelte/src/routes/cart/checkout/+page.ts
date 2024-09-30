import {
	checkoutFormSchema,
	type CheckoutFormAddress,
	type CheckoutFormItem,
	type CheckoutFormSchema
} from '$lib/types/ShoppingCart';
import { superValidate } from 'sveltekit-superforms';
import { zod } from 'sveltekit-superforms/adapters';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ parent }) => {
	const data = await parent();

	const pre: CheckoutFormSchema = {
		items: data.cart.map<CheckoutFormItem>((cart) => ({
			itemId: cart.productId,
			quantity: cart.count
		})),
		deliveryAddress: {
			name: data.user.name
		} as CheckoutFormAddress,
		billingAddress: {
			name: data.user.name
		} as CheckoutFormAddress
	};
	const form = await superValidate(pre as CheckoutFormSchema, zod(checkoutFormSchema), {
		errors: false
	});
	return {
		user: data.user,
		form
	};
};
