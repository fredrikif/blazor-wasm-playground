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

Run this from the repository root:

```bash
./publish-docs.sh
```

The script publishes the Blazor app in release mode and updates `docs/` for GitHub Pages.

After publishing, commit and push the generated files:

```bash
git add docs
git commit -m "Publish updated Blazor app to GitHub Pages"
git push
```
