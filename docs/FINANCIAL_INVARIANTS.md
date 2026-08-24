# Financial Invariants

These invariants are non-negotiable rules for implementation and testing.

1. An Order is never Paid solely because the client reports success.
2. A Payment is never Succeeded solely because checkout/session creation succeeded.
3. A successful financial state must be linked to a trusted provider-side transaction/event or a deliberately supported authoritative payment mechanism.
4. One application payment operation must not produce multiple successful provider charges because of retries.
5. One Stripe event ID is processed at most once for business effects.
6. A webhook may be delivered multiple times without duplicating financial effects.
7. Refund total cannot exceed captured/succeeded refundable amount.
8. Every refund has an immutable link to its original payment and order.
9. Monetary values are never represented as binary floating-point values.
10. Currency minor-unit conversion is currency-aware; zero-decimal currencies are not multiplied by 100.
11. Payment and order statuses cannot make invalid transitions.
12. Inventory cannot be deducted twice for one order/payment lifecycle.
13. Inventory reservation can be safely released after payment failure/expiration.
14. External Stripe IDs are retained for reconciliation and audit.
15. Financial history is not destroyed to represent a new state; status history/transactions provide an audit trail.
16. Reconciliation must be able to identify missing, duplicated, mismatched and stale payment/refund records.
17. Webhook processing failures must result in a non-2xx HTTP response when Stripe should retry, or a persisted retry job that has explicit ownership and semantics.
18. Secrets and raw card data are never stored in source control or the application database.
