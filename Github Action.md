## Automatisches Build + GitHub Release

- baut `CodyNET.Frontend` im Release-Modus,
- veröffentlicht self-contained Single-File-Builds für:
    - `win-x64`
    - `linux-x64`
- packt die Ergebnisse als `.zip` (Windows) bzw. `.tar.gz` (Linux),
- hängt die Dateien automatisch an das GitHub Release an.

### Release auslösen
1. Commit + Push der Änderungen~~~~
2. Tag erstellen und pushen:

```bash
git tag v1.0.0
git push origin v1.0.0
```

Danach erzeugt GitHub Actions den Build und veröffentlicht die Assets im passenden Release.