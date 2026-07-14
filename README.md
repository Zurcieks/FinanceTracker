# Finance Tracker API

Backend API for a personal finance tracking application. The project exposes a
.NET Minimal API for managing categories, transactions, dashboard summaries,
currency conversion and AI-assisted receipt scanning.

The current scope is a single-user backend, so authentication is not
included by design.

## Features

- **Categories** with create, read, update, archive and restore operations
- **Transactions** with filtering, search, pagination and receipt attachment
- **Dashboard summaries** for balance, category spending and monthly spending
- **Currency conversion** using the Polish National Bank (NBP) API
- **Receipt scanning** using an OpenAI vision model
- **Receipt storage** using S3-compatible object storage, tested locally with MinIO
- **Short-lived presigned URLs** for receipt access
- **Validation pipeline** using FluentValidation endpoint filters
- **Integration tests** running against a real PostgreSQL database with Testcontainers

## Tech Stack

- .NET 10
- ASP.NET Core Minimal API
- Entity Framework Core
- PostgreSQL
- Docker Compose
- Testcontainers
- FluentValidation
- Serilog
- OpenAI API
- Amazon S3 SDK / MinIO
- Scalar API Reference

## Architecture

The API follows **Vertical Slice Architecture**. Each feature keeps its endpoint,
request/response models and validation close together instead of spreading logic
across broad horizontal layers.

```text
api/
  Features/
    Categories/
    Transactions/
    Dashboard/
    Receipts/
    ExchangeRates/
  Infrastructure/
  Domain/
  Common/
```

This keeps the project easy to navigate and makes feature changes local to a
single area of the codebase.

The project intentionally does not use CQRS or MediatR. For this size of API, a
mediator pipeline would add indirection without solving a real problem. The code
keeps handlers explicit and validation is handled through endpoint filters.

## API Areas

### Categories

Categories can be created, updated, archived and restored. They are archived
instead of hard-deleted so historical transactions remain consistent.

### Transactions

Transactions support filtering, search and pagination. When a transaction is
saved, its PLN value is calculated once and stored as a snapshot.

### Dashboard

The dashboard endpoints expose aggregated financial data, including balance
summary, category spending and monthly spending.

### Exchange Rates

Currency conversion uses the NBP API. Exchange rates are used when a transaction
is saved, not on every read, so historical transaction values do not change when
current rates move.

### Receipts

Receipts are stored in S3-compatible object storage. The API stores object keys in
the database and generates short-lived presigned URLs when receipts are requested.

### AI Receipt Scanning

Receipt scanning uses an OpenAI vision model to extract a draft from a receipt
image. The API treats model output as a suggestion, not as trusted business data.

The scanning flow validates the uploaded file, asks the model for structured JSON,
safely parses supported enum values and accepts suggested category IDs only when
they match active categories from the database. The result is returned as a draft
for user confirmation.
