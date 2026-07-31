# Wallet

Wallet is a user-scoped personal finance module composed of Wallets, Transactions, and Wallet Tags. It represents exclusively the user's financial control — balance, income, expenses, transactions, and categorization.

## Wallet

A Wallet owns its transaction state and derives or protects balance according to domain rules. Transaction mutations must preserve wallet consistency.

## Transaction

Transactions have a type represented by `TransactionType` and participate in wallet balance behavior. Invalid amounts or state transitions are rejected by domain and application validation.

## Wallet tags

`WalletTag` provides user-defined classification for wallet records. Tag updates and deletion must preserve referential consistency in affected records.

## Application boundary

Wallet commands, queries, requests, responses, validators, and handlers live in the Application feature (`Features/Wallets`). Web owns forms, filters, loading, modals, responsive behavior, and feedback. Infrastructure persists the same user-scoped state through the repository.

## Isolation

Every wallet operation must be scoped to the authenticated user. Tests must cover cross-user access attempts for reads and mutations.
