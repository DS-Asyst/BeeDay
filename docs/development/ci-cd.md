# CI/CD and IIS Deployment

## Continuous integration

`.github/workflows/ci.yml` validates supported pushes and pull requests by restoring dependencies, verifying formatting, building in Release, running tests, publishing the Web project, validating the artifact, and uploading results.

A formatting, build, test, publish, or artifact-validation failure fails the workflow.

## Production deployment

`.github/workflows/deploy-prd.yml` validates on a GitHub-hosted runner and deploys the exact validated artifact through a controlled self-hosted Windows runner. Production does not rebuild source code.

The deployment script:

1. validates the artifact;
2. creates and validates external runtime directories;
3. backs up the current application and data;
4. stops IIS;
5. configures required environment values;
6. replaces application binaries only;
7. starts IIS;
8. checks readiness with retries;
9. restores the prior application when readiness fails.

## Required safeguards

- Protect the GitHub `production` Environment with reviewer approval.
- Restrict production deployment to `prd`.
- Do not execute untrusted pull-request code on the self-hosted runner.
- Keep the runner dedicated or tightly controlled.
- Grant only the IIS and filesystem permissions required by the deployment script.
- Preserve `C:\Apps\LevelUp-Data` across deployments and rollbacks.
