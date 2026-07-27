# Inventory

Inventory is a user-scoped productivity finance module composed of Wallets, Transactions, and Inventory Tags.

## Wallet

A Wallet owns its transaction state and derives or protects balance according to domain rules. Transaction mutations must preserve wallet consistency.

## Transaction

Transactions have a type represented by `TransactionType` and participate in wallet balance behavior. Invalid amounts or state transitions are rejected by domain and application validation.

## Inventory tags

`InventoryTag` provides user-defined classification for inventory records. Tag updates and deletion must preserve referential consistency in affected records.

## Application boundary

Inventory commands, queries, requests, responses, validators, and handlers live in the Application feature. Web owns forms, filters, loading, modals, responsive behavior, and feedback. Infrastructure persists the same user-scoped state through the repository.

## Isolation

Every inventory operation must be scoped to the authenticated user. Tests must cover cross-user access attempts for reads and mutations.
