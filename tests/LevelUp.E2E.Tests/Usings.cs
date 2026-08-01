global using Xunit;

// Each test class launches its own Chromium instance and its own in-process Kestrel-hosted app
// (see PlaywrightAppFixture). Running multiple test classes concurrently — xunit's default —
// starves the machine (several browsers + web hosts at once) and causes real requests to exceed
// Playwright's timeouts, which looks like a login/UI bug but isn't one. E2E suites are inherently
// resource-heavy per test; running them sequentially is the correct default here, not a workaround.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
