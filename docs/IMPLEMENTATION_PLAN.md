# Implementation Plan

## Phase 0 — Foundation

- Repository standards and documentation
- Backend solution and clean architecture projects
- Angular workspace
- Shared configuration conventions
- Global exception handling
- Structured logging and correlation IDs
- JWT authentication/authorization foundation
- Swagger/OpenAPI
- SQL Server + EF Core foundation
- Health checks
- CI build/test pipeline

## Phase 1 — Domain and Persistence

- Users/roles/permissions
- Products/categories/variants
- Pricing/coupons
- Addresses/cart
- Inventory and concurrency model
- Orders/order items
- Payment aggregate and state history
- Payment attempts/transactions
- Refund aggregate and refund attempts
- Stripe customer/payment-method mappings
- Webhook event persistence
- Invoices/billing records
- Financial audit records
- Reconciliation records
- Database indexes and unique constraints
- EF Core migrations

## Phase 2 — Catalog, Cart and Inventory

- Customer product browsing/search/filter/sort
- Admin catalog management
- Inventory reservation
- Reservation expiry/release
- Concurrent checkout protection
- Cart lifecycle

## Phase 3 — Checkout and Payment Abstraction

- Checkout pricing snapshot
- Server-side price/tax/discount/shipping calculation
- Order creation with pending-payment state
- Payment creation
- Idempotency key enforcement
- IPaymentGateway contract
- Internal Stripe-independent payment models
- StripePaymentGateway
- PaymentIntent creation and confirmation support
- Provider metadata/correlation IDs

## Phase 4 — Stripe Webhooks and Financial Consistency

- Raw-body signature verification
- Event persistence before processing
- Atomic/unique event claiming
- Idempotent event handlers
- PaymentIntent lifecycle handlers
- Charge handlers where required
- Checkout events where required
- Trusted payment confirmation
- Order/payment transition coordination
- Inventory finalization
- Webhook retry strategy
- Correct HTTP status responses to Stripe

## Phase 5 — Refunds and Billing

- Full refunds
- Partial refunds
- Multiple partial refunds
- Refund eligibility and remaining refundable amount
- Refund state machine
- Stripe refund integration
- Refund webhooks
- Refund retry/recovery
- Billing records
- Invoice numbering and generation
- Invoice history/download

## Phase 6 — Reconciliation and Workers

- Pending payment verification worker
- Webhook retry worker
- Stale payment/inventory cleanup worker
- Refund recovery worker
- Stripe/SQL reconciliation worker
- Reconciliation admin UI
- Mismatch resolution with audit trail

## Phase 7 — Admin and Customer UI

- Customer checkout/payment experience
- Payment result/status page based on server state
- Order/payment/refund/invoice history
- Admin payments and transactions
- Payment timeline
- Webhook event inspection
- Refund management
- Audit logs
- Reconciliation dashboard

## Phase 8 — Hardening and Verification

- Unit tests for state machines and money calculations
- Integration tests with SQL Server
- Stripe test-mode integration tests/mocks
- Webhook duplicate/retry tests
- Payment idempotency tests
- Refund boundary tests
- Concurrent inventory tests
- Reconciliation tests
- End-to-end critical payment scenarios
- Security review
- Configuration/secret review
- CI verification
- Final architecture and operational documentation

## Definition of Done

No milestone is considered complete solely because the happy path works. Each payment capability must also define its failure, retry, duplicate, timeout, webhook-delay and reconciliation behavior before it is considered production-oriented.
