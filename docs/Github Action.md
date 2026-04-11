## Automatisches Build + GitHub Release

- baut `CodyNET.Frontend` im Release-Modus,
- veröffentlicht self-contained Single-File-Builds für:
    - `win-x64`
    - `linux-x64`
    - `osx-arm64`
- packt die Ergebnisse als `.zip` (Windows) bzw. `.tar.gz` (Linux),
- hängt die Dateien automatisch an das GitHub Release an.

### Release auslösen
1. Änderung der version.json
2. Committen and pushen

Danach erzeugt GitHub Actions den Build und veröffentlicht die Assets im passenden Release.