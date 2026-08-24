# Payment State Machine

## Internal Payment Status

The application owns its internal payment status. Stripe statuses are mapped at the infrastructure boundary and must not leak through the domain.

```text
Created
  |
  v
Pending
  |
  +----------------------+
  |                      |
  v                      v
Processing          RequiresAction
  |                      |
  |                      v
  |                 Processing
  |                      |
  +----------+-----------+
             |
             v
          Succeeded

Pending/Processing --> Failed
Pending/Processing --> Cancelled

Succeeded --> RefundPending --> PartiallyRefunded --> Refunded
```

## Rules

- Only valid transitions are accepted.
- A successful frontend callback cannot transition a payment to Succeeded.
- Payment success requires trusted server-side confirmation.
- A payment cannot be refunded beyond its captured/succeeded refundable amount.
- Partial refunds accumulate against the immutable captured amount.
- Duplicate provider events do not create additional transitions or financial transactions.
- Failed/cancelled payments do not finalize inventory.
- Successful payment finalization and inventory finalization must be designed for retries and partial failure.

## Order State

Order state is intentionally separate:

```text
PendingPayment -> Paid -> Processing -> Packed -> Shipped -> Delivered
       |             |
       v             v
   Cancelled     Returned / PartiallyRefunded / Refunded
```

Payment state changes do not blindly overwrite order state. Application services decide whether a payment event authorizes a corresponding order transition.
