# Sprint 2 — Stage 2: Application

## Objective

Connect the identity domain introduced in Stage 1 to application use cases without implementing provider-specific infrastructure or Web pages.

## Delivered

- Application contracts for UTC time, secure token generation/hashing, e-mail composition, and e-mail delivery.
- Commands, requests, handlers, and validators for:
  - email confirmation;
  - confirmation resend;
  - password reset request;
  - password reset completion.
- Automatic confirmation-token issuance after `CreateUser` and `CreateAccount`.
- Authentication blocked until the account email is confirmed.
- Generic password-reset behavior for unknown, inactive, or unconfirmed accounts.
- Previous active tokens revoked when a new token of the same purpose is issued.
- Password reset tokens are single-use and time limited.
- Current-user response now exposes `IsEmailConfirmed`.
- Application tests for confirmation, expiration, reuse, resend, reset, privacy, login blocking, and validation.

## Intentionally deferred

The following belong to Sprint 2 Stage 3 or Stage 4:

- cryptographically secure token implementation;
- SHA-256 or equivalent token hashing implementation;
- Resend integration;
- HTML templates and public URLs;
- configuration and secrets;
- confirmation and password-reset pages.
