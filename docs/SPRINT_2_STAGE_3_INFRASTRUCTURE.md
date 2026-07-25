# Sprint 2 - Stage 3 - Infrastructure

## Scope

This stage implements the infrastructure behind the identity contracts introduced by Stage 2.

## Implemented

- `SystemClock` as the UTC clock implementation.
- Cryptographically secure 256-bit URL-safe user tokens.
- SHA-256 token hashing before persistence.
- Identity email URL composition and HTML templates.
- Resend HTTP API integration through `HttpClient`.
- Bearer authentication, User-Agent and idempotency headers.
- Configuration validation for public URLs and Resend credentials.
- Disabled email mode for local development without committed secrets.
- Infrastructure tests for token generation, hashing, links, HTML encoding and email delivery behavior.

## Configuration

The committed configuration keeps email delivery disabled:

```json
"LevelUp": {
  "IdentityEmail": {
    "PublicBaseUrl": "https://localhost:5001",
    "ConfirmationPath": "/account/confirm-email",
    "PasswordResetPath": "/account/reset-password"
  },
  "Email": {
    "Resend": {
      "Enabled": false,
      "ApiKey": "",
      "FromName": "LevelUp",
      "FromAddress": ""
    }
  }
}
```

For local testing, use .NET user secrets instead of editing tracked configuration:

```bash
dotnet user-secrets set "LevelUp:Email:Resend:Enabled" "true" --project src/LevelUp.Web
dotnet user-secrets set "LevelUp:Email:Resend:ApiKey" "re_xxxxxxxxx" --project src/LevelUp.Web
dotnet user-secrets set "LevelUp:Email:Resend:FromAddress" "noreply@your-verified-domain.com" --project src/LevelUp.Web
dotnet user-secrets set "LevelUp:IdentityEmail:PublicBaseUrl" "https://your-public-url" --project src/LevelUp.Web
```

The sender domain must be verified in Resend before production delivery.

## Deferred

The Web confirmation, resend, forgot-password and reset-password pages remain part of Stage 4.
