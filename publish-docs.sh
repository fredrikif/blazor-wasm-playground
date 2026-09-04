#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
project_dir="$repo_root/blazor-wasm-playground"
publish_dir="$repo_root/docs"
tmp_dir="$repo_root/.publish-tmp"

rm -rf "$publish_dir"
rm -rf "$tmp_dir"
mkdir -p "$tmp_dir"

dotnet publish "$project_dir" -c Release -p:BaseHref=/blazor-wasm-playground/ -o "$tmp_dir"

mkdir -p "$publish_dir"
cp -a "$tmp_dir/wwwroot/." "$publish_dir/"
rm -rf "$tmp_dir"

# Ensure GitHub Pages uses the repo path as the base href.
perl -pi -e 's|<base href="\./" />|<base href="/blazor-wasm-playground/" />|' "$publish_dir/index.html"
cp "$publish_dir/index.html" "$publish_dir/404.html"

# Fail before deployment if the generated entrypoint references a missing asset.
while IFS= read -r asset; do
	[[ -f "$publish_dir/$asset" ]] || {
		echo "Missing published asset referenced by index.html: $asset" >&2
		exit 1
	}
done < <(grep -Eo '(href|src)="[^"]+"' "$publish_dir/index.html" | sed -E 's/^[^=]+="([^"]+)"/\1/' | grep '^_framework/')

touch "$publish_dir/.nojekyll"
echo "Published Blazor site to $publish_dir"
