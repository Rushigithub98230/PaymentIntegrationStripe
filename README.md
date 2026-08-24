# PaymentIntegrationStripe

Enterprise-style e-commerce reference application focused on reliable Stripe payment processing and financial consistency.

## Architecture

Angular → ASP.NET Core API → Application → Domain → Infrastructure → SQL Server

Payment providers are isolated behind `IPaymentGateway` so the payment domain is not coupled to Stripe SDK types.

## Primary Engineering Goals

- Correct money handling using decimal + integer minor units where appropriate
- Separate Order and Payment state machines
- Stripe PaymentIntent lifecycle
- Stripe webhook signature verification and idempotency
- Application and Stripe idempotency keys
- Full and partial refunds
- Immutable financial transaction history
- Payment audit timeline
- Inventory reservation and concurrency protection
- Background verification, retry, cleanup and reconciliation
- JWT authentication and role-based authorization
- Automated unit, integration and end-to-end test strategy

## Repository Status

This repository is intentionally being built incrementally. The first milestone establishes architecture, domain boundaries, payment state machines, persistence contracts and development standards before implementing checkout and Stripe infrastructure.

## Security

No Stripe secret, webhook secret, database password, JWT signing key or other credential belongs in source control. Development configuration must use local secret storage/environment variables.
