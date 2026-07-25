# Sprint 2 — Stage 4: Web

Implemented anonymous identity pages for email confirmation, confirmation resend, password recovery and password reset. Registration now redirects to a pending-confirmation page, while login distinguishes invalid credentials from an unconfirmed account.

During development, when Resend is disabled, identity messages are captured as HTML and JSON files under `src/LevelUp.Web/Data/Emails`. Production delivery can later be enabled through the existing Resend configuration without changing application handlers or Web pages.

## Extension — confirmation states and mandatory first-login tutorial

The confirmation page now distinguishes successful confirmation, an already-used link,
an expired link, a revoked/replaced link, and an invalid or missing token. Reopening a
successfully consumed confirmation link directs the user to sign in instead of presenting
the account as invalid.

Login destination resolution is now centralized in `LoginDestinationResolver`. Character
creation and the onboarding tutorial take precedence over every return URL. Consequently,
a confirmed user with a character and `HasCompletedOnboarding == false` is always sent to
`/onboarding/tutorial` on the first successful login. Only users who completed onboarding
may follow a safe local return URL or continue to `/daily`.
