# blazor-wasm-playground

This repository hosts a Blazor WebAssembly app published to GitHub Pages from the `docs/` folder.

## Local development

From the project root:

```bash
cd blazor-wasm-playground
dotnet watch run
```

The app is configured with a relative base URL for local development.

## Publish to GitHub Pages

GitHub Actions publishes the app automatically whenever `main` is pushed. In
the repository settings, set **Pages > Build and deployment > Source** to
**GitHub Actions**.

To build the Pages artifact locally, run this from the repository root:

```bash
./publish-docs.sh
```

The script publishes the Blazor app in release mode and updates `docs/` for GitHub Pages.

The script validates that every framework asset referenced by the generated
HTML is present before the artifact is uploaded.
