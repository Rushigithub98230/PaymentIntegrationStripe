# Architecture

## Objective

Build a production-oriented e-commerce system whose payment and financial subsystem is the primary engineering concern.

## Layers

```text
Angular Frontend
       |
       v
ASP.NET Core API
       |
       v
Application
       |
       v
Domain
       |
       v
Infrastructure
       |
       v
SQL Server
```

## Payment Boundary

The application/domain layers depend on payment contracts, not Stripe SDK types:

```text
IPaymentGateway
    |
    +-- StripePaymentGateway
    +-- FuturePaymentGateway
```

Stripe-specific objects are translated at the infrastructure boundary into internal payment models and state transitions.

## Core Principles

1. Order state and Payment state are independent state machines.
2. The database is the source of truth for internal business state; Stripe is the external payment provider of record for provider-side financial state.
3. A frontend success callback never authorizes an order to become Paid.
4. Payment success must be established through trusted server-side/provider confirmation.
5. Stripe webhook processing is signature-verified, persisted, idempotent and retry-safe.
6. External provider IDs are immutable references and must be uniquely constrained where appropriate.
7. Financial history is append-oriented; important financial facts must not be overwritten.
8. Money uses decimal for business calculations and provider-specific integer minor units at API boundaries.
9. Inventory reservation/finalization is designed for concurrent checkouts and recovery from payment failures.
10. Reconciliation detects and repairs recoverable divergence between SQL Server and Stripe.

## Planned Runtime Flow

```text
Cart
 -> Checkout Validation
 -> Order + Payment Intent/Attempt
 -> Stripe PaymentIntent
 -> Customer Action / Stripe Processing
 -> Trusted Stripe Event
 -> Idempotent Event Processing
 -> Payment State Transition
 -> Order State Transition
 -> Inventory Finalization
 -> Invoice
 -> Audit Timeline
```

## Recovery

Background workers will handle stuck intermediate payments, webhook retries, stale inventory reservations, refund recovery and Stripe/SQL reconciliation. All worker operations must be safe to execute more than once.
