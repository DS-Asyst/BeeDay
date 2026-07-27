# Configuration and Secrets

Configuration is composed from `appsettings.json`, environment-specific settings, environment variables, and user secrets.

## Rules

- Never commit API keys, passwords, tokens, production hostnames that must remain private, or secret connection material.
- Use ASP.NET Core user secrets for local sensitive values.
- Use GitHub Environment secrets or server environment variables for deployment.
- Keep production runtime paths outside the application publish directory.
- Production must use explicit allowed hosts and HTTPS public URLs.

## Main configuration areas

- JSON storage directories and backup behavior;
- identity-email public URL, token lifetime, and sender mode;
- Resend API and verified sender;
- production hosting and Data Protection key paths;
- forwarded headers and trusted proxies or networks;
- development generated-email output;
- logging and health behavior.

Review `src/LevelUp.Web/appsettings*.json` and the typed options under Infrastructure and Web before introducing a new configuration key.
